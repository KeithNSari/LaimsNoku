namespace LAIMS.Models.Premiums
{
    public class PremiumReversalParameters
    {
        public Guid PolicyID { get; set; }
        public string? CorrectPolicyNo { get; set; }
        public int PremiumHeaderID { get; set; }
        public int BillID { get; set; }
        public string ReversedBy { get; set; }
        public int ReversalReason { get; set; }
        public string ReversalComment { get; set; }
    }
}
