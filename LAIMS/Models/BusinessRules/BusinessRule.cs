using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.BusinessRules
{
    [Table("Rules")]
    public class BusinessRule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntryNo { get; set; }

        [Required]
        [MaxLength(500)]
        public string RuleName { get; set; }

        [Required]
        [MaxLength(200)]
        public string StoredProcedure { get; set; }

        [Required]
        public Guid ID { get; set; }

        [Required]
        public DateTime AddedOn { get; set; }

        [MaxLength(450)]
        public string AddedBy { get; set; }
    }
}
