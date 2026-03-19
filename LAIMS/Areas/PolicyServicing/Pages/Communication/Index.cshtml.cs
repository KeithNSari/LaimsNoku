using LAIMS.Interfaces.Policies;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
namespace LAIMS.Areas.PolicyServicing.Pages.Communication
{
  //  [Authorize(Roles = "Policy Servicing Approver")]
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyRepository _policyRepository; 
        public IndexModel(UserManager<ApplicationUser> userManager,IWebHostEnvironment webHostEnvironment,
            IPolicyRepository policyRepository)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _policyRepository = policyRepository;
        }
        public DataTable MessagesDT { get; set; }
        public void OnGet()
        {
            MessagesDT = _policyRepository.PolicyServicingMessagesGet(); 
        }
    }
}
