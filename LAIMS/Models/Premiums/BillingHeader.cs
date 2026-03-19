namespace LAIMS.Models.Premiums
{
    public class BillingHeader
    {
        public int ID { get; set; }
        public long BatchID { get; set; }
        public int BillID { get; set; }
        public int PCCID { get; set; }
        public string InvoiceNo { get; set; }
        public Guid PolicyID { get; set; }
        public int MemberID { get; set; }
        public int PremiumPayerAccountID { get; set; }
        public int PaymentProviderID { get; set; }
        public int PaymentMethodID { get; set; }
        public int CurrencyID { get; set; }
        public decimal TotalAmount { get; set; }
        public byte Paid { get; set; }
        public byte Printed { get; set; }
        public DateTime DateDue { get; set; }
        public DateTime AddedOn { get; set; }
        public string AddedBy { get; set; }
    }
}
