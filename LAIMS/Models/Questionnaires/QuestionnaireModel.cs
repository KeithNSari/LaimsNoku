namespace LAIMS.Models.Questionnaires
{
    public class QuestionnaireModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int QuestionType  { get; set; }
        public List<string> Options { get; set; }
    } 
}
