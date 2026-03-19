using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.LifeProducts
{
    public class PolicyType
    {
        public int EntryNo { get; set; }
        public Guid ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string? OpenForNewBusinessDesc { get; set; }
        public byte OpenForNewBusiness { get; set; }
        public string? CurrencyName { get; set; }
        [Required(ErrorMessage = "Currency is required.")]
        public int CurrencyID { get; set; }
        [Required(ErrorMessage = "Minimum term is required.")]
        [Range(0, 120, ErrorMessage = "Minimum term should be 0 or greater and less than 120.")]
        public int MinimumTerm { get; set; }
        [Required(ErrorMessage = "Maximum term is required.")]
        [Range(0, 120, ErrorMessage = "Maximum term should be 0 or greater and less than 120.")]
        public int MaximumTerm { get; set; }
        public string? AllowDeferingOfMaturityDateDesc { get; set; }
        public byte AllowDeferingOfMaturityDate { get; set; }
        public int DefermentNoticePeriod { get; set; }
        [Required(ErrorMessage = "Life Assured Min Age is required.")]
        [Range(0, 120, ErrorMessage = "Life Assured Min Age should be 0 or greater and less than 120.")]
        public int LifeAssuredMinAge { get; set; }
        [Required(ErrorMessage = "Life Assured Max Age is required.")]
        [Range(0, 120, ErrorMessage = "Life Assured Max Age  should be 0 or greater and less than 120.")]
        public int LifeAssuredMaxAge { get; set; }
        [Required(ErrorMessage = "Proposer Min Age is required.")]
        [Range(0, 120, ErrorMessage = "Proposer Min Age should be 0 or greater and less than 120.")]
        public int ProposerMinAge { get; set; }
        [Required(ErrorMessage = "Proposer Max Age is required.")]
        [Range(0, 120, ErrorMessage = "Proposer Max Age should be 0 or greater and less than 120.")]
        public int ProposerMaxAge { get; set; }
        [Required(ErrorMessage = "Premium Payer Min Age is required.")]
        [Range(0, 120, ErrorMessage = "Premium Payer Min Age  should be 0 or greater and less than 120.")]
        public int PremiumPayerMinAge { get; set; }
        [Required(ErrorMessage = "Premium Payer Max Age is required.")]
        [Range(0, 120, ErrorMessage = "Premium Payer Max Age should be 0 or greater and less than 120.")]
        public int PremiumPayerMaxAge { get; set; }
        public byte Current { get; set; }
        public DateTime? AddedOn { get; set; }
        public string AddedBy { get; set; }
        public byte? Archived { get; set; }
        public string ArchivedBy { get; set; }
        public string ArchivedComment { get; set; }
        public DateTime? ArchivedOn { get; set; }
        public byte? Deleted { get; set; }
        public string DeletedBy { get; set; }
        public string DeletedComment { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
