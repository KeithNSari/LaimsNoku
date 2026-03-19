using LAIMS.Models.LifeProducts;
using System.Data;

namespace LAIMS.Interfaces.Lifeproducts
{
    public interface IPolicyTypeLinesBenefitsDocumentsRepository
    {
        int CheckExistence(PolicyTypeLinesBenefitDocument policyTypeLinesBenefitDocument);
        void InsertDocument(PolicyTypeLinesBenefitDocument policyTypeLinesBenefitDocument);
        DataTable GetDocuments(Guid PolicyDefinitionID);
        void ArchiveDocument(int id, string DeletedBy, DateTime DeletedOn);
    }
}
