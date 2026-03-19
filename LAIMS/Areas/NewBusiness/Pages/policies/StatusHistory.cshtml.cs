using LAIMS.Interfaces.Policies;
using LAIMS.Models.Policies;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class StatusHistoryModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyRepository _policyRepository;
        public DataTable StatiiHistoryDT { get; set; }
        public StatusHistoryModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
        }
        [BindProperty]
        public Guid ProposerID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid PolicyID { get; set; }
        public IActionResult OnGet(Guid id, Guid policyTypeid, Guid policyid)
        {
            try
            {
                ProposerID = id;
                PolicyTypeID = policyTypeid;
                PolicyID = policyid;
                StatiiHistoryDT = _policyRepository.GetPolicyStatusHistory(policyid);
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
    }
}
