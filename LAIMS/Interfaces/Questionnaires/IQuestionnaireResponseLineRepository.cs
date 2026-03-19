using LAIMS.Models.Questionnaires;

namespace LAIMS.Interfaces.Questionnaires
{
    public interface IQuestionnaireResponseLineRepository
    {
        void InsertQuestionnaireResponseLine(QuestionnaireResponseLine responseLine);
        QuestionnaireResponseLine GetQuestionnaireResponseLine(int id);
        void UpdateQuestionnaireResponseLine(QuestionnaireResponseLine responseLine);
        void ArchiveQuestionnaireResponseLines(Guid HeaderID, string ArchivedBy);
        void DeleteQuestionnaireResponseLine(int id);
    }
}
