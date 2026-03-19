namespace LAIMS.Models.Investments
{
    public class PBLSplit
    {
        public int ID { get; set; }
        public Guid BatchID { get; set; }
        public Guid PBLUID { get; set; }
        public Guid PolicyID { get; set; }
        public int? PBLID { get; set; }
        public int PolicyBeneficiaryID { get; set; }
        public decimal SplitPercentage { get; set; }
        public DateTime? AddedOn { get; set; }
        public string AddedBy { get; set; }
    }
}
