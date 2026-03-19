using LAIMS.Models.Commissions;
using System.Data;

namespace LAIMS.Interfaces.Commissions
{
    public interface IPolicyCommissionRepository
    {
        List<PolicyCommission> GetAllPolicyCommissions();
        DataTable GetLatest();
        DataTable GetPayments();
        DataTable Search(int CurrencyID, int StartMonth, int EndMonth, int Year);
        DataTable Search(int IntermediaryID, int CurrencyID, int StartMonth, int EndMonth, int Year);
        CommissionSearchHeader GetSearchHeader(int IntermediaryID, int CurrencyID, int StartMonth, int EndMonth, int Year);
        CommissionSearchHeader GetSearchHeader(int CurrencyID, int StartMonth, int EndMonth, int Year);
        PolicyCommission GetPolicyCommissionById(int id);
        void AddPolicyCommission(PolicyCommission policyCommission);
        decimal CalculateCommission(string ProcedureName, int PolicyTypeCommissionID, decimal CommissionRate, Guid PolicyID, int PolicyPremiumID);
        void UpdatePolicyCommission(PolicyCommission policyCommission);
        void DeletePolicyCommission(int id);
    }
}
