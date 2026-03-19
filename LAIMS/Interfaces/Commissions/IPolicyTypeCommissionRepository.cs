using LAIMS.Models.Commissions;
using System.Data;

namespace LAIMS.Interfaces.Commissions
{
    public interface IPolicyTypeCommissionRepository
    {
        List<PolicyTypeCommission> GetAllPolicyTypeCommissions();
        PolicyTypeCommission GetPolicyTypeCommissionById(int id);
        DataTable Get();
        List<PolicyTypeCommission> GetPolicyTypeCommissions(Guid PolicyTypeID, Guid ProductID, int IntermediaryTypeID);
        void AddPolicyTypeCommission(PolicyTypeCommission policyTypeCommission);
        void UpdatePolicyTypeCommission(PolicyTypeCommission policyTypeCommission);
        void DeletePolicyTypeCommission(int id);
    }
}
