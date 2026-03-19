namespace LAIMS.Models.Claims
{
    public class DeathRecord
    {
        public Guid RequestID { get; set; }
        public int PolicyClaimID { get; set; } = 0;
        public string MemberName { get; set; }
        public int MemberID { get; set; } = 0;
        public DateTime DateOfDeath { get; set; }
        public string? BurialOrderNo { get; set; }
        public string? DeathCertificateNo { get; set; }
        public int EventCauseID { get; set; }
        public string EventCauseName { get; set; }  
        public string? CauseDetails { get; set; }
        public DateTime? DateHealthAffected { get; set; }
        public string? Place { get; set; }
        public string? Hospital { get; set; }
        public string? PoliceStation { get; set; }
        public string? CaseReferenceNo { get; set; } 
        public string AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
    }
}
