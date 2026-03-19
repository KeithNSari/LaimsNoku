using LAIMS.Interfaces.Policies;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.Policies.Pages
{

    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public DataTable PoliciesDT;
        public string SearchPageUrl;
        public IndexModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository, IWebHostEnvironment webHostEnvironment)
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
				PoliciesDT = _policyRepository.GetByLatestStatii();
				ResultsCount = PoliciesDT.Rows.Count;
			}
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = "" });
			}
            return Page();
        }
        public IActionResult OnGetSearch(string? search)
        {
			try
			{
				SearchTerm = search;
				SearchPageUrl = "Index";
				string addedBy = _userManager.GetUserId(User).ToString();
				if (!string.IsNullOrEmpty(SearchTerm))
				{
					PoliciesDT = _policyRepository.Search(SearchTerm);
					ResultsCount = PoliciesDT.Rows.Count;
				}
			}
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = "" });
			}
            return Page();
		}
    }
}
