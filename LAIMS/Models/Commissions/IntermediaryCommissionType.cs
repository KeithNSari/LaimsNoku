using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Commissions
{
    public class IntermediaryCommissionType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int CommissionTypeID { get; set; }

        [Required]
        public int IntermediaryID { get; set; }
    }
}
