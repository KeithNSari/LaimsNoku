namespace LAIMS.Models.Premiums
{
    public class PremiumLine
    {
        public int ID { get; set; }
        public int PremiumHeaderID { get; set; }
        public int SuspenseID { get; set; }
        public int PaymentMethodID { get; set; }
        public int PaymentProviderID { get; set; }
        public int InternalAccountsID { get; set; }
        public int ExchangeRateID { get; set; }
        public int SourceID { get; set; }
        public string Source { get; set; }
        public int CurrencyID { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string Details { get; set; }
        public DateTime PaymentDate { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
    }
}
