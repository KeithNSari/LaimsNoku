namespace LAIMS.Models.Premiums
{
    public class BilledPremium
    {
        public int ID { get; set; }
        public long BatchID { get; set; }
        public int? BillID { get; set; }
        public int PCCID { get; set; }
        public int MemberID { get; set; }
        public Guid PolicyID { get; set; }
        public int PolicyPremiumID { get; set; }
        public int CurrencyID { get; set; }
        public decimal Amount { get; set; }
        public byte PaymentMethodID { get; set; }
        public int? PaymentProviderID { get; set; }
        public byte? Paid { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? AddedOn { get; set; }
    }
}
