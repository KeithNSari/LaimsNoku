namespace LAIMS.Models.Premiums
{
    public class Payment
    {
        public int ID { get; set; }
        public long BatchID { get; set; }
        public int CurrencyID { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public int InternalAccountNoID { get; set; }
        public int PaymentMethod { get; set; }
        public int PaymentType { get; set; }
        public int PaymentProvider { get; set; }
        public int? PaidBy { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Details { get; set; }
        public decimal? PolicySuspenseAmount { get; set; }
        public decimal? PystemSuspenseAmount { get; set; }
        public decimal? AllocationSuspenseAmount { get; set; }
		public string? PolicyNo { get; set; }
		public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public string Field6 { get; set; }
        public string Field7 { get; set; }
        public string Field8 { get; set; }
        public string Field9 { get; set; }
        public string Field10 { get; set; }
        public DateTime? AddedOn { get; set; }
        public string AddedBy { get; set; }
        public byte? Reversed { get; set; }
        public DateTime? ReversedOn { get; set; }
        public string ReversedBy { get; set; }
    }
}
