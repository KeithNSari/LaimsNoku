using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.PaymentCollection.Pages
{
    public class PolicyDetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBilledPremiumRepository _billedPremiumRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly ISuspenseProcessing _suspenseProcessing;
        [BindProperty]
        public PolicyDetailedBalance PolicyDetailedBalance { get; set; }
        [BindProperty]
        public int BillID { get; set; }
        [BindProperty]
        public int SuspenseHeaderID  { get; set; }
        [BindProperty]
        public decimal SuspenseHeaderBalance { get; set; }
        [BindProperty]
        public string PolicyNo { get; set; }
        [BindProperty]
        public decimal AmountToAllocate { get; set; }
        public DataTable BillDT { get; set; }
        [BindProperty]
        public PremiumHeader PremiumHeader { get; set; }
        [BindProperty]
        public PremiumLine PremiumLine { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        public PolicyDetailsModel(UserManager<ApplicationUser> userManager,
            IPolicyRepository policyRepository,ISuspenseProcessing suspenseProcessing,
            IBilledPremiumRepository billedPremiumRepository)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _billedPremiumRepository = billedPremiumRepository;
            _suspenseProcessing = suspenseProcessing;
        }
        public void OnGet(int sphid, string policyno, decimal spbal, decimal amnt)
        {
            SuspenseHeaderID = sphid;
            SuspenseHeaderBalance = spbal;
            AmountToAllocate = amnt;
            PolicyNo = policyno;
            PolicyDetailedBalance = _billedPremiumRepository.GetDetailedBalance(policyno);
            BillDT = _billedPremiumRepository.GetFirstUnpaid(policyno);
            if(BillDT != null) 
            {
                if(BillDT.Rows.Count>0)
                {
                    BillID = Convert.ToInt32(BillDT.Rows[0]["BillID"]);
                }
            }
        }
        public IActionResult OnPost()
        {
            ReturnUrl = Request.Path + Request.QueryString;
            //try
            //{ 
                using (TransactionScope TS = new TransactionScope())
                {
                    Guid PolicyID = _policyRepository.GetPolicyID(PolicyNo);
                    string addedBy = _userManager.GetUserId(User).ToString();
                    decimal invoiceAmount = _billedPremiumRepository.GetBillAmount(BillID);
                    decimal suspenseAmount = 0;
                    if (invoiceAmount <= AmountToAllocate)
                    {
                        decimal fullAmount = AmountToAllocate;
                        PremiumHeader.BillingID = BillID;
                        PremiumHeader.DatePaymentReceived = DateTime.Now;
                        PremiumHeader.TotalAmount = invoiceAmount;
                        suspenseAmount = fullAmount - invoiceAmount;
                        PremiumHeader.AddedBy = addedBy;
                        int premiumHeaderID = _billedPremiumRepository.AddPremiumHeader(PremiumHeader);
                        PremiumLine.PremiumHeaderID = premiumHeaderID;
                        PremiumLine.PaymentMethodID = 3;
                        PremiumLine.PaymentProviderID = 0;
                        PremiumLine.Amount = PremiumHeader.TotalAmount;
                        _billedPremiumRepository.AddPremiumLine(PremiumLine);
                        _billedPremiumRepository.UpdateBilledPremiums(PremiumHeader.BillingID);
                        _billedPremiumRepository.UpdateBillingHeader(PremiumHeader.BillingID);
                        _billedPremiumRepository.PremiumBreakDown(premiumHeaderID, PremiumHeader.TotalAmount, PolicyID, PremiumLine.CurrencyID);
                        _billedPremiumRepository.UpdatePolicyBalance(PolicyID, -PremiumHeader.TotalAmount);
                    }
                    else if (invoiceAmount > AmountToAllocate)
                    {
                        //insufficient balance send whole amount to policy suspense
                        suspenseAmount = AmountToAllocate;
                    }
                    //Update the source suspense
                    _suspenseProcessing.UpdateSuspenseHeaderBalance(SuspenseHeaderID, -AmountToAllocate); 
                    SuspenseLine sourceSuspenseLine = new()
                    {
                        HeaderID = SuspenseHeaderID,
                        Credit = 0,
                        Debit = AmountToAllocate,
                        AddedOn = DateTime.Now,
                        AddedBy = addedBy
                    };
                    _suspenseProcessing.AddSuspenseLine(sourceSuspenseLine);
                    if (suspenseAmount > 0)
                    {
                        int proposerID = _policyRepository.GetProposerID(PolicyNo); //change to use PolicyNo not search term
                        SuspenseHeader suspenseHeader = new()
                        {
                            MemberID = proposerID,
                            PaymentMethodID = 3,
                            PaymentTypeID = 0,
                            //Source = PaymentSource,
                            InternalBankAccountID = 0,
                            SuspenseType = 2,
                            PolicyID = PolicyID,
                            PaymentDate = PremiumHeader.DatePaymentReceived,
                            Reference = PremiumLine.Reference,
                            StatusID = 0,
                            ProcessedAmount = 0,
                            CurrencyID = PremiumLine.CurrencyID,
                            Balance = suspenseAmount,
                            AddedOn = DateTime.Now,
                            AddedBy = addedBy
                        };
                        int id = _suspenseProcessing.AddSuspenseHeader(suspenseHeader);
                        SuspenseLine suspenseLine = new()
                        {
                            HeaderID = id,
                            Credit = suspenseAmount,
                            Debit = 0,
                            AddedOn = DateTime.Now,
                            AddedBy = addedBy
                        };
                        _suspenseProcessing.AddSuspenseLine(suspenseLine);
                    }                    
                    TS.Complete();
                }
                return Redirect(ReturnUrl);
            //}
            //catch (Exception Ex)
            //{
            //    return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + Ex.Message, returnUrl = ReturnUrl });
            //}
        }
    }
}
