using LAIMS.Models.Premiums;
using System.Data;

namespace LAIMS.Interfaces.Premiums
{
    public interface ISuspenseProcessing
    {
        SuspenseProcessingMessage PayPremium(decimal premium, int MemberID);
        List<SuspenseBalance> GetSuspenseBalances(int MemberID);
        int AddSuspenseHeader(SuspenseHeader header);
        void AddSuspenseLine(SuspenseLine line);
        void UpdateSuspenseHeaderBalance(int SuspenseHeaderID, decimal Amount);
        DataTable GetLatestPolicySuspenseData();
        DataTable GetLatestSystemSuspenseData();
        DataTable GetPolicySuspenseData(long BatchID);
        DataTable GetPolicySuspenseData(Guid PolicyID);
        DataTable GetPolicySuspenseBatch(long BatchID);
        DataTable GetSystemSuspenseData(long BatchID);
        decimal GetBatchSuspenseBalance(long BatchID, int SuspenseType);
        DataTable SearchLatestPolicySuspenseData(string PolicyNo);
        DataTable SearchPolicySuspenseDataByDate(DateTime PaymentDate);
        DataTable SearchSytemSuspenseDataByDate(DateTime PaymentDate);
		DataTable GetPaymentDetails(int ID);
        void ReversePolicySuspense(int SuspenseHeaderID, int ReversalReason, string ReversalComment, string ReversedBy);
        void ReverseSuspenseEntry(int SuspenseHeaderID, int ReversalReason, string ReversalComment, string ReversedBy);
        void RefundSystemSuspenseEntry(int SuspenseHeaderID, int ReversalReason, decimal Amount, string ReversalComment, string ReversedBy);
    }
}
