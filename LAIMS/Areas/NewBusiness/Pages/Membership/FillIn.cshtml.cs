using LAIMS.Models.Questionnaires;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuGet.Protocol;

namespace LAIMS.Areas.NewBusiness.Pages.Membership
{
    public class FillInModel : PageModel
    {
        public List<QuestionnaireModel> Questions { get; set; }
        [BindProperty]
        public Dictionary<int, List<string>> Answers { get; set; }
        public void OnGet()
        {
            // Sample questions (customize as needed)
            Questions = new List<QuestionnaireModel>
        {
            new QuestionnaireModel
            {
                QuestionId = 1,
                QuestionText = "What is your favorite programming language?",
                QuestionType = 1,
                Options = new List<string> { "C#", "JavaScript", "Python", "Java" }
            },
            new QuestionnaireModel
            {
                QuestionId = 2,
                QuestionText = "Select your hobbies",
                QuestionType = 2,
                Options = new List<string> { "Reading", "Sports", "Music", "Travel" }
            },
            new QuestionnaireModel
            {
                QuestionId = 3,
                QuestionText = "Tell us about your experience with programming",
                QuestionType = 3
            }
        };
        }

        public IActionResult OnPost()
        {
            string results = string.Empty;
            Questions = new List<QuestionnaireModel>
        {
            new QuestionnaireModel
            {
                QuestionId = 1,
                QuestionText = "What is your favorite programming language?",
                QuestionType = 1,
                Options = new List<string> { "C#", "JavaScript", "Python", "Java" }
            },
            new QuestionnaireModel
            {
                QuestionId = 2,
                QuestionText = "Select your hobbies",
                QuestionType = 2,
                Options = new List<string> { "Reading", "Sports", "Music", "Travel" }
            },
            new QuestionnaireModel
            {
                QuestionId = 3,
                QuestionText = "Tell us about your experience with programming",
                QuestionType = 3
            }
        };
            // Process submitted answers
            foreach (var (questionId, selectedOptions) in Answers)
            {
                var question = Questions.FirstOrDefault(q => q.QuestionId == questionId);

                if (question != null)
                {
                    var response = string.Join(", ", selectedOptions);

                    // You can customize the logic based on the question type
                    if (question.QuestionType == 1)
                    {
                        response = HttpContext.Request.Form[$"answers[{questionId}]"];
                        results = "Question: " + question.QuestionText + "Response: " + response;
                    }

                    // Do something with the response (e.g., store in a database)
                    // For now, we'll just print it to the console
                    
                }
            }

            // Redirect to a thank you page or another appropriate action
            return Redirect("FillIn?id=" + results);
        }
    }
}
