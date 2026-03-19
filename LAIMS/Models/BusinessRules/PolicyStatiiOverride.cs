namespace LAIMS.Models.BusinessRules
{
    public class PolicyStatiiOverride
    {
        public int ID { get; set; }
        public Guid PolicyID { get; set; }
        public int Status { get; set; }
        public byte Override { get; set; }
        public string OverridenComment { get; set; }
        public string OverridenBy { get; set; }
        public DateTime OverriddenOn { get; set; }
    }
}
