namespace LAIMS.Models.Premiums
{
    public class BillingMessage
    {
        public int ID { get; set; }
        public int BillID { get; set; }
        public int Status { get; set; }
        public int StatusReason { get; set; }
        public string Message { get; set; }
        public string AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
    }
}
