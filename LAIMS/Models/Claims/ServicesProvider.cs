namespace LAIMS.Models.Claims
{
    public class ServicesProvider
    {
        public int ID { get; set; }
        public string ServiceProviderName { get; set; }
        public int BankID { get; set; }
        public string BranchCode { get; set; }
        public string BankAccountNo { get; set; }
        public string BankAccountNoConfirm { get; set; }
        public string BankAccountName { get; set; }
    }
}
