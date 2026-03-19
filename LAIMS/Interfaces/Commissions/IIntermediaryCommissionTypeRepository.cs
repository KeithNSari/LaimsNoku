using LAIMS.Models.Commissions;

namespace LAIMS.Interfaces.Commissions
{
    public interface IIntermediaryCommissionTypeRepository
    {
        // CREATE
        void AddIntermediaryCommissionType(IntermediaryCommissionType intermediaryCommissionType);

        // READ
        List<IntermediaryCommissionType> GetAllIntermediaryCommissionTypes();

        // UPDATE
        void UpdateIntermediaryCommissionType(IntermediaryCommissionType intermediaryCommissionType);

        // DELETE
        void DeleteIntermediaryCommissionType(int intermediaryCommissionTypeId);
    }
} 
