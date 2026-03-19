using LAIMS.Models.Premiums;
using System.Data;

namespace LAIMS.Interfaces.Premiums
{
    public interface IBilledPremiumRepository
    {
        PolicyDetailedBalance GetDetailedBalance(string policyNo);
        void AddBilledPremium(BilledPremium billedPremium);       
        BilledPremium GetBilledPremiumById(int id);
        decimal GetBillAmount(int BillID);
        int AddPremiumHeader(PremiumHeader premiumHeader);
        void AddPremiumLine(PremiumLine premiumLine);
        void UpdateBilledPremiums(int BillID);
        void UpdateBillingHeader(int BillID);
        void UpdatePolicyBalance(Guid PolicyID, decimal Amount);
        void PremiumBreakDown(int PremiumID, decimal PremiumAmount, Guid PolicyId, int CurrencyID);
        List<BilledPremium> GetAllBilledPremiums();
        DataTable GetLatest();
        DataTable GetFirstUnpaid(string PolicyNo);
        List<BillingSummary> GetPolicyBilledPremiums(string PolicyNo);
        public DataTable Search(string PolicyNo);
        void UpdateBilledPremium(BilledPremium billedPremium);
        void DeleteBilledPremium(int id);
        DataTable GetUnPaid(long BatchID);
        DataTable GetUnPaid(Guid PolicyID);
        DataTable GetLatestPayments();
        DataTable GetLatestAllocationSuspenseEntries();
        DataTable GetAllocationSuspenseEntries(long BatchID);
        DataTable GetAllocationSuspenseEntries(Guid PolicyID);
        DataTable SearchAllocationSuspenseEntries(DateTime PaymentDate);
        DataTable SearchAllocationSuspenseEntries(string SearchTerm);
        DataTable SearchPayments(DateTime PaymentDate);
        DataTable SearchPayments(string SearchTerm);
        void ReversePremiums(PremiumReversalParameters parameters);
        void RefundPremium(PremiumReversalParameters parameters);
    }
}
