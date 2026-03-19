using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Commissions
{
    public class PolicyPremiumLinesCommission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int PolicyPremiumsLinesID { get; set; }

        [Required]
        public int IntermediaryCommissionTypeID { get; set; }

        [Required]
        public int ProductCommissionTypeID { get; set; }

        [Required]
        public int StatusID { get; set; }

        [Required] 
        public DateTime StatusDate { get; set; }

        [Required] 
        public decimal Commission { get; set; }
    }
}
