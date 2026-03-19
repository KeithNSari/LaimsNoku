namespace LAIMS.Models.BusinessRules
{
    public class StatusReport
    {
       public string RuleName { get; set; }
        public  Guid RuleID { get;set; }
       public int SuccessStatus { get; set; }
       public int StatusID { get; set; }
       public int StatusReasonID { get; set; }
       public string? StatusCode { get; set; }
       public string? StatusMessage { get; set; }
    }
}
