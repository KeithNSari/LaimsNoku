using LAIMS.Interfaces.Policies;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.PolicyServicing.Pages.Policies
{
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
        public void OnGetSearch(string? search)
        {
            SearchTerm = search;
            SearchPageUrl = "PolicySearch";
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                PoliciesDT = _policyRepository.PoliciesSearchPostApproved(SearchTerm);//search only approved and beyond
                ResultsCount = PoliciesDT.Rows.Count;
            }
        }
    }
}
