using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Questionnaires;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.PolicySetUps.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class PQuestionnairesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private readonly IProductQuestionnaireRepository _productQuestionnaireRepository;
        public List<SelectListItem> QuestionnairesList = new List<SelectListItem>();
        public DataTable QuestionnaireDT;
        public PQuestionnairesModel(UserManager<ApplicationUser> userManager, IProductRepository productRepository, IQuestionnaireRepository questionnaireRepository, IProductQuestionnaireRepository productQuestionnaireRepository)
        {
            _userManager = userManager;
            _productRepository = productRepository;
            _questionnaireRepository = questionnaireRepository;
            _productQuestionnaireRepository = productQuestionnaireRepository;
        }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public Product MyProduct { get; set; } = default!;
        [BindProperty]
        public ProductQuestionnaire MyProductQuestionnaire { get; set; } = default!;
        public IActionResult OnGet(Guid id)
        {           
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                var product = _productRepository.GetProduct(id);
                if (product == null)
                {
                    return NotFound();
                }
                else
                {
                    LoadQuestionnaireSelectList();
                    MyProduct = product;
                    QuestionnaireDT = _productQuestionnaireRepository.GetQuestionnaires(id);
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
            return Page();
        }
        private void LoadQuestionnaireSelectList()
        {
            foreach (Questionnaire questionnaire in _questionnaireRepository.GetAll())
            {
                QuestionnairesList.Add(new SelectListItem
                {
                    Value = questionnaire.ID.ToString(),
                    Text = questionnaire.Title
                });
            }
        }
        public IActionResult OnPost(Guid id)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    
                //    return Page();
                //}
                MyProductQuestionnaire.ProductID = id;
                if (_productQuestionnaireRepository.CheckExistence(MyProductQuestionnaire) == 0)
                {
                    MyProductQuestionnaire.Current = 1;
                    MyProductQuestionnaire.AddedOn = DateTime.Now;
                    MyProductQuestionnaire.AddedBy = _userManager.GetUserId(User).ToString();
                    _productQuestionnaireRepository.InsertQuestionnaire(MyProductQuestionnaire);
                }
                QuestionnaireDT = _productQuestionnaireRepository.GetQuestionnaires(id);
                MyProduct = _productRepository.GetProduct(id);
                LoadQuestionnaireSelectList();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
            return Page();
        }
        public IActionResult OnPostRemove(Guid id, int entryno)
        {
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                DateTime deletedOn = DateTime.Now;
                _productQuestionnaireRepository.DeleteQuestionnaire(entryno, addedBy, deletedOn);
                return Redirect(ReturnUrl);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            } 
        }
    }
}
