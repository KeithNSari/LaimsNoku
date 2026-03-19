using LAIMS.Interfaces.Policies;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    [Authorize(Roles = "New Business Initiator, New Business Approver")]
    public class MyReviewsModel : PageModel
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public DataTable PoliciesDT;
        public string SearchPageUrl;
        public MyReviewsModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _webHostEnvironment = webHostEnvironment;
        }
        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        [BindProperty]
        public int TotalCount { get; set; } = 0;
        public IActionResult OnGet()
        {
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                PoliciesDT = _policyRepository.GetMyReviews(addedBy);
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new
                {
                    errorMessage = "An error occurred during processing. " + ex.Message,
                    returnUrl = ""
                });
            }                   
        }
        public IActionResult OnGetSearch(string? search)
        {
            try
            {
                SearchTerm = search;
                SearchPageUrl = "MyReviews";
                string addedBy = _userManager.GetUserId(User).ToString();
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    PoliciesDT = _policyRepository.PoliciesSearchByStatusUpdateUser(SearchTerm, addedBy);
                    ResultsCount = PoliciesDT.Rows.Count;
                }
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new
                {
                    errorMessage = "An error occurred during processing. " + ex.Message,
                    returnUrl = ""
                });
            }
        }
    }
} 
