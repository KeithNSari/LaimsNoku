namespace LAIMS.Models.Investments
{
    public class UnitTrustsLine
    {
        public int ID { get; set; }
        public Guid HeaderID { get; set; }
        public decimal? ResidualValue { get; set; }
        public byte? ResidualValueIsPercentage { get; set; }
        public int CurrencyID { get; set; }
        public decimal MinimumCashWithdrawal { get; set; }
        public decimal? MinimumSurrenderValue { get; set; }
        public int WaitingPeriod { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int? ValueMode { get; set; }
        public string AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
    }
}
