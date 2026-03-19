using LAIMS.Models.Questionnaires;
using System.Data;

namespace LAIMS.Interfaces.Questionnaires
{
    public interface IQuestionnaireResponseRepository
    {
        void InsertQuestionnaireResponse(QuestionnaireResponse response);
        void PreInsertQuestionnaireResponse(QuestionnaireResponse response);
        void PreInsertQuestionnaireResponse(QuestionnaireResponse response, string QuestionnaireList);
        List<QuestionnaireResponse> GetQuestionnaireResponseHeaders(Guid PolicyID);
        QuestionnaireResponse GetQuestionnaireResponse(Guid id);
        public DateTime? GetResponseDate(Guid id);
        public DataTable GetResponses(Guid Questionnaire, string MemberUID);
        void UpdateQuestionnaireResponse(QuestionnaireResponse response);
        void ArchiveQuestionnaireResponses(Guid MemberUID, Guid PolicyID, string ArchivedBy);
        int CountQuestionnaireResponses(Guid PolicyID);
        void DeleteQuestionnaireResponse(Guid id);
    }
}
