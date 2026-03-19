using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LAIMS.Models.Premiums
{
    public class ReversalHeader
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int CurrencyID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        public int ReversalReason { get; set; }

        [MaxLength(500)]
        public string ReversalComment { get; set; }

        [Required]
        public byte Reversed { get; set; } 

        [Required]
        public DateTime ReversedOn { get; set; }

        [Required]
        [MaxLength(256)]
        public string ReversedBy { get; set; }
    }
}
