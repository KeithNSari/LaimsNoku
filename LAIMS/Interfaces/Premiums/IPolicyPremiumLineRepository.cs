using LAIMS.Models.Premiums;
using LAIMS.Models.Policies;
namespace LAIMS.Interfaces.Premiums
{
    public interface IPolicyPremiumLineRepository
    {
        // CREATE
        void AddPolicyPremiumLine(PolicyPremiumLine policyPremiumLine);

        // READ
        List<PolicyPremiumLine> GetAllPolicyPremiumLines();
        List<PolicyPremiumLine> GetPolicyPremiumLines(int PolicyPremiumID);

        // UPDATE
        void UpdatePolicyPremiumLine(PolicyPremiumLine policyPremiumLine);
        void UpdatePolicyPremiumLines(Guid PolicyID);
        void UpdatePolicyPremiumLines(int PolicyPremiumID);
        bool UpdatePolicyPremiumDates(int PolicyPremiumID, PolicyDates policyDates); 
        void UpdateMainPremiumLines(Guid PolicyID, Guid RequestID);

        // DELETE
        void DeletePolicyPremiumLine(int policyPremiumLineId);
    }
}
