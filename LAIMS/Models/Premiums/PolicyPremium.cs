
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Premiums
{
    public class PolicyPremium
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public Guid? HeaderID { get; set; }

        public int? PaymentFrequencyID { get; set; }

        public int? PaymentMethodID { get; set; }

        public int? PaymentProviderID { get; set; } 

        [Required]
        public int PremiumPayer { get; set; }

        public int? PremiumPayerAccountID { get; set; }

        [Required] 
        public decimal Premium { get; set; }

        public byte? AuthoriseAutoPayment { get; set; }

        [Required]
        public byte Current { get; set; }
        public DateTime? AddedOn { get; set; }

        [StringLength(450)]
        public string AddedBy { get; set; }
    }
}
