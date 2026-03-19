using LAIMS.Interfaces.Lifeproducts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Pages.Lifeproducts.PolicyTypes
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        public DataTable PolicyTypesDT;
        public IndexModel(UserManager<IdentityUser> userManager, IPolicyTypeRepository policyTypeRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
        }
        public void OnGet()
        {
            PolicyTypesDT = _policyTypeRepository.Get();  
        }
    }
}
