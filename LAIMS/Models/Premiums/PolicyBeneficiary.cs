using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Premiums
{
    [Table("PolicyBeneficiaries")]
    public class PolicyBeneficiary
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public Guid HeaderID { get; set; }

        [Required]
        public int MemberID { get; set; }
        [Required]
        public int IDType { get; set; }

        [Required]
        public int RelationshipID { get; set; }

        [Required]
        public int LIRole { get; set; }
        [Required]
        public byte Insured { get; set; } =0;
        [Required]
        public byte Beneficiary { get; set; } = 0;
        [Required]
        public int RiskGroupID { get; set; } = -1;

        [Column(TypeName = "datetime2(7)")]
        public DateTime? AddedOn { get; set; }

        [MaxLength(450)]
        public string AddedBy { get; set; }
    }
}
