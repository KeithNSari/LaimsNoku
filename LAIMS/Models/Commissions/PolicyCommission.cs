namespace LAIMS.Models.Commissions
{
    public class PolicyCommission
    {
        public int ID { get; set; }
        public int PolicyTypeCommissionsID { get; set; }
        public int? PolicyPremiumLineID { get; set; }
        public int IntermediaryID { get; set; }
        public decimal Commission { get; set; }
        public int CPPStarts { get; set; }
        public int CPPEnds { get; set; }
        public int StatusID { get; set; }
        public DateTime StatusDate { get; set; }
        public DateTime AddedOn { get; set; }
        public string AddedBy { get; set; }
    }
}
