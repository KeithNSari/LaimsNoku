using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Transactions;
using LAIMS.Models.Security;

namespace LAIMS.Areas.PaymentCollection.Pages
{
    [Authorize(Roles = "Suspense Reconcilliation")]
    public class SystemSuspenseModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBilledPremiumRepository _billedPremiumRepository;
        private readonly ISuspenseProcessing _suspenseProcessing;
        private readonly IPolicyRepository _policyRepository;
        private readonly IReversalHeaderRepository _reversalHeaderRepository;
        private readonly IProcessPayments _processPayments;
        private readonly IPaymentRepository _paymentRepository;
        public DataTable PaymentsDT;
        public string SearchPageUrl;
        [BindProperty]
        public int SearchCriteria { get; set; }
        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        [BindProperty]
        public SuspenseHeader SuspenseHeader { get; set; }
        [BindProperty]
        public string PolicyNo { get; set; }
        [BindProperty ]
        public int ProcessAction { get; set; }
        [BindProperty]
        public string ActionComment { get; set; }
        [BindProperty ]
        public decimal AmountToAllocate { get; set; }

        public SystemSuspenseModel(UserManager<ApplicationUser> userManager,
            IBilledPremiumRepository billedPremiumRepository, IPolicyRepository policyRepository,
           IWebHostEnvironment webHostEnvironment, ISuspenseProcessing suspenseProcessing,
           IReversalHeaderRepository reversalHeaderRepository, IProcessPayments processPayments, 
           IPaymentRepository paymentRepository)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _policyRepository = policyRepository;
            _billedPremiumRepository = billedPremiumRepository;
            _suspenseProcessing = suspenseProcessing;
            _reversalHeaderRepository = reversalHeaderRepository;
            _processPayments = processPayments;
            _paymentRepository = paymentRepository;
        }
        public void OnGet()
        {
            PaymentsDT = _suspenseProcessing.GetLatestSystemSuspenseData();
            ResultsCount = PaymentsDT.Rows.Count;
        }
        public void OnGetSearch(string? search)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            SearchTerm = search;
            SearchPageUrl = "SystemSuspense";
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                string format = "yyyy-MM-dd";
                if (DateTime.TryParseExact(SearchTerm, format, null, System.Globalization.DateTimeStyles.None, out DateTime PaymentDate))
                {
                    SearchCriteria = 0; //searching by date
                    PaymentsDT = _suspenseProcessing.SearchSytemSuspenseDataByDate(PaymentDate);
                    ResultsCount = PaymentsDT.Rows.Count;
                }
                else
                {
                    //SearchCriteria = 1; //Search by PolicyNo
                    //PaymentsDT = _suspenseProcessing.SearchLatestPolicySuspenseData(SearchTerm);
                    //ResultsCount = PaymentsDT.Rows.Count;
                    //if (ResultsCount > 0)
                    //{
                    //    string policyNo = PaymentsDT.Rows[0]["PolicyNo"].ToString();
                    //}
                }
            }
        }
        public IActionResult OnPost()
        {
            using (TransactionScope TS = new TransactionScope())
            {
                try
                {
                    if (AmountToAllocate == 0)
                    {
                        throw new Exception("Amount allocated should be greater than 0!");
                    }
                    if (AmountToAllocate > SuspenseHeader.Balance)
                    {
                        throw new Exception("You cannot allocate an amount which is greater than the available balance!");
                    }
                    string addedBy = _userManager.GetUserId(User).ToString();
                    if (ProcessAction == 1001) //Refund
                    {
                        ReversalHeader reversalHeader = new()
                        {
                            Reversed = 1,
                            ReversedOn = DateTime.Now,
                            ReversalReason = 1001,
                            ReversedBy = addedBy,
                            CurrencyID = (int)SuspenseHeader.CurrencyID,
                            Amount = AmountToAllocate
                        };
                        _reversalHeaderRepository.InsertReversalHeader(reversalHeader);
                        _suspenseProcessing.RefundSystemSuspenseEntry(SuspenseHeader.ID, 1001, AmountToAllocate, ActionComment, addedBy);
                        //_suspenseProcessing.ReverseSuspenseEntry(SuspenseHeader.ID, 1001, ActionComment, addedBy);
                        try
                        {
                            _reversalHeaderRepository.UpdatePolicyStatus((Guid)SuspenseHeader.PolicyID);
                        }
                        catch { }
                    }
                    else
                    {
                        ValidateAllocationEntries(PolicyNo);
                        AllocateToPolicy(SuspenseHeader.ID);
                    }
                    _processPayments.UpdateBatchBalances(SuspenseHeader.BatchID);
                    TS.Complete();
                    return Redirect("SystemSuspense");
                }
                catch (Exception ex)
                {
                    return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
                }
            }
        }
        public void AllocateToPolicy(int SuspenseHeaderID)
        {
            using (TransactionScope TS = new TransactionScope())
            {
                Guid PolicyID = _policyRepository.GetPolicyID(PolicyNo);                
                string addedBy = _userManager.GetUserId(User).ToString();
                decimal paid = _processPayments.AdhocPayment(PolicyID, SuspenseHeaderID.ToString(), AmountToAllocate, 4, 0, DateTime.Now, addedBy);
                decimal suspenseAmount = AmountToAllocate - paid; 
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
                if (suspenseAmount > 0) //create a new suspense policy entry
                {
                    int proposerID = _policyRepository.GetProposerID(PolicyNo);
                    SuspenseHeader suspenseHeader = new()
                    {
                        MemberID = proposerID,
                        PaymentMethodID = 4,
                        PaymentTypeID = 0,
                        SourceID= SuspenseHeaderID,
                        BatchID =SuspenseHeader.BatchID,
                        InternalBankAccountID = 0,
                        SuspenseType = 2,
                        PolicyID = PolicyID,
                        PaymentDate = DateTime.Now,
                        Reference = SuspenseHeaderID.ToString(),
                        StatusID = 0,
                        ProcessedAmount = 0,
                        CurrencyID = SuspenseHeader.CurrencyID,
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
                _processPayments.UpdatePolicyStatus(PolicyID); 
                TS.Complete();
            }
        }
        public void ValidateAllocationEntries(string Recipient)
        {
            if (string.IsNullOrEmpty(Recipient))
            {
                throw new Exception("Recepient cannot be empty!");
            }           
            if (!_policyRepository.CheckExistence(Recipient))
            {
                throw new Exception("Invalid policy provided!");
            }
        }
        public IActionResult OnGetLoadDetails(int id)
        {
            // Use the id parameter in your logic to fetch data
            List<Payment> payments = new List<Payment>();
            payments.Add(_paymentRepository.GetPaymentById(id));
            return new JsonResult(payments);
        }
    }
}
