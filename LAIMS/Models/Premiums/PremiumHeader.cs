namespace LAIMS.Models.Premiums
{
    public class PremiumHeader
    {
        public int ID { get; set; }
        public int BillingID { get; set; }
        public int BilledPremiumID { get; set; }
        public int PaymentID { get; set; }
        public string DocumentNo { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DatePaymentReceived { get; set; }
        public DateTime DatePaymentRecorded { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
    }
}
