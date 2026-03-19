namespace LAIMS.Models.Commissions
{
    public class CommissionSearchHeader
    {
        public int IntermediaryID { get; set; }
        public string AgentName { get; set; }
        public string AgentCode { get;set; }
        public string CurrencyName { get; set; }
        public decimal IntermediaryCommission { get; set; }
        public decimal OverridingCommission { get; set; }
    }
}
