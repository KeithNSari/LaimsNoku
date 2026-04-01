namespace LAIMS.Models.Claims
{
    public class ClaimTypeLine
    {
        public int ID { get; set; }
        public Guid ProductID { get; set; }
        public int ClaimTypeID { get; set; }
        public DateTime AddedOn { get; set; }
        public string AddedBy { get; set; } = string.Empty;
    }

    public class DisabilityPremiumWaiver
    {
        public int ID { get; set; }
        public Guid RequestID { get; set; }
        public Guid PolicyID { get; set; }
        public int MemberID { get; set; }
        public DateTime NotificationDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public int StatusID { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime AddedOn { get; set; }
        public string AddedBy { get; set; } = string.Empty;
    }

    public class DeathPremiumWaiver
    {
        public int ID { get; set; }
        public Guid RequestID { get; set; }
        public Guid PolicyID { get; set; }
        public int MemberID { get; set; }
        public int DeathRecordID { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int StatusID { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime AddedOn { get; set; }
        public string AddedBy { get; set; } = string.Empty;
    }
}
