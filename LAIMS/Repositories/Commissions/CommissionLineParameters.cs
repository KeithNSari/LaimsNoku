namespace LAIMS.Repositories.Commissions
{
    public class CommissionLineParameters
    {
        public Guid PolicyTypeID { get; set; }
        public Guid PolicyID { get; set; }
        public int PremiumID { get; set; }
        public long BatchID { get; set; }
        public Guid ProductID { get; set; }
        public byte MainProduct { get; set; }
        public int CurrencyID { get; set; }
        public int PolicyPremiumID { get; set; }
        public decimal BasicPolicyPremium { get; set; }
        public int PolicyPremiumLinesID { get; set; }
        public int BilledPremiumID { get; set; }
        public DateTime DueDate { get; set; }
        public int DueYear { get; set; }
        public int DueMonth { get; set; }
        public int PolicyAge { get; set; }
        public DateTime CommencementDate { get; set; }
        public string AddedBy { get; set; }
    }
}
