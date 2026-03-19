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
    public class PolicyQuestionnaireModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private readonly IQuestionnaireQsnsRepository _questionnaireQsnsRepository;
        private readonly IQuestionnaireResponseLineRepository _questionnaireResponseLineRepository;
        private readonly IQuestionnaireResponseRepository _questionnaireResponseRepository;
        public PolicyQuestionnaireModel(UserManager<ApplicationUser> userManager, IQuestionnaireRepository questionnaireRepository, IQuestionnaireQsnsRepository questionnaireQsnsRepository, IQuestionnaireResponseRepository questionnaireResponseRepository, IQuestionnaireResponseLineRepository questionnaireResponseLineRepository)
        {
            _userManager = userManager;
            _questionnaireRepository = questionnaireRepository;
            _questionnaireQsnsRepository = questionnaireQsnsRepository;
            _questionnaireResponseLineRepository = questionnaireResponseLineRepository;
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
                Questions = _questionnaireQsnsRepository.GetQuestions(questionnaireId);
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
        public IActionResult OnPost()
        {
            try
            {
                if (Answers.Count == 0) throw new Exception("Responses are required!");
                string AddedBy= _userManager.GetUserId(User).ToString();
                _questionnaireResponseLineRepository.ArchiveQuestionnaireResponseLines(QuestionnaireResponseID, AddedBy);
                Questions = _questionnaireQsnsRepository.GetQuestions(QuestionnaireID);
                foreach (var (questionId, selectedOptions) in Answers)
                {
                    var question = Questions.FirstOrDefault(q => q.ID == questionId);
                    if (question != null)
                    {
                        if ((question.QuestionTypeID == 1) || (question.QuestionTypeID == 2))
                        {
                            // var response = HttpContext.Request.Form[$"answers[{questionId}]"];
                            //results = "Question: " + question.QuestionText + "Response: " + response;
                            foreach (string responseID in selectedOptions)
                            {
                                QuestionnaireResponseLine questionnaireResponseLine = new QuestionnaireResponseLine
                                {
                                    HeaderID = QuestionnaireResponseID,
                                    QuestionID = question.ID,
                                    ResponseID = Guid.Parse(responseID),
                                    ResponseText = string.Empty,
                                    AddedOn = DateTime.Now,
                                    AddedBy = AddedBy
                                };
                                _questionnaireResponseLineRepository.InsertQuestionnaireResponseLine(questionnaireResponseLine);
                            }

                        }
                        else
                        {
                            var responseText = selectedOptions.FirstOrDefault();
                            responseText ??= string.Empty;
                            QuestionnaireResponseLine questionnaireResponseLine = new QuestionnaireResponseLine
                            {
                                HeaderID = QuestionnaireResponseID,
                                QuestionID = question.ID,
                                ResponseID = Guid.NewGuid(),
                                ResponseText = responseText,
                                AddedOn = DateTime.Now,
                                AddedBy = AddedBy
                            };
                            _questionnaireResponseLineRepository.InsertQuestionnaireResponseLine(questionnaireResponseLine);
                        }
                    }
                }
                LoadPageData(QuestionnaireID, MemberUID);
                return Page(); //Redirect("PolicyQuestionnaire?questionnaireId=" + QuestionnaireID + "&memberUID" + MemberUID + "&id" + QuestionnaireResponseID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
}
