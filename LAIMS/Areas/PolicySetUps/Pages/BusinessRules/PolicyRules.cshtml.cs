using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Security;
using LAIMS.Repositories.Lifeproducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.PolicySetUps.Pages.BusinessRules
{
    [Authorize(Roles = "Admin")]
    public class PolicyRulesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        private readonly IObjectRuleRepository _objectRuleRepository;
        public List<SelectListItem> PolicyTypesList = new List<SelectListItem>();
        public List<SelectListItem> BusinessRuleList = new List<SelectListItem>();
        public DataTable PolicyRulesDT; 
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public ObjectRule ObjectRule { get; set; }
        public PolicyRulesModel(UserManager<ApplicationUser> userManager,
            IBusinessRuleRepository businessRuleRepository,
            IObjectRuleRepository objectRuleRepository,
            IPolicyTypeRepository policyTypeRepository)
        {
            _userManager = userManager;
            _businessRuleRepository = businessRuleRepository;
            _policyTypeRepository = policyTypeRepository;
            _objectRuleRepository = objectRuleRepository;
        }
        public void OnGet(string? policyID)
        {
            try
            { 
                if (policyID != null)
                {
                    Guid myPolicyID = Guid.Parse(policyID);
                    PolicyTypeID = myPolicyID;
                    PolicyRulesDT = _objectRuleRepository.Get(myPolicyID);
                }
                LoadPolicyTypesList(); 
                LoadBusinessRulesSelectList();
               
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
           
        }
        private void LoadBusinessRulesSelectList()
        {
            foreach  (BusinessRule BR in _businessRuleRepository.GetAllBusinessRules())
            {
                BusinessRuleList.Add(new SelectListItem
                {
                    Value = BR.ID.ToString(),
                    Text = BR.RuleName
                }) ;
            }
        }
        private void LoadPolicyTypesList()
        {
            foreach (PolicyType policyType in _policyTypeRepository.GetAllPolicyTypes())
            {
                PolicyTypesList.Add(new SelectListItem
                {
                    Value = policyType.ID.ToString(),
                    Text = policyType.Name
                });
            }
        }
    }
}
