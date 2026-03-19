namespace LAIMS.Models.Premiums
{
    public class PolicyBeneficiaryLineDetail
    {
        public int LineId { get; set; }
        public string Product { get; set; }
        public int CategoryID { get; set; }
        public byte PolicyBeneficiariesLineApproved { get; set; }
        public byte ProposeToArchive { get; set; }
        public string LIRole { get; set; }
        public string FullName { get; set; }
        public string Relationship { get; set; }
        public DateTime DOB { get; set; }
        public DateTime? CommencementDate { get; set; }
        public string IDType { get; set; }
        public string ID { get; set; }
        public decimal Premium { get; set; }
        public decimal Cover { get; set; }
    }
}
