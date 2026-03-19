using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.PaymentCollection.Pages
{
    [Authorize(Roles = "Suspense Reconcilliation")]
    public class PolicySuspenseModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBilledPremiumRepository _billedPremiumRepository;
        private readonly ISuspenseProcessing _suspenseProcessing;
        private readonly IPolicyRepository _policyRepository;
        private readonly IReversalHeaderRepository _reversalHeaderRepository;
        private readonly IProcessPayments _processPayments; 
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
        public ReversalHeader ReversalHeader { get; set; }
        [BindProperty ]
        public string CorrectPolicyNo { get; set; }

        public PolicySuspenseModel(UserManager<ApplicationUser> userManager, 
            IBilledPremiumRepository billedPremiumRepository, IPolicyRepository policyRepository,
           IWebHostEnvironment webHostEnvironment, ISuspenseProcessing suspenseProcessing,
           IReversalHeaderRepository reversalHeaderRepository, IProcessPayments processPayments)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _policyRepository = policyRepository;
            _billedPremiumRepository = billedPremiumRepository;
            _suspenseProcessing = suspenseProcessing;
            _reversalHeaderRepository = reversalHeaderRepository;
            _processPayments = processPayments;
        }
        public IActionResult OnGet()
        {
            try
            {
                PaymentsDT = _suspenseProcessing.GetLatestPolicySuspenseData();
                ResultsCount = PaymentsDT.Rows.Count;
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
            return Page();        
        }
        public IActionResult OnGetBatch(long BatchID)
        {
            try
            {
                PaymentsDT = _suspenseProcessing.GetPolicySuspenseBatch(BatchID);
                ResultsCount = PaymentsDT.Rows.Count;
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
            return Page();
        }
        public void OnGetSearch(string? search)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            SearchTerm = search;
            SearchPageUrl = "PolicySuspense";
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                string format = "yyyy-MM-dd";
                if (DateTime.TryParseExact(SearchTerm, format, null, System.Globalization.DateTimeStyles.None, out DateTime PaymentDate))
                {
                    SearchCriteria = 0; //searching by date
                    PaymentsDT = _suspenseProcessing.SearchPolicySuspenseDataByDate(PaymentDate);
                    ResultsCount = PaymentsDT.Rows.Count;
                }
                else
                {
                    SearchCriteria = 1; //Search by PolicyNo
                    PaymentsDT = _suspenseProcessing.SearchLatestPolicySuspenseData(SearchTerm);
                    ResultsCount = PaymentsDT.Rows.Count;
                    if (ResultsCount > 0)
                    {
                        string policyNo = PaymentsDT.Rows[0]["PolicyNo"].ToString();  
                    }
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
                    ReversalHeader.Reversed = 1;
                    ReversalHeader.ReversedOn = DateTime.Now;
                    ReversalHeader.ReversedBy = addedBy;
                    ReversalHeader.CurrencyID = (int)SuspenseHeader.CurrencyID; 
                    ReversalHeader.Amount = SuspenseHeader.Balance; 
                    if ( ReversalHeader.ReversalReason == 1001)
                    { 
                         _reversalHeaderRepository.InsertReversalHeader(ReversalHeader);
                        _suspenseProcessing.ReverseSuspenseEntry(SuspenseHeader.ID, ReversalHeader.ReversalReason, ReversalHeader.ReversalComment, addedBy);
                    }
                    else
                    { 
                        ReversalHeader.ReversalComment = "Correct Policy No: " + CorrectPolicyNo + ". " + ReversalHeader.ReversalComment;
                        _reversalHeaderRepository.InsertReversalHeader(ReversalHeader);
                        _suspenseProcessing.ReversePolicySuspense(SuspenseHeader.ID, ReversalHeader.ReversalReason, ReversalHeader.ReversalComment, addedBy);
                    }
                    try
                    {
                        _reversalHeaderRepository.UpdatePolicyStatus((Guid)SuspenseHeader.PolicyID);
                    }
                    catch { }
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
    }
}
