namespace LAIMS.Models.Claims
{
    public class PolicyClaim
    {
        public int ID { get; set; }
        public Guid PolicyID { get; set; }
        public Guid RequestID { get; set; }
        public int? EventID { get; set; }
        public int? ClaimantID { get; set; }
        public DateTime? ClaimDate { get; set; }
        public int ClaimTypeID { get; set; }
        public int ValueMode { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? PaymentMethodID { get; set; }
        public int? CurrencyID { get; set; }
        public byte? Paid { get; set; }
        public DateTime? DatePaid { get; set; }
        public string PaidComment { get; set; }
        public int StatusID { get; set; }
        public DateTime StatusDate { get; set; }
        public string? StatusComment { get; set; }
        public string StatusAddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
        public string AddedBy { get; set; }
    }
}
