using Azure;
using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.Membership;
using LAIMS.Models.Questionnaires;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class PolicyQuestionnairesResponsesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IQuestionnaireRepository _questionnaireRepository; 
        private readonly IQuestionnaireResponseRepository _questionnaireResponseRepository;
        public PolicyQuestionnairesResponsesModel(UserManager<ApplicationUser> userManager, IQuestionnaireRepository questionnaireRepository, IQuestionnaireResponseRepository questionnaireResponseRepository)
        {
            _userManager = userManager;
            _questionnaireRepository = questionnaireRepository; 
            _questionnaireResponseRepository = questionnaireResponseRepository;
        }
        public DataTable PreviousResponsesDT { get; set; }
        [BindProperty]
        public string? QuestionnaireTitle { get; set; }
        public DateTime? PreviousResponseDate { get; set; }
        public List<Question> Questions { get; set; }
        [BindProperty]
        public Dictionary<Guid, List<string>> Answers { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public Guid MemberUID { get; set; }
        [BindProperty]
        public Guid QuestionnaireResponseID { get; set; }
        [BindProperty]
        public Guid QuestionnaireID { get; set; }
        public void OnGet(Guid questionnaireId, Guid memberUID, Guid ID)
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString; 
                QuestionnaireResponseID = ID;
                MemberUID = memberUID;
                QuestionnaireID = questionnaireId;
                LoadPageData(questionnaireId, memberUID);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
        }
        private void LoadPageData(Guid questionnaireId, Guid memberUID)
        {
            QuestionnaireTitle = _questionnaireRepository.GetTitle(questionnaireId);
            if (QuestionnaireTitle == null)
            {
                throw new Exception("Questionnaire not found");
            }
            PreviousResponsesDT = _questionnaireResponseRepository.GetResponses(questionnaireId, memberUID.ToString());
            PreviousResponseDate = _questionnaireResponseRepository.GetResponseDate(questionnaireId);
        } 
    }
}
