using LAIMS.Models.Premiums;

namespace LAIMS.Interfaces.Premiums
{
    public interface IProcessPayments
    {
        void RunBatch(PaymentUploadModel paymentUploadModel);
        void UpdateBillingBatch(long BatchID);
        decimal GetBatchAllocationSuspenseBalance(long BatchID);
        bool UpdateBatchBalances(long BatchID);
        BillingBatch GetBillingBatchById(long batchId);
        decimal AdhocPayment(Guid PolicyID, string Reference, decimal Amount, int PaymentMethodID, int PaymentID, DateTime PaymentDate, string AddedBy);
        void UpdatePolicyStatus(Guid PolicyID);
    }
}
