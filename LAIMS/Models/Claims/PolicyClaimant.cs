using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Claims
{
    public class PolicyClaimant
    {
        public int ClaimID { get; set; }
        public int MainCover { get; set; }
        public int PolicyBeneficiariesLineID { get; set; }
        public int MemberID { get; set; }
        public int BankAccountID { get; set; }
        public int SubmittedBankBranchID { get; set; }
        public int CellPhoneID { get; set; }
        public int TelephoneID {get;set;}
        public int EmailAddressID  {get;set;}
        public int AddressID { get; set; }
        public byte AmountIsPercentage {  get; set; }
        public decimal Amount { get; set; }
        public int PayAfter { get; set; }
        public int RoleID { get; set; }
        public DateTime? AddedOn { get; set; }

        [MaxLength(450)]
        public string AddedBy { get; set; }
    }
}
