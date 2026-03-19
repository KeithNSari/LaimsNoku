using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using LAIMS.Interfaces.Claims;
using Microsoft.AspNetCore.Authorization;
using LAIMS.Models.Security;

namespace LAIMS.Areas.Claims.Pages
{
    [Authorize(Roles = "Claims Initiator, Claims Approver")]
    public class MyReviewsModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyClaimRepository _policyClaimRepository;
        public string SearchPageUrl;
        public DataTable ClaimsDT;
        public MyReviewsModel(UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment,
            IPolicyClaimRepository policyClaimRepository)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _policyClaimRepository = policyClaimRepository;
        }
        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        [BindProperty]
        public int TotalCount { get; set; } = 0;
        public void OnGet()
        {
            string addedBy = _userManager.GetUserId(User).ToString();
            ClaimsDT = _policyClaimRepository.GetMyReviews(addedBy);
            ResultsCount = ClaimsDT.Rows.Count;
            if (ResultsCount < 100)
            {
                TotalCount = ResultsCount;
            }
        }
        public void OnGetSearch(string? search)
        {
            SearchTerm = search;
            SearchPageUrl = "MyReviews";
            string addedBy = _userManager.GetUserId(User).ToString();
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                ClaimsDT = _policyClaimRepository.SearchMyReviews(addedBy, SearchTerm);
                ResultsCount = ClaimsDT.Rows.Count;
            }
        }
    }
}
