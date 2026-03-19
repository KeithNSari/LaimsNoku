using LAIMS.Interfaces.Policies;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.Claims.Pages
{
    [Authorize(Roles = "Claims Initiator")]
    public class PolicySearchModel : PageModel
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public DataTable PoliciesDT;
        public string SearchPageUrl;

        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        public PolicySearchModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _webHostEnvironment = webHostEnvironment;
        }
        public void OnGet()
        {
        }
        public IActionResult OnGetSearch(string? search)
        {
			string ReturnUrl = Request.Path + Request.QueryString;
			try
            {
				SearchTerm = search;
				SearchPageUrl = "PolicySearch";
				if (!string.IsNullOrEmpty(SearchTerm))
				{
					PoliciesDT = _policyRepository.PoliciesSearchRiskPolicies(SearchTerm);//search only approved and beyond
					ResultsCount = PoliciesDT.Rows.Count;
				}
			}
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
			}
			return Page();			
        }
    }
}
