using LAIMS.Models.LifeProducts;
using System.Data;

namespace LAIMS.Interfaces.Lifeproducts
{
    public interface IPolicyTypeLinesBenefitsRepository
    {
        void InsertBenefit(PolicyTypeLinesBenefit policyTypeLinesBenefit);
        DataTable GetBenefits(Guid PolicyDefinitionID);
        void ArchiveBenefit(int id, string DeletedBy, DateTime DeletedOn);
    }
}
