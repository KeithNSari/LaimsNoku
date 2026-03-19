using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LAIMS.Interfaces.Commissions;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using LAIMS.Interfaces.Policies;
using System.Data;
using LAIMS.Repositories.Policies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using LAIMS.Models.Security;

namespace LAIMS.Areas.PolicyServicing.Pages.Policies
{
  //[Authorize(Roles = "Policy Servicing Approver")]
    public class PPReviewsModel : PageModel
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public DataTable PoliciesDT;
        public string SearchPageUrl;
        public PPReviewsModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository, IWebHostEnvironment webHostEnvironment)
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
			string ReturnUrl = Request.Path + Request.QueryString;
			try
            {
				PoliciesDT = _policyRepository.GetPolicyServicingRequests();
				ResultsCount = PoliciesDT.Rows.Count;
				TotalCount = ResultsCount;
			}
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new
				{
					errorMessage = "An error occurred during processing. " + ex.Message,
					returnUrl = ReturnUrl
				});
			}
            return Page ();			
        }
        public IActionResult OnGetSearch(string? search)
        {
			string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
				SearchTerm = search.Trim();
				SearchPageUrl = "PPReviews";
				if (!string.IsNullOrEmpty(SearchTerm))
				{
					PoliciesDT = _policyRepository.SearchPolicyServicingRequests(SearchTerm);//Status 6 is awaiting approval
					ResultsCount = PoliciesDT.Rows.Count;
				}
			}
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new
				{
					errorMessage = "An error occurred during processing. " + ex.Message,
					returnUrl = ReturnUrl
				});
			}
			return Page();			
        }
    }
}
