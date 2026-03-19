namespace LAIMS.Models.Premiums
{
    public class ReversalLine
    {
        public int ID { get; set; }
        public int HeaderID { get; set; }
        public int OriginalPaymentID { get; set; }
        public int SourceID { get; set; }
        public decimal Amount { get; set; }
        public byte Reversed { get; set; }
        public DateTime ReversedOn { get; set; }
        public string ReversedBy { get; set; }
    }
}
