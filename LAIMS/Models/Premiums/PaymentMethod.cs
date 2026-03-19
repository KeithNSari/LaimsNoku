using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Premiums
{
    public class PaymentMethod
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(200)]
        public string Method { get; set; }

        [Required]
        public byte Automate { get; set; }

        [Required]
        [StringLength(450)]
        public string AddedBy { get; set; }

        [Required]
        public DateTime AddedOn { get; set; }

        [Required]
        public byte Archived { get; set; }

        [StringLength(450)]
        public string ArchivedBy { get; set; }

        public DateTime? ArchivedOn { get; set; }
    }
}
