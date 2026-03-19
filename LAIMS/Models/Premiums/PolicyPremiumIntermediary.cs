namespace LAIMS.Models.Premiums
{
    public class PolicyPremiumIntermediary
    {
        public int  ID { get; set; }
        public int PolicyPremiumID { get; set; }
        public int IntermediaryID { get; set; }
        public int IntermediaryTypeID { get; set; }
    }
} 