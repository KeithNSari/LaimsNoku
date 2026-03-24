using LAIMS.Models.Premiums;
using System.Data;

namespace LAIMS.Interfaces.Premiums
{
    public interface IPaymentRepository
    {
        int InsertPayment(Payment payment);
        void UploadStatement(string ExcelFilePath, long BatchID, string AddedBy);
        void UploadDebitOrders(string ExcelFilePath, long BatchID, int PaymentProviderID, string AddedBy);
        void UploadPremiumRates(DataTable PremiumRatesDT, Guid MediaUploadID, DateTime EffectiveDate, string AddedBy);
        void UploadRates(DataTable RatesDT, Guid MediaUploadID, DateTime EffectiveDate, string Destination, string AddedBy);
        Payment GetPaymentById(int id);
        List<Payment> GetAllPayments(); 
        DataTable GetLatestCashBatchHeaders();
        DataTable GetLatestPremiumRatesHeaders();
        DataTable GetLatestAllocationRates();
        DataTable GetLatestCoverRates();
        DataTable GetLatestCoverLevels();
        void UpdatePayment(Payment payment);
        void DeletePayment(int id);
    }
}
