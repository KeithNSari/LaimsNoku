namespace LAIMS.Models.Investments
{
    public class UnitTrust
    {
        public Guid ID { get; set; }
        public string UnitTrustName { get; set; }
        public bool IsActive { get; set; }
        public DateTime InceptionDate { get; set; }
        public decimal UnitsIssued { get; set; }
    }
}
