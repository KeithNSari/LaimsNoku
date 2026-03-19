using LAIMS.Models.Questionnaires;
using System.Data;

namespace LAIMS.Interfaces.Questionnaires
{
    public interface IQuestionnaireRepository
    {
        int CheckExistence(Questionnaire questionnaire);
        void Create(Questionnaire questionnaire);
        Questionnaire Read(Guid id);
        List<Questionnaire> GetAll();
        DataTable Get();
        public string? GetTitle(Guid id);
        void Update(Questionnaire questionnaire);
        void Archive(Guid id, string AddedBy);
    }
}
