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
    public class SettlementsModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyClaimRepository _policyClaimRepository;
        public string SearchPageUrl;
        public DataTable ClaimsDT;
        public SettlementsModel(UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment,
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
            ClaimsDT = _policyClaimRepository.GetDuePayments();
            ResultsCount = ClaimsDT.Rows.Count;
            if (ResultsCount < 100)
            {
                TotalCount = ResultsCount;
            }
        }
        public void OnGetSearch(string? search)
        {
            SearchTerm = search;
            SearchPageUrl = "Approve";
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                ClaimsDT = _policyClaimRepository.SearchUnReviewed(SearchTerm);
                ResultsCount = ClaimsDT.Rows.Count;
            }
        }
    }
}
