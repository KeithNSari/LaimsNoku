using LAIMS.Models.LifeProducts;
using System.Data;

namespace LAIMS.Interfaces.Lifeproducts
{
    public interface IDocumentsRepository
    {
        int CheckExistence(string DocumentName);
        int CheckExistenceOther(string DocumentName, Guid ID);
        void InsertDocument(Document document);
        void ArchiveDocument(Guid ID, string AddedBy, DateTime AddedOn);
        void UpdateDocument(Document document);
        Document GetDocument(Guid ID); 
        List<Document> GetAllDocuments();
        List<Document> GetClaimRequiredDocuments(Guid RequestID);
        DataTable Get();
    }
}
