using LAIMS.Models.LifeProducts;
using System.Data;

namespace LAIMS.Interfaces.Lifeproducts
{
    public interface IProductQuestionnaireRepository
    {
        int CheckExistence(ProductQuestionnaire productQuestionnaire);
        void InsertQuestionnaire(ProductQuestionnaire productQuestionnaire);
        DataTable GetQuestionnaires(Guid QuestionnaireID);
        void DeleteQuestionnaire(int id, string DeletedBy, DateTime DeletedOn);
    }
}
