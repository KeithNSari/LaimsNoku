using LAIMS.Models.LifeProducts;
using System.Data;

namespace LAIMS.Interfaces.Lifeproducts
{
    public interface IPolicyTypeDocumentsRepository
    {
        int CheckExistence(PolicyTypeDocument policyTypeDocument);
        void InsertDocument(PolicyTypeDocument policyTypeDocument);
        DataTable GetDocuments(Guid PolicyDefinitionID);
        void ArchiveDocument(int id, string DeletedBy, DateTime DeletedOn);
    }
}
