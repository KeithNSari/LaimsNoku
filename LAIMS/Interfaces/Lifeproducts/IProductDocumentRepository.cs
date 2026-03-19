using LAIMS.Models.LifeProducts;
using System.Data;

namespace LAIMS.Interfaces.Lifeproducts
{
    public interface IProductDocumentRepository
    {
        int CheckExistence(ProductDocument productDocument);
        void InsertDocument(ProductDocument productDocument);
        DataTable GetDocuments(Guid DocumentID);
        void DeleteDocument(int id, string DeletedBy, DateTime DeletedOn);
    }
}
