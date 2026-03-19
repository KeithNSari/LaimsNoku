using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.PolicySetUps.Pages.PolicyTypes
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        public DataTable PolicyTypesDT;
        public IndexModel(UserManager<ApplicationUser> userManager, IPolicyTypeRepository policyTypeRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
        }
        public void OnGet()
        {
            try
            {
                PolicyTypesDT = _policyTypeRepository.Get();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }            
        }
    }
}
