using LAIMS.Models.Policies;

namespace LAIMS.Models.Premiums
{
    public class BillingSummary
    {
        public int BatchID { get; set; }
        public int BillID { get; set; }
        public string InvoiceNo { get; set; }
        public string PolicyNo { get; set; }
        public string Currency { get; set; }
        public decimal BilledAmount { get; set; }
        public string Paid { get; set; }
        public string DateDue { get; set; }
    }
}
