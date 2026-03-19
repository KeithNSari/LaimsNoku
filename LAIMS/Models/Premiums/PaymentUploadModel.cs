using System.Data;

namespace LAIMS.Models.Premiums
{
    public class PaymentUploadModel
    {
        public Guid UploadID { get; set; }
        public long BillingBatchID { get; set; }
        public int IdentityType { get; set; }
        public int PaymentMethod { get; set; }
        public int PaymentProvider { get; set; }
        public int PCCID { get; set; }
        public int IdentifierColumn { get; set; }
        public int CurrencyID { get; set; }
        public int AmountColumn { get; set; }
        public int ReferenceColumn { get; set; }
        public int DatePaidColumn { get; set; }
        public DataTable PaymentData { get; set; }
        public string AddedBy { get; set; }
    }
}
