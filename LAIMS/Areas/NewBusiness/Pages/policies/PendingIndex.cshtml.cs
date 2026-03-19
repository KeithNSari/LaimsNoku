using LAIMS.Interfaces.Policies;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    [Authorize(Roles = "New Business Initiator")]
    public class PendingIndexModel : PageModel
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public DataTable PoliciesDT;
        public string SearchPageUrl;
        public PendingIndexModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository, IWebHostEnvironment webHostEnvironment)
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
                string addedBy = _userManager.GetUserId(User).ToString();
                PoliciesDT = _policyRepository.GetMyWorkQueue(addedBy);
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new
                {
                    errorMessage = "An error occurred during processing. " + ex.Message,
                    returnUrl = ReturnUrl
                });
            }
        }
        public IActionResult OnGetSearch(string? search)
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                SearchTerm = search;
                SearchPageUrl = "PendingIndex";
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    string addedBy = _userManager.GetUserId(User).ToString();
                    PoliciesDT = _policyRepository.SearchMyWorkQueue(SearchTerm, addedBy);
                    ResultsCount = PoliciesDT.Rows.Count;
                }
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new
                {
                    errorMessage = "An error occurred during processing. " + ex.Message,
                    returnUrl = ReturnUrl 
                });
            }
        }
        public IActionResult OnPostPolicyDetails(Guid memberId, Guid policyTypeId, Guid policyId)
        {
             
            int stage = _policyRepository.GetPolicyStage(policyId);
            //if (statusId == 0) { }
            switch (stage)
            {
                case 2:
                    return Redirect("~/newbusiness/policies/AddDetails?id=" + memberId + "&policytypeid=" + policyTypeId + "&policyid=" + policyId);
                case 3:
                    return Redirect("~/newbusiness/policies/ivshares?id=" + memberId + "&policytypeid=" + policyTypeId + "&policyid=" + policyId);
                case 4:
                    return Redirect("~/newbusiness/policies/documents?id=" + memberId + "&policytypeid=" + policyTypeId + "&policyid=" + policyId);
                case 5:
                    return Redirect("~/newbusiness/policies/pbquestionnaires?id=" + memberId + "&policytypeid=" + policyTypeId + "&policyid=" + policyId);
                case 6: 
                    return Redirect("~/newbusiness/policies/submission?id=" + memberId + "&policytypeid=" + policyTypeId + "&policyid=" + policyId);
                default: break;
            }
            return Redirect("PendingIndex");
        }
    }
}
