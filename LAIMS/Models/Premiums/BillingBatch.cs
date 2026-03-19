namespace LAIMS.Models.Premiums
{
    public class BillingBatch
    {
        public long BatchID { get; set; }
        public int PCCID { get; set; }
        public string Provider { get; set; }
        public int PaymentProviderID { get; set; }
        public int PaymentMethodID { get; set; }
        public string PaymentMethod { get; set; }
        public int StatusID { get; set; }
        public int Entries { get; set; }
        public int CurrencyID { get; set; }
        public string Currency { get; set; }
        public decimal AllocationSuspenseAmount { get; set; }
        public decimal PolicySuspenseAmount { get; set; }
        public decimal SystemSuspenseAmount { get; set; }
        public decimal BatchTotalAmount { get; set; }
        public decimal PaidTotalAmount { get; set; }
        public DateTime AddedOn { get; set; }

    }
}
