using LAIMS.Models.Questionnaires;
using System.Data;

namespace LAIMS.Interfaces.Questionnaires
{
    public interface IQuestionExpectedResponseRepository
    {
        void Create(QuestionExpectedResponse expectedResponse);
        int CheckExistence(QuestionExpectedResponse expectedResponse);
        QuestionExpectedResponse Read(Guid id);
        void Update(QuestionExpectedResponse expectedResponse);
        void Archive(Guid id, string AddedBy);
        List<QuestionExpectedResponse> GetAll();
        DataTable Get();
    }
}
