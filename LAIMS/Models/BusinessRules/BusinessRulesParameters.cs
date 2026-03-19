namespace LAIMS.Models.BusinessRules
{
    public class BusinessRulesParameters
    {
        public int? LifeAssuredMinAge { get; set; }
        public int? LifeAssuredMaxAge { get; set; }
        public int? ProposerMinAge { get; set; }
        public int? ProposerMaxAge { get; set; }
        public int? PremiumPayerMinAge { get; set; }
        public int? PremiumPayerMaxAge { get; set; }
        public int? MinimumTerm { get; set; }
        public int? MaximumTerm { get; set; }
        public byte? Gender { get; set; }
        public Guid? PolicyID { get; set; }
        public Guid? PolicyTypeID { get; set; }
        public Guid? ProposerUID { get; set; }
        public DateTime? ProposerDateOfBirth { get; set; }
        public int? ProposerAgeNextBirthday { get; set; }
        public int? ProposerCurrentAge { get; set; }
        public Guid? ProductID { get; set; }
        public Guid? MemberUID { get; set; }
        public Guid? PremiumPayerUID { get; set; }
        public int? PremiumPayerID { get; set; }
        public DateTime? PremiumPayerDateOfBirth { get; set; }
        public int? PremiumPayerAgeNextBirthday { get; set; }
        public int? PremiumPayerCurrentAge { get; set; }
        public int? PaymentMethodID { get; set; }
        public int? PrincipalMemberID { get; set; }
        public Guid? BeneficiaryUID { get; set; }
        public DateTime? BeneficiaryDateOfBirth { get; set; }
        public int? BeneficiaryAgeNextBirthday { get; set; }
        public int? BeneficiaryCurrentAge { get; set; }
        public int? PolicyBeneficiary { get; set; }
        public byte? Relationship { get; set; }
        public int? ILRoleID { get; set; }
        public decimal? Premium { get; set; }
        public decimal? Cover { get; set; }
        public Guid? DocumentID { get; set; }
        public string DocumentFormat { get; set; }
        public DateTime? DateSigned { get; set; }
        public Guid RequestID { get; set; }
        public byte Status { get; set; }
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }
}
