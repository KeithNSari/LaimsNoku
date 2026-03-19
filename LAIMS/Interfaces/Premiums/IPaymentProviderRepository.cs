using LAIMS.Models.Banking;
using LAIMS.Models.Premiums;
using System.Data;

namespace LAIMS.Interfaces.Premiums
{
    public interface IPaymentProviderRepository
    {
        int AddPaymentProvider(PaymentProvider paymentProvider);
        List<PaymentProvider> GetAllPaymentProviders();
        List<PaymentProvider> GetPaymentProviders(int PaymentMethodID);
        List<PaymentProvider> GetDebitOrderProviderList();
		List<StopOrder> GetStopOrdersByProvider(int ProviderID);
        DataTable GetDebitOrderProviders();
        BankAccountFormat GetBankAccountFormat(int PaymentProviderID);
        void UpdatePaymentProvider(PaymentProvider paymentProvider);
        void DeletePaymentProvider(int entryNo);
        bool AddBatch(int PCCID, long BatchID, int PaymentMethodID, DateTime AddedOn);
        bool BillStopOrderPremiums(long BatchID, int PaymentProviderID, int PCCID);
        bool BillStopOrderPremiums(long BatchID, int PaymentProviderID, int PCCID, DateTime CollectionDate);
		bool BillDebitOrderPremiums(long BatchID, int PaymentProviderID, int BillingDay, int PCCID);
        bool BillDebitOrderPremiums(long BatchID, int BillingDay, int PCCID);
        bool BillDirectPaymentPremiums(long BatchID, int BillingDay, int CurrencyID);
        void AddBillID(long BatchID, int BillingDay, DateTime LastBilled);
        void AddBilledPolicies(long BatchID);
        bool AddBillHeaders(long BatchID, int BillingDay, DateTime LastBilled);
        DataTable GetLatestStopOrders();
        DataTable GetDebitOrderData(string ProcedureName, long BatchID);
		DataTable GetStopOrderData(string ProcedureName);
        DataTable GeStopOrderData(string ProcedureName, int PCCID);
        string GetSPName(int PCCID);
        public string GetDebitOrderName(int PCCID);
        string GetStopOrderCode(int PCCID);
        int GetFileFormat(int PCCID);
        DataTable GetLatestDebitOrders();
        int GetFirstPCCID(int PaymentProviderID);
        DataTable GetLatestBillingBatches(int PaymentMethodID);
        DataTable GetBillingBatchData(long BatchID);
    }
}
