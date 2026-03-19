using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Commissions
{
    public class ProductCommissionType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int CommissionTypeID { get; set; }

        [Required]
        public Guid ProductID { get; set; }

        [Required]
        public byte FunctionType { get; set; }

        [Required]
        public int FunctionID { get; set; }

        [Required] 
        public decimal CommissionRate { get; set; }

        [Required]
        public int CPPStarts { get; set; }

        [Required]
        public int CPPEnds { get; set; }
    }
}
