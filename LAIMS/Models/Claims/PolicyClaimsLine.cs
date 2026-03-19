namespace LAIMS.Models.Claims
{
    public class PolicyClaimsLine
    {
        public int ID { get; set; }
        public int HeaderID { get; set; }
        public int? PTLBenefitID { get; set; }
        public int PolicyBeneficiariesLineID { get; set; }
        public int? PolicyUnitsID { get; set; }
        public decimal Amount { get; set; }
    }
}
