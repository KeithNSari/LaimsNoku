using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LAIMS.Models.BusinessRules
{    
    public class ObjectRulesStatiiHistory
    { 
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long ID { get; set; }

            [Required]
            public Guid UID { get; set; }

            public Guid? RequestID { get; set; }

            public int? SourceID { get; set; }

            [Required]
            public int MemberID { get; set; }

            public Guid? StatusRuleID { get; set; }
            public int? SuccessStatus { get; set; }

            public int? Status { get; set; }

            public int? StatusReason { get; set; }

            public DateTime? StatusDate { get; set; }

            [MaxLength(500)]
            public string StatusComment { get; set; }

            [MaxLength(450)]
            public string StatusAddedBy { get; set; } 

    }
}
