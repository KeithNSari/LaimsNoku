namespace LAIMS.Models.Policies
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class PolicyStatiiStaging
    {
        [Key]
        public Guid RequestID { get; set; }

        [Required]
        public Guid PolicyID { get; set; }

        [Required]
        public int PolicyStage { get; set; }

        public Guid? PolicyStatusRuleID { get; set; }

        public int? PolicyStatus { get; set; }

        public int? PolicyStatusReason { get; set; }

        public DateTime? PolicyStatusDate { get; set; }

        [StringLength(500)]
        public string PolicyStatusComment { get; set; }

        [Required]
        [StringLength(450)]
        public string AddedBy { get; set; }

        public DateTime? AddedOn { get; set; }

        public byte? Approved { get; set; }

        [StringLength(450)]
        public string ApprovedBy { get; set; }

        public DateTime? ApprovedOn { get; set; }
    }

}
