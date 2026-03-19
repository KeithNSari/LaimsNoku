namespace LAIMS.Models.Premiums
{
    public class SuspenseHeader
    {
        public int ID { get; set; }
        public long BatchID { get; set; }
        public int MemberID { get; set; }
        public int PaymentMethodID { get; set; }
        public int PaymentTypeID { get; set; }
        public int PaymentID { get; set; }
        public int InternalBankAccountID { get; set; }
        public byte? SuspenseType { get; set; }
        public Guid? PolicyID { get; set; }
        public string? TargetPolicyNo { get; set; }
        public int? SourceID { get; set; }
        public string Source { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Reference { get; set; }
        public int StatusID { get; set; }
        public decimal ProcessedAmount { get; set; }
        public int? CurrencyID { get; set; }
        public decimal Balance { get; set; }
        public DateTime AddedOn { get; set; }
        public string AddedBy { get; set; }
    }
}
