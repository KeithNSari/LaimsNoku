using LAIMS.Interfaces.Lifeproducts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{   
    public class PolicyListModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        public DataTable PolicyTypesDT;
        [BindProperty ]
        public Guid ApplicantID { get; set; }
        public PolicyListModel(UserManager<IdentityUser> userManager, IPolicyTypeRepository policyTypeRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
        }
        public void OnGet(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) { return; }
            if(Guid.TryParse(id,out Guid _applicantID))
            {
                ApplicantID = _applicantID;
                PolicyTypesDT = _policyTypeRepository.Get();
            }
            else
            { 
                return; 
            }           
        } 
    }
}
