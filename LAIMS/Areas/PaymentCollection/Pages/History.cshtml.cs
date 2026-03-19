using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.PaymentCollection.Pages
{
    public class HistoryModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBilledPremiumRepository _billedPremiumRepository; 
        public DataTable PaymentsDT;
        public string SearchPageUrl;

        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        [BindProperty ]
        public int ReversalReason { get; set; }
        [BindProperty]
        public string ReversalComment { get; set; }
        [BindProperty]
        public int BillID { get; set; }
        [BindProperty]
        public int PremiumID { get; set; }
        public HistoryModel(UserManager<ApplicationUser> userManager, IBilledPremiumRepository billedPremiumRepository,           
           IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager; 
            _webHostEnvironment = webHostEnvironment;
            _billedPremiumRepository = billedPremiumRepository;
        }
        public void OnGet()
        {
            PaymentsDT = _billedPremiumRepository.GetLatestPayments();
            ResultsCount = PaymentsDT.Rows.Count;
        }
        public void OnGetSearch(string? search)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            SearchTerm = search;
            SearchPageUrl = "History";
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                string format = "yyyy-MM-dd";
                if (DateTime.TryParseExact(SearchTerm, format, null, System.Globalization.DateTimeStyles.None, out DateTime PaymentDate))
                {
                    PaymentsDT = _billedPremiumRepository.SearchPayments (PaymentDate);
                    ResultsCount = PaymentsDT.Rows.Count;
                }
                else
                {
                    PaymentsDT = _billedPremiumRepository.SearchPayments(SearchTerm);
                    ResultsCount = PaymentsDT.Rows.Count;
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
                    _billedPremiumRepository.ReversePremiums(premiumReversalParameters);
                    TS.Complete();
                    return Redirect("Suspense");
                }
                catch (Exception ex)
                {
                    return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
                }
            }
        }
    }
}
