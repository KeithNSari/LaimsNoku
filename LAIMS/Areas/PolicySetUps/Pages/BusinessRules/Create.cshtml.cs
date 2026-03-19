using LAIMS.Interfaces.BusinessRules;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.PolicySetUps.Pages.BusinessRules
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        public List<SelectListItem> StoredProcsList = new List<SelectListItem>();
        public DataTable BusinessRulesDT;
        [BindProperty]
        public BusinessRule BusinessRule { get; set; } 
        public CreateModel(UserManager<ApplicationUser> userManager,
            IBusinessRuleRepository businessRuleRepository)
        {
            _userManager = userManager;
            _businessRuleRepository = businessRuleRepository;
        }
        public void OnGet()
        {
            BusinessRulesDT = _businessRuleRepository.Get();
            LoadStoredProcsSelectList ();
        }
        private void LoadStoredProcsSelectList()
        {
            foreach (string SP in _businessRuleRepository.GetStoredProcedures( ))
            {
                StoredProcsList.Add(new SelectListItem
                {
                    Value = SP,
                    Text = SP
                });
            }
        }
        public void OnPost()
        {
            string addedBy = _userManager.GetUserId(User).ToString();
            BusinessRule.AddedBy = addedBy;
            BusinessRule.AddedOn = DateTime.Now;
            if (_businessRuleRepository.CheckExistence(BusinessRule)==0)
            {
                BusinessRule.ID = Guid.NewGuid();
                _businessRuleRepository.CreateBusinessRule(BusinessRule);
            }           
            BusinessRulesDT = _businessRuleRepository.Get();
            LoadStoredProcsSelectList();    
        }
        public void OnPostRemove(Guid ID)
        {
            string addedBy = _userManager.GetUserId(User).ToString();
            _businessRuleRepository.ArchiveBusinessRule(ID, addedBy, DateTime.Now);
            BusinessRulesDT = _businessRuleRepository.Get();
            LoadStoredProcsSelectList();
        }
    }
}
