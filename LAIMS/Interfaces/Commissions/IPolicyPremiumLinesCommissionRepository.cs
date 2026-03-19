using LAIMS.Models.Commissions;

namespace LAIMS.Interfaces.Commissions
{
    public interface IPolicyPremiumLinesCommissionRepository
    {
        // CREATE
        void AddPolicyPremiumLinesCommission(PolicyPremiumLinesCommission policyPremiumLinesCommission);

        // READ
        List<PolicyPremiumLinesCommission> GetAllPolicyPremiumLinesCommissions();

        // UPDATE
        void UpdatePolicyPremiumLinesCommission(PolicyPremiumLinesCommission policyPremiumLinesCommission);

        // DELETE
        void DeletePolicyPremiumLinesCommission(int policyPremiumLinesCommissionId);
    }
}
