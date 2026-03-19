using System.ComponentModel.DataAnnotations;
namespace LAIMS.Models.Premiums
{
    public class PaymentProvider
    {   
        public int ID { get; set; }
        [Key]
        public int PCCID { get; set; }
        [Required]
        public int MemberID { get; set; }
        [Required]
        public Guid ProviderUID { get; set; }       
        [Required]
        public string ProviderName { get; set; }

        [Required]
        public int PaymentMethodID { get; set; }

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
