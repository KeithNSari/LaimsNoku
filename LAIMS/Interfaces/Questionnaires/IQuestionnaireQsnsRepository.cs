using LAIMS.Models.Questionnaires;
using System.Data;

namespace LAIMS.Interfaces.Questionnaires
{
    public interface IQuestionnaireQsnsRepository
    {
        int CheckExistence(QuestionnaireQsn questionnaireQsns);
        void Create(QuestionnaireQsn questionnaireQsns);
        QuestionnaireQsn Read(Guid id);
        List<Question> GetQuestions(Guid QuestionnaireID);
        DataTable GetAllQuestions();
        void Update(QuestionnaireQsn questionnaireQsns);
        void Archive(Guid id, string AddedBy);
        List<QuestionnaireQsn> GetAll();
    }
}
