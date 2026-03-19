using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    [Authorize(Roles = "New Business Initiator")]
    public class MySubmissionsModel : PageModel
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public string SearchPageUrl;
        public DataTable PoliciesDT;
        public MySubmissionsModel(UserManager<ApplicationUser> userManager,IPolicyRepository policyRepository, IWebHostEnvironment webHostEnvironment)
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
				PoliciesDT = _policyRepository.GetMySubmissions(addedBy);
				ResultsCount = PoliciesDT.Rows.Count;
				if (ResultsCount < 100)
				{
					TotalCount = ResultsCount;
				}
				else
				{
					TotalCount = _policyRepository.CountAllMyPoliciesAwaitingApproval(addedBy);
				}
			}
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new
				{
					errorMessage = "An error occurred during processing. " + ex.Message,
					returnUrl = ""
				});
			}
            return Page();
        }
        public IActionResult OnGetSearch(string? search)
        {
            try
            {
				SearchTerm = search;
				SearchPageUrl = "MySubmissions";
				string addedBy = _userManager.GetUserId(User).ToString();
				if (!string.IsNullOrEmpty(SearchTerm))
				{
					PoliciesDT = _policyRepository.PoliciesSearchByUser(SearchTerm, addedBy);
					ResultsCount = PoliciesDT.Rows.Count;
				}
			}
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new
				{
					errorMessage = "An error occurred during processing. " + ex.Message,
					returnUrl = ""
				});
			}
			return Page();			
        }  
    }
}
