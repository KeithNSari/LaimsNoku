using LAIMS.Models.Questionnaires;
using System.Data;

namespace LAIMS.Interfaces.Questionnaires
{
    public interface IQuestionRepository
    {
        void Create(Question question);
        Question Read(Guid id);
        List<Question> GetAll();
        List<Question> GetAllNonOpen();
        int CheckExistence(Question question);
        DataTable Get();
        void Update(Question question);
        void Delete(Guid id, string AddedBy);
    }
}
