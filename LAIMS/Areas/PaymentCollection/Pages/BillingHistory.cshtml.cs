using LAIMS.Interfaces.BatchJobs;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.PaymentCollection.Pages
{
	[Authorize(Roles = "Billing History")]
	public class BillingHistoryModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBilledPremiumRepository _billedPremiumRepository;
        private readonly IPaymentTypeRepository _paymentTypeRepository;
        private readonly ISuspenseProcessing _suspenseProcessing;
        private readonly IPolicyRepository _policyRepository;
        private readonly IBillingMessageRepository _billingMessageRepository; 
        public string SearchPageUrl;
        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public PremiumHeader PremiumHeader { get; set; }
        [BindProperty]
        public PremiumLine PremiumLine { get; set; }
        [BindProperty]
        public List<PaymentType> PaymentTypes { get; set; }
        [BindProperty]
        public int BillID { get; set; }
        [BindProperty]
        public string Currency { get; set; }
        [BindProperty]
        public decimal Amount { get; set; }
        [BindProperty]
        public int PaymentType { get; set; }
        [BindProperty]
        public PolicyDetailedBalance PolicyDetailedBalance { get; set; }
        [BindProperty]
        public string Reference { get; set; }
        [BindProperty]
        public string PaymentSource { get; set; }
        [BindProperty ]
        public int StatusID { get; set; }
        [BindProperty]
        public int StatusReason { get; set; }
        [BindProperty ]
        public string StatusComment { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        public BillingHistoryModel(UserManager<ApplicationUser> userManager
            ,IBilledPremiumRepository billedPremiumRepository, IPolicyRepository policyRepository,
             IPaymentTypeRepository paymentTypeRepository,
             ISuspenseProcessing suspenseProcessing, IBillingMessageRepository billingMessageRepository)
        {
            _userManager = userManager;
            _billedPremiumRepository = billedPremiumRepository;
            _policyRepository = policyRepository;
            _paymentTypeRepository = paymentTypeRepository;
            _suspenseProcessing = suspenseProcessing;
            _billingMessageRepository = billingMessageRepository;
        }
        public DataTable BillsDT { get; set; }
        public void OnGet()
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                BillsDT = _billedPremiumRepository.GetLatest();
                int totalRowsCount = (from DataRow row in BillsDT.Rows
                                      let value = row.Field<string>("InvoiceNo")
                                      where value != null && value.Contains("Total")
                                      select row).Count();
                ResultsCount = BillsDT.Rows.Count - totalRowsCount;
                PaymentTypes = _paymentTypeRepository.GetAllPaymentTypes();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }         
        }
        public void OnGetSearch(string? search)
        {
            try
            {
                SearchTerm = search;
                SearchPageUrl = "BillingHistory";
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    BillsDT = _billedPremiumRepository.Search(SearchTerm);
                    int rowsCount = BillsDT.Rows.Count;
                    if (rowsCount > 0)
                    {
                        int totalRowsCount = (from DataRow row in BillsDT.Rows
                                              let value = row.Field<string>("InvoiceNo")
                                              where value != null && value.Contains("Total")
                                              select row).Count();
                        ResultsCount = BillsDT.Rows.Count - totalRowsCount;
                        PolicyDetailedBalance = _billedPremiumRepository.GetDetailedBalance(SearchTerm);
                    }
                    else
                    {
                        ResultsCount = 0;
                    }
                    PaymentTypes = _paymentTypeRepository.GetAllPaymentTypes();
                }
            }
             catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }           
        }

        public IActionResult OnPost()
        {
            ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                if (PremiumHeader.DatePaymentReceived > DateTime.Now.Date)
                {
                    throw new Exception("Payment date cannot be in the future.");
                }
                using (TransactionScope TS = new TransactionScope())
                {
                    string addedBy = _userManager.GetUserId(User).ToString();
                    decimal invoiceAmount = _billedPremiumRepository.GetBillAmount(PremiumHeader.BillingID);
                    decimal suspenseAmount = 0;
                    if (invoiceAmount <= PremiumHeader.TotalAmount)
                    {
                        decimal fullAmount = PremiumHeader.TotalAmount; 
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
                    else if (invoiceAmount > PremiumHeader.TotalAmount)
                    {
                        //insufficient balance send whole amount to policy suspense
                        suspenseAmount = PremiumHeader.TotalAmount;
                    }
                    if (suspenseAmount > 0)
                    {
                        int proposerID = _policyRepository.GetProposerID(SearchTerm); //change to use PolicyNo not search term
                        SuspenseHeader suspenseHeader = new()
                        {
                            MemberID = proposerID,
                            PaymentMethodID = 3,
                            PaymentTypeID = PaymentType,
                            Source= PaymentSource,
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
                    if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        BillsDT = _billedPremiumRepository.Search(SearchTerm);
                        PolicyDetailedBalance = _billedPremiumRepository.GetDetailedBalance(SearchTerm);
                    }
                    else
                    {
                        BillsDT = _billedPremiumRepository.GetLatest();
                    }
                    if (BillsDT.Rows.Count > 0)
                    {
                        int totalRowsCount = (from DataRow row in BillsDT.Rows
                                              let value = row.Field<string>("InvoiceNo")
                                              where value != null && value.Contains("Total")
                                              select row).Count();
                        ResultsCount = BillsDT.Rows.Count - totalRowsCount;
                        PolicyDetailedBalance = _billedPremiumRepository.GetDetailedBalance(SearchTerm);
                    }
                    else
                    {
                        ResultsCount = 0;
                    }
                    TS.Complete();
                }
                return Redirect(ReturnUrl);
            }
            catch (Exception Ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + Ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostSendBillingMessage()
        {
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                BillingMessage billingMessage = new()
                {
                    BillID = BillID,
                    Status = StatusID,
                    StatusReason = StatusReason,
                    Message = StatusComment,
                    AddedBy = addedBy,
                    AddedOn = DateTime.Now
                };
                _billingMessageRepository.AddBillingMessage(billingMessage);
                return RedirectToPage("BillingNotifications");
            }
            catch (Exception Ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + Ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
}
