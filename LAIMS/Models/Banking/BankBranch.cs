using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Banking
{
    public class BankBranch
    {       
        [Required]
        [Display(Name = "Entry Number")]
        public int EntryNo { get; set; }

        [Key]
        [Required]
        [Display(Name = "Bank ID")]
        public int BankID { get; set; }
        public string BranchName { get; set; }

        [Required]
        [Display(Name = "Member ID")]
        public int MemberID { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

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
