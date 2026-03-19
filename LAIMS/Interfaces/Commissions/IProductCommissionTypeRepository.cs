using LAIMS.Models.Commissions;

namespace LAIMS.Interfaces.Commissions
{
    public interface IProductCommissionTypeRepository
    { 
        void AddProductCommissionType(ProductCommissionType productCommissionType); 
        List<ProductCommissionType> GetAllProductCommissionTypes(); 
        void UpdateProductCommissionType(ProductCommissionType productCommissionType); 
        void DeleteProductCommissionType(int productCommissionTypeId);
    }
}
