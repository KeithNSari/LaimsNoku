using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Premiums;
using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Premiums;
using LAIMS.Models.Questionnaires;
using LAIMS.Models.Security;
using LAIMS.Repositories.Lifeproducts;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.PolicySetUps.Pages.PolicyTypes
{
    [Authorize(Roles = "Admin")]
    public class PTQuestionnairesModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private readonly IPolicyTypesQuestionnairesRepository _policyTypesQuestionnairesRepository;
        public List<SelectListItem> PolicyTypesList = new List<SelectListItem>();

        [BindProperty]
        public PTQuestionnaire PTQuestionnaire { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public List<Questionnaire> MyQuestionnaires { get; set; }
        public DataTable PTQuestionnairesDT { get; set; }
        public PTQuestionnairesModel(UserManager<ApplicationUser> userManager, IPolicyTypeRepository policyTypeRepository, IQuestionnaireRepository questionnaireRepository, IPolicyTypesQuestionnairesRepository policyTypesQuestionnairesRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
            _questionnaireRepository= questionnaireRepository;
            _policyTypesQuestionnairesRepository = policyTypesQuestionnairesRepository;
        }

        public void OnGet(string? policyID)
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                if (policyID != null)
                {
                    Guid myPolicyID = Guid.Parse(policyID);
                    PolicyTypeID = myPolicyID;
                    PTQuestionnairesDT = _policyTypesQuestionnairesRepository.GetByPolicyType(Guid.Parse(policyID));
                }
                LoadPolicyTypesList();
                MyQuestionnaires = _questionnaireRepository.GetAll(); 
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
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
        public IActionResult OnPost()
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    
                //    return Page();
                //}
                PTQuestionnaire.PolicyTypeID = PolicyTypeID;
                PTQuestionnaire.AddedBy = _userManager.GetUserId(User).ToString();
                PTQuestionnaire.AddedOn = DateTime.Now;
                //if (_policyTypesQuestionnairesRepository.CheckExistence(PTQuestionnaire) == 0)
                //{
                    _policyTypesQuestionnairesRepository.InsertPTQuestionnaire(PTQuestionnaire); 
                //}
                //else
                //{
                //    throw new Exception("This questionnaire has already been added. If it applies to both Tested and Untested business, select the 'ALL' option!");
                //}
                return Redirect("PTQuestionnaires?policyid=" + PolicyTypeID); 
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
            
        }
        public IActionResult OnPostRemove(Guid ID)
        {
            try
            {
                PTQuestionnaire.ID = ID;
                PTQuestionnaire.AddedBy = _userManager.GetUserId(User).ToString();
                PTQuestionnaire.AddedOn = DateTime.Now;
                _policyTypesQuestionnairesRepository.ArchivePTQuestionnaire(PTQuestionnaire);
                return Redirect("PTQuestionnaires?policyid=" + ID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
            
        }
    }
}
