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

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    [Authorize(Roles = "New Business Approver")]
    public class ApproveModel : PageModel
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public DataTable PoliciesDT;
        public string SearchPageUrl;
        public ApproveModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository, IWebHostEnvironment webHostEnvironment)
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
                PoliciesDT = _policyRepository.GetPoliciesAwaitingApproval();
                ResultsCount = PoliciesDT.Rows.Count;
                if (ResultsCount < 100)
                {
                    TotalCount = ResultsCount;
                }
                else
                {
                    TotalCount = _policyRepository.CountAllPoliciesAwaitingApproval();
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
        public IActionResult OnGetSearch(string? search)
        {
            try
            {
                SearchTerm = search;
                SearchPageUrl = "Approve";
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    PoliciesDT = _policyRepository.PoliciesSearch(SearchTerm, 6);//Status 6 is awaiting approval
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
        public void OnPost()
        {
            if (!string.IsNullOrEmpty(SearchTerm))
            {
               
            }
        }
    }
}
