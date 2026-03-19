using LAIMS.Interfaces.Claims;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
namespace LAIMS.Areas.Claims.Pages
{
    [Authorize(Roles = "Claims Initiator, Claims Approver")]
    public class DeceasedIndexModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyClaimRepository _policyClaimRepository;
        public DataTable DeceasedDT;
        public DeceasedIndexModel(UserManager<ApplicationUser> userManager,
          IWebHostEnvironment webHostEnvironment, IPolicyClaimRepository policyClaimRepository)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _policyClaimRepository = policyClaimRepository;
        }
        public void OnGet()
        {
            DeceasedDT = _policyClaimRepository.GetMemberDeaths(); 
        }
    }
}
