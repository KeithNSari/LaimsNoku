using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.BusinessRules
{
    [Table("ObjectRules")]
    public class ObjectRule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntryNo { get; set; }

        [Required]
        public Guid ObjectRuleID { get; set; }

        [Required]
        public Guid ObjectID { get; set; }

        [Required]
        public Guid RuleID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Filter { get; set; }

        [Required]
        public DateTime AddedOn { get; set; }

        [MaxLength(450)]
        public string AddedBy { get; set; }
    }
}
