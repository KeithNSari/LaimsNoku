
using LAIMS.Models.LifeProducts;
using System.Data;

namespace LAIMS.Interfaces.Lifeproducts
{ 
    public interface IProductRepository
    {
        int CheckExistence(string ProductName);
        //checks existence of other products with supplied name
        int CheckOtherExistence(string ProductName, Guid ProductID);
        int CheckExistence(Guid ProductID);
        void InsertProduct(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(Guid ID, string DeletedBy, DateTime DeletedOn);
        Product GetProduct(Guid ID);
        List<Product> GetAllProducts();
        DataTable Get();
        DataTable GetByID(Guid ID);
        Guid GetID(string ProductName);
    }
}
