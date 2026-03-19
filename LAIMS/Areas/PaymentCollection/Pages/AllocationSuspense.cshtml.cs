using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Transactions;
using LAIMS.Models.Security;


namespace LAIMS.Areas.PaymentCollection.Pages
{
    [Authorize(Roles = "Suspense Reconcilliation")]
    public class AllocationSuspenseModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBilledPremiumRepository _billedPremiumRepository;
        private readonly ISuspenseProcessing _suspenseProcessing;
        private readonly IReversalHeaderRepository _reversalHeaderRepository;
        private readonly IProcessPayments _processPayments;
        [BindProperty]
        public int SearchCriteria { get; set; }
        public DataTable PaymentsDT;
        public string SearchPageUrl;
        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public int ReversalReason { get; set; }
        [BindProperty]
        public string ReversalComment { get; set; }
        [BindProperty]
        public int BillID { get; set; }
        [BindProperty]
        public int PremiumID { get; set; }
        [BindProperty]
        public PolicyDetailedBalance PolicyDetailedBalance { get; set; }
        [BindProperty ]
        public string CorrectPolicyNo { get; set; }
        [BindProperty]
        public SuspenseHeader SuspenseHeader { get; set; }
        public AllocationSuspenseModel(UserManager<ApplicationUser> userManager, 
            IBilledPremiumRepository billedPremiumRepository,
           IWebHostEnvironment webHostEnvironment, ISuspenseProcessing suspenseProcessing, 
           IReversalHeaderRepository reversalHeaderRepository, IProcessPayments processPayments)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _billedPremiumRepository = billedPremiumRepository;
            _suspenseProcessing = suspenseProcessing;
            _processPayments = processPayments;
            _reversalHeaderRepository = reversalHeaderRepository;
        }
        public void OnGet()
        {
            PaymentsDT = _billedPremiumRepository.GetLatestAllocationSuspenseEntries(); 
            ResultsCount = PaymentsDT.Rows.Count;
        }
        public void OnGetSearch(string? search)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            SearchTerm = search;
            SearchPageUrl = "AllocationSuspense";
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                string format = "yyyy-MM-dd";
                if (DateTime.TryParseExact(SearchTerm, format, null, System.Globalization.DateTimeStyles.None, out DateTime PaymentDate))
                {
                    SearchCriteria = 0; //searching by date
                    PaymentsDT = _billedPremiumRepository.SearchAllocationSuspenseEntries(PaymentDate);                   
                }
                else
                {
                    SearchCriteria = 1; //Search by PolicyNo
                    PaymentsDT = _billedPremiumRepository.SearchAllocationSuspenseEntries(SearchTerm); 
                }
                ResultsCount = PaymentsDT.Rows.Count;
                if (ResultsCount > 0)
                {
                    string policyNo = PaymentsDT.Rows[0]["Policies"].ToString();
                    PolicyDetailedBalance = _billedPremiumRepository.GetDetailedBalance(policyNo);
                }                
            }
        }
        public IActionResult OnPost()
        {
            using (TransactionScope TS = new TransactionScope())
            {
                try
                {
                    string addedBy = _userManager.GetUserId(User).ToString();
                    PremiumReversalParameters premiumReversalParameters = new()
                    {
                        PremiumHeaderID = PremiumID,
                        BillID = BillID, 
                        ReversedBy = addedBy,
                        ReversalReason = ReversalReason,
                        ReversalComment = ReversalComment
                    };
                    ReversalHeader reversalHeader = new ReversalHeader()
                    {
                         CurrencyID= (int)SuspenseHeader.CurrencyID,  
                         Amount = SuspenseHeader.Balance,
                         ReversalReason= ReversalReason, 
                         Reversed =1, 
                         ReversedOn=DateTime.Now, 
                         ReversedBy=addedBy
                    };                     
                    
                    if (ReversalReason == 1001)
                    {
                        _billedPremiumRepository.RefundPremium(premiumReversalParameters);
                        reversalHeader.ReversalComment= ReversalComment;
                        _reversalHeaderRepository.InsertReversalHeader(reversalHeader);
                    }
                    else
                    {
                        _billedPremiumRepository.ReversePremiums(premiumReversalParameters); 
                        reversalHeader.ReversalComment = "Correct Policy No: " + CorrectPolicyNo + ". " + ReversalComment;
                        _reversalHeaderRepository.InsertReversalHeader(reversalHeader);
                        SuspenseHeader.PaymentTypeID = 0; //to be adjusted
                        SuspenseHeader.InternalBankAccountID = 0;
                        SuspenseHeader.SuspenseType = 3; 
                        SuspenseHeader.PaymentDate = DateTime.Now; 
                        SuspenseHeader.StatusID = 0;
                        SuspenseHeader.ProcessedAmount = 0;  
                        SuspenseHeader.AddedOn = DateTime.Now;
                        SuspenseHeader.AddedBy = addedBy;
                        int id = _suspenseProcessing.AddSuspenseHeader(SuspenseHeader);
                        SuspenseLine suspenseLine = new()
                        {
                            HeaderID = id,
                            Credit = SuspenseHeader.Balance,
                            Debit = 0,
                            AddedOn = DateTime.Now,
                            AddedBy = addedBy
                        };
                        _suspenseProcessing.AddSuspenseLine(suspenseLine);
                    }
                    try
                    {
                        _reversalHeaderRepository.UpdatePolicyStatus((Guid)SuspenseHeader.PolicyID);
                    }
                    catch { }
                    
                    _processPayments.UpdateBatchBalances(SuspenseHeader.BatchID);
                    TS.Complete();
                    if (ReversalReason == 1001)
                    {
                        return Redirect("ReversalHistory");
                    }
                    else
                    {
                        return Redirect("AllocationSuspense");
                    }
                }
                catch (Exception ex)
                {
                    return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
                }
            }
        }
    }
}
