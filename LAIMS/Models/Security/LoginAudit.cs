namespace LAIMS.Models.Security
{
    public class LoginAudit
    {
        public int AuditID { get; set; } // Will be auto-incremented
        public string UserID { get; set; }
        public DateTime ActionTime { get; set; }
        public bool Success { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public string FailureReason { get; set; }
        public string ActionSource { get; set; }
        public string SessionID { get; set; }
        public string Location { get; set; }
        public string Status { get; set; } 
        public bool? TwoFactorEnabled { get; set; } = false;
        public string TwoFactorMethod { get; set; }
    }

}
