using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class StatusOverrideModel : PageModel
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        public DataTable PoliciesDT;
        public string SearchPageUrl;
        public StatusOverrideModel(UserManager<ApplicationUser> userManager, IBusinessRuleRepository businessRuleRepository,
            IPolicyRepository policyRepository, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _webHostEnvironment = webHostEnvironment;
            _businessRuleRepository = businessRuleRepository;
        }
        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        [BindProperty]
        public int TotalCount { get; set; } = 0;  
        [BindProperty]
        public PolicyStatiiOverride PolicyStatiiOverride { get; set; }
        public void OnGet()
        {
            string addedBy = _userManager.GetUserId(User).ToString();
            PoliciesDT = _policyRepository.GetPoliciesByStatusID(5);
        }
        public void OnGetSearch(string? search)
        {
            SearchTerm = search;
            SearchPageUrl = "StatusOverride";
            string addedBy = _userManager.GetUserId(User).ToString();
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                PoliciesDT = _policyRepository.PoliciesSearch(SearchTerm, 5);
                ResultsCount = PoliciesDT.Rows.Count;
            }
        }
        public void OnPost()
        {
            string addedBy = _userManager.GetUserId(User).ToString();
            PolicyStatiiOverride.OverridenBy = addedBy;
            PolicyStatiiOverride.OverriddenOn = DateTime.Now;
            _businessRuleRepository.AddPolicyStatiiOverride(PolicyStatiiOverride);
        }
    }
}
