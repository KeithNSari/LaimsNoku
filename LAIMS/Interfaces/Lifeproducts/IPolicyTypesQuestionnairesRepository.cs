using LAIMS.Models.LifeProducts;
using System.Data;

namespace LAIMS.Interfaces.Lifeproducts
{
    public interface IPolicyTypesQuestionnairesRepository
    {
        int CheckExistence(PTQuestionnaire pTQuestionnaire);
        void InsertPTQuestionnaire(PTQuestionnaire pTQuestionnaire);
        void ArchivePTQuestionnaire(PTQuestionnaire pTQuestionnaire);
        DataTable GetByPolicyType(Guid PolicyTypeID);
        DataTable Get();
    }
}
