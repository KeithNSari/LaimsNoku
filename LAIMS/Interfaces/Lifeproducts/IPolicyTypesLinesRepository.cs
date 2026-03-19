using LAIMS.Models.LifeProducts;
using System.Data;

namespace LAIMS.Interfaces.Lifeproducts
{
    public interface IPolicyTypesLinesRepository
    {
        int CheckExistenceOfMain(Guid ID);
        int CheckExistence(Guid PolicyTypeID, Guid ProductID);
        void InsertPolicyTypeLine(PolicyTypesLines policyTypeLine);
        PolicyTypesLines GetPolicyTypeLineById(Guid id);
        List<Product> GetAllProducts(Guid ID);
        List<Product> GetAllInvestmentProducts(Guid ID);
        List<Product> GetAllNonInvestmentProducts(Guid ID);
        void UpdatePolicyTypeLine(PolicyTypesLines policyTypeLine);
        void Archive(Guid ID, string AddedBy, DateTime AddedOn);
        void DeletePolicyTypeLine(Guid id);
        DataTable GetProducts(Guid PolicyDefinitionID);
    }
}
