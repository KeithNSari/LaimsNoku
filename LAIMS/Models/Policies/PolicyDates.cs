using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Policies
{
    public class PolicyDates
    {
        [Display(Name = "Application Date")]
        public DateTime? ApplicationDate { get; set; }
        [Display(Name = "Effective Date")]
        public DateTime? EffectiveDate { get; set; }
        [Display(Name = "Expiration Date")]
        public DateTime? ExpirationDate { get; set; }
        [Display(Name = "Proposed Start Date")]
        public DateTime? ProposedStartDate { get; set; }
        [Display(Name = "Preferred Billing Day")]
        public int PreferredBillingDay { get; set; }
        [Display(Name = "Client Signed Date")]
        public DateTime? ClientSignedDate { get; set; }

        [Display(Name = "Agent Signed Date")]
        public DateTime? AgentSignedDate { get; set; }

        [Display(Name = "Application Received Date")]
        public DateTime? DateApplicationReceived { get; set; }
        [Display(Name = "System Date")]
        public DateTime? SystemDate { get; set; }

        [Display(Name = "Commencement Date")]
        public DateTime? CommencementDate { get; set; }

        [Display(Name = "Billing Day")]
        public DateTime? BillingDay { get; set; }

        [Display(Name = "Deduction Start Date")]
        public DateTime? DeductionStartDate { get; set; }       

        [Display(Name = "Anniversary Date")]
        public DateTime? AnniversaryDate { get; set; }

        [Display(Name = "Maturity Date")]
        public DateTime? MaturityDate { get; set; }
    }
}
