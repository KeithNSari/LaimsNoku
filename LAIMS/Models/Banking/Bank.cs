using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Banking
{
    public class Bank
    {
        
        [Required]
        [Display(Name = "Entry Number")]
        public int EntryNo { get; set; }
        [Key]
        [Required]
        [Display(Name = "Bank ID")]
        public int BankID { get; set; }
        [Required]
        public string BankName { get; set; }
        [Required]
        [StringLength(50)]
        public string Code { get; set; }
        public string BankAccountNoFormat { get; set; }
        public string FormatDescription { get; set; }
        [Required]
        [Display(Name = "Added By")]
        [StringLength(450)]
        public string AddedBy { get; set; }

        [Required]
        [Display(Name = "Added On")]
        public DateTime AddedOn { get; set; }

        [Required]
        public byte Archived { get; set; }

        [StringLength(450)]
        [Display(Name = "Archived By")]
        public string ArchivedBy { get; set; }

        [Display(Name = "Archived On")]
        public DateTime? ArchivedOn { get; set; }
    }
}
