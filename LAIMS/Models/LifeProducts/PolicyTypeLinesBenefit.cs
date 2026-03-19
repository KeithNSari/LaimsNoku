namespace LAIMS.Models.LifeProducts
{
    public class PolicyTypeLinesBenefit
    {
        public int ID { get; set; }
        public Guid PTLID { get; set; }
        public Guid ProductID { get; set; }
        public byte TestedBusiness { get; set; }
        public int WaitingPeriod { get; set; }
        public int WPDurationUnit { get; set; }
        public string? WPDurationUnitDesc { get; set; } 
        public decimal Benefit { get; set; }
        public decimal MaximumBenefit { get; set; } 
        public decimal Contribution { get; set; }
        public byte Current { get; set; }
        public DateTime AddedOn { get; set; }
        public string AddedBy { get; set; }
        public byte? Archived { get; set; }
        public string ArchivedBy { get; set; }
        public string ArchivedComment { get; set; }
        public DateTime? ArchivedOn { get; set; }
        public byte? Deleted { get; set; }
        public string DeletedBy { get; set; }
        public string DeletedComment { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
