using Azure;
using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.Membership;
using LAIMS.Models.Questionnaires;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.PolicySetUps.Pages.Questionnaires
{
    [Authorize(Roles = "Admin")]
    public class PreviewModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private readonly IQuestionnaireQsnsRepository _questionnaireQsnsRepository;
        private readonly IQuestionnaireResponseLineRepository _questionnaireResponseLineRepository;
        private readonly IQuestionnaireResponseRepository _questionnaireResponseRepository;
        public PreviewModel(UserManager<ApplicationUser> userManager, IQuestionnaireRepository questionnaireRepository, IQuestionnaireQsnsRepository questionnaireQsnsRepository, IQuestionnaireResponseRepository questionnaireResponseRepository, IQuestionnaireResponseLineRepository questionnaireResponseLineRepository)
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
        public void OnGet(Guid id)
        {           
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                Questions = _questionnaireQsnsRepository.GetQuestions(id);
                LoadPageData(id);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
        }
        private void LoadPageData(Guid id)
        {
            string memberID = "00000000-0000-0000-0000-000000000000"; //this is the default ID
            QuestionnaireTitle = _questionnaireRepository.GetTitle(id);
            if (QuestionnaireTitle == null)
            {
                throw new Exception("Questionnaire not found");
            }           
            PreviousResponsesDT = _questionnaireResponseRepository.GetResponses(id, memberID);
            PreviousResponseDate = _questionnaireResponseRepository.GetResponseDate(id);
        }
        public IActionResult OnPost(Guid id)
        {
            try
            { 
                if (Answers.Count == 0) throw new Exception("Responses are required!");
                QuestionnaireResponse questionnaireResponse = new QuestionnaireResponse
                {
                    Questionnaire = id,
                    ID = Guid.NewGuid(),
                    AddedBy = _userManager.GetUserId(User).ToString(),
                    AddedOn = DateTime.Now
                };
                _questionnaireResponseRepository.InsertQuestionnaireResponse(questionnaireResponse);
                Questions = _questionnaireQsnsRepository.GetQuestions(id);
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
                                    HeaderID = questionnaireResponse.ID,
                                    QuestionID = question.ID,
                                    ResponseID = Guid.Parse(responseID),
                                    ResponseText = string.Empty,
                                    AddedOn = DateTime.Now,
                                    AddedBy = _userManager.GetUserId(User).ToString()
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
                                HeaderID = questionnaireResponse.ID,
                                QuestionID = question.ID,
                                ResponseID = Guid.NewGuid(),
                                ResponseText = responseText,
                                AddedOn = DateTime.Now,
                                AddedBy = _userManager.GetUserId(User).ToString()
                            };
                            _questionnaireResponseLineRepository.InsertQuestionnaireResponseLine(questionnaireResponseLine);
                        }
                    }
                }
                return Redirect("Preview?id=" + id);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
}
