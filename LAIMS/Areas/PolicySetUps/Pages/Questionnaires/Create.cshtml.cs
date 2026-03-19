using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.Questionnaires;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Runtime.CompilerServices;

namespace LAIMS.Areas.PolicySetUps.Pages.Questionnaires
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuestionExpectedResponseRepository _questionExpectedResponseRepository;
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private readonly IQuestionnaireQsnsRepository _questionnaireQsnsRepository;
        public DataTable QuestionnairesDT;
        public DataTable QuestionnairesQsnsDT;
        public DataTable QuestionsDT;
        public DataTable ResponsesDT;
        public CreateModel(UserManager<ApplicationUser> userManager, IQuestionRepository questionRepository, IQuestionExpectedResponseRepository questionExpectedResponseRepository, IQuestionnaireRepository questionnaireRepository, IQuestionnaireQsnsRepository questionnaireQsnsRepository)
        {
            _userManager = userManager;
            _questionRepository = questionRepository;
            _questionExpectedResponseRepository = questionExpectedResponseRepository;
            _questionnaireRepository = questionnaireRepository;
            _questionnaireQsnsRepository = questionnaireQsnsRepository;
        }
        [BindProperty]
        public Question Question { get; set; }
        [BindProperty]
        public List<Question> Questions { get; set; }
        [BindProperty]
        public List<Question> AllQuestions { get; set; }
        [BindProperty]
        public QuestionExpectedResponse QuestionExpectedResponse { get; set; }
        [BindProperty]
        public Questionnaire Questionnaire { get; set; }
        [BindProperty ]
        public List<Questionnaire> MyQuestionnaires { get; set; }
        [BindProperty]
        public QuestionnaireQsn QuestionnaireQsn { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        public void OnGet()
        {           
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                Load();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
        }
        private void Load()
        {
            QuestionsDT = _questionRepository.Get();
            Questions = _questionRepository.GetAllNonOpen();
            AllQuestions = _questionRepository.GetAll();
            ResponsesDT = _questionExpectedResponseRepository.Get();
            QuestionnairesDT =_questionnaireRepository.Get();
            MyQuestionnaires = _questionnaireRepository.GetAll();
            QuestionnairesQsnsDT = _questionnaireQsnsRepository.GetAllQuestions();
        }
        public IActionResult OnPostCreateQuestions()
        {           
            try
            {               
                Question.ID = Guid.NewGuid();
                Question.AddedBy = _userManager.GetUserId(User).ToString();
                Question.AddedOn = DateTime.Now;
                if (_questionRepository.CheckExistence(Question) > 0)
                {
                    throw new Exception("This question already exists!");
                }
                _questionRepository.Create(Question);
                Load();
                return Redirect("Create?tab=tab1");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }           
        }
        public IActionResult OnPostRemove(Guid id)
        {
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                _questionRepository.Delete(id, addedBy);
                Load();
                return Redirect("Create?tab=tab1");
            } 
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl});
            }
         
        }
        public IActionResult OnPostAddResponses()
        {            
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                QuestionExpectedResponse.ID = Guid.NewGuid();
                QuestionExpectedResponse.AddedBy = addedBy;
                QuestionExpectedResponse.AddedOn = DateTime.Now;
                if(_questionExpectedResponseRepository.CheckExistence(QuestionExpectedResponse )>0)
                {
                    throw new Exception("This response has already been added to the selected question!");
                }
                _questionExpectedResponseRepository.Create(QuestionExpectedResponse);
                Load();
                return Redirect("Create?tab=tab2");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }            
        }
        public IActionResult OnPostRemoveResponses(Guid id)
        {           
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                _questionExpectedResponseRepository.Archive(id, addedBy);
                Load();
                return Redirect("Create?tab=tab2");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }      
        }
        public IActionResult OnPostAddQuestionnaire()
        {            
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                Questionnaire.ID = Guid.NewGuid();
                Questionnaire.AddedBy = addedBy;
                Questionnaire.AddedOn = DateTime.Now;
                if(_questionnaireRepository.CheckExistence(Questionnaire )>0)
                {
                    throw new Exception("This questionnaire already exists!");
                }
                _questionnaireRepository.Create(Questionnaire);
                Load();
                return Redirect("Create?tab=tab3");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }           
        }
        public IActionResult OnPostRemoveQuestionnaire(Guid id)
        {
           
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                _questionnaireRepository.Archive(id, addedBy);
                Load();
                return Redirect("Create?tab=tab3");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostAddQuestionnaireQsn()
        {            
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                QuestionnaireQsn.ID = Guid.NewGuid();
                QuestionnaireQsn.AddedBy = addedBy;
                QuestionnaireQsn.AddedOn = DateTime.Now;
                if(_questionnaireQsnsRepository.CheckExistence(QuestionnaireQsn)>0)
                {
                    throw new Exception("This question has already been added to the selected questionnaire!");
                }
                _questionnaireQsnsRepository.Create(QuestionnaireQsn);
                Load();
                return Redirect("Create?tab=tab4");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }           
        }
        public IActionResult OnPostLoadQuestionnaireQsns()
        {
            try
            {
                Guid questionnaireQsn = QuestionnaireQsn.Questionnaire;
                QuestionnairesQsnsDT = _questionnaireQsnsRepository.GetAllQuestions();
                Load();
                return Redirect("Create?tab=tab4");
                //QuestionnaireQsn.Questionnaire = questionnaireQsn;
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostRemoveQuestionnaireQsn(Guid id)
        {
            
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                _questionnaireQsnsRepository.Archive(id, addedBy);
                Load();
                return Redirect("Create?tab=tab4");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
}
