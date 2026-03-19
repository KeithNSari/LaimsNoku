using LAIMS.Models.Premiums;
using LAIMS.Models.Policies;
using System.Data;

namespace LAIMS.Interfaces.Premiums
{
    public interface IPolicyPremiumRepository
    { 
        int AddPolicyPremium(PolicyPremium policyPremium); // initial add
        int AddPolicyPremium(PolicyPremium policyPremium, Guid RequestID); //add driven by requests e.g new money
        int GetPremiumPayer(Guid PolicyID);
        PolicyPremium GetMainPolicyPremium(Guid PolicyID);
        PolicyPremium GetPolicyPremiumByRequest(Guid RequestID);
        List<PolicyPremium> GetAllPolicyPremiums();
        List<PolicyPremiumIntermediary> GetPolicyPremiumsIntermediaries(int PolicyPremiumID);
        DataTable GetInitialPremiumAgents(Guid PolicyID);
        DataTable GetAgents(int PolicyPremiumID);
        public int GetInitialPolicyPremiumID(Guid PolicyID);
        void UpdatePolicyPremium(PolicyPremium policyPremium);
        void UpdatePaymentMethod(PolicyPremium policyPremium, Guid PolicyID);
        void UpdatePremiumAmount(Guid PolicyID);
        void UpdatePremiumAmount(Guid PolicyID, int PolicyPremiumID);
        void UpdateFullPremiumAmount(Guid PolicyID);
		void UpdatePremiumPayer(PolicyPremium policyPremium);
        void UpdatePremiumPayer(int PolicyPremiumID, int MemberID);
        void ArchiveBeneficiaryPremiumLines(Guid PolicyID, int MemberID);
        int GetCurrentBillingDay(Guid PolicyID);
        decimal GetPremiumRates(Guid ProductID, int PolicyBeneficiaryID, decimal Cover);
        decimal GetPremiumRates(Guid ProductID, int PolicyBeneficiaryID, decimal Cover, int RiskGroupID);
        PolicyDates GetPolicyPremiumDates(int PolicyPremiumID);
        CoverDetails GetCoverDetails(Guid PolicyID);
        void UpdateBillingDay(Guid PolicyID, int BillingDay);
        void UpdateStopOrderBillingDay(Guid PolicyID, int PaymentProviderID);
		int UpdatePolicyPremium(Guid PolicyID, string AgentCodes, int PreferredBillingDay);
        void ApprovePremium(int PolicyPremiumID);
        int UpdatePolicyPremiumIntermediaries(int PolicyPremiumID, string AgentCodes);
        void AddPolicyPremiumIntermediaries(int PolicyPremiumID, string AgentCode);
        void AddSupervisors(int PolicyPremiumID);
        DataTable GetPremiumDetails(Guid PolicyID, int ID);
        DataTable GetLatestOrders();
        void DeletePolicyPremium(int policyPremiumId);
    }
}
