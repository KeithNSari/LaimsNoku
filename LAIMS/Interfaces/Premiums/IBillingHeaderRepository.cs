using LAIMS.Models.Premiums;
using System.Data;

namespace LAIMS.Interfaces.Premiums
{
    public interface IBillingHeaderRepository
    {
        PolicyDetailedBalance GetDetailedBalance(string policyNo);
        void CreateBillingHeader(BillingHeader billingHeader);
        int AddAdhocBill(BillingHeader billingHeader);
        bool AddAdhocBillHeader(int BilledPremiumID); 
        void UpdatePolicyBalance(Guid PolicyID, decimal Amount);
        BillingHeader ReadBillingHeader(int id);
        DataTable GetBatches(int StatusID);
        BillingBatch GetBillingBatchById(long batchId);
        void UpdateBillingHeader(BillingHeader billingHeader);
        void DeleteBillingHeader(int id);
    }
}
