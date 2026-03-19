using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Banking
{
    public class MemberBankAccount
    {        
        [Required]
        public int ID { get; set; }
        [Required]
        public int BankID { get; set; }
        [Required]
        public string BranchCode { get; set; }
        [Key]
        [Required]
        [Display(Name = "Member ID")]
        public int MemberID { get; set; }

        [Required]
        [Display(Name = "Bank Account Number")]
        [StringLength(50)]
        public string BankAccountNo { get; set; }
        public string BankAccountNoConfirm { get; set; }
        [Required]
        public string AccountName { get; set; }
        [Required]
        public byte IsInternalAccount { get; set; } = 0;
        [Required]
        public int CurrencyID { get; set; } = 0;

        [Required]
        public bool Current { get; set; }
        [StringLength(450)]
        public string AddedBy { get; set; }

        [Required]
        [Display(Name = "Added On")]
        public DateTime AddedOn { get; set; }

    }
}
