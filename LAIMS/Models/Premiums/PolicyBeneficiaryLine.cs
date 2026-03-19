using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Premiums
{
    [Table("PolicyBeneficiariesLines")]
    public class PolicyBeneficiaryLine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int HeaderID { get; set; }
        public Guid RequestID { get; set; } = Guid.Empty;
        public int Approved { get; set; } = 1;
        public byte ProposeToArchive { get; set; }
        public int PolicyPremiumID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Cover { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Contribution { get; set; }

        [Required]
        public Guid ProductID { get; set; }

        [Required]
        public byte Current { get; set; }

        [Column(TypeName = "datetime2(7)")]
        public DateTime? AddedOn { get; set; }

        [MaxLength(450)]
        public string AddedBy { get; set; }
    }
}
