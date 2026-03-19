using LAIMS.Models.Commissions;

namespace LAIMS.Interfaces.Commissions
{
    public interface ICommissionTypeRepository
    { 
        void AddCommissionType(CommissionType commissionType);
         
        List<CommissionType> GetAllCommissionTypes(); 
        void UpdateCommissionType(CommissionType commissionType); 
        void DeleteCommissionType(int commissionTypeId);
    }
}
