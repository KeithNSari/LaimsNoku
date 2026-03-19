namespace LAIMS.Models.Policies
{
    public class ProposerDetails
    {
        public bool IsPremiumPayer { get; set; } = false;
        public bool IsBeneficiary { get; set; } = false;
        public bool IsMainLifeAssured { get; set; } = false;
        public bool Smoker { get; set; } = false;
        public bool Tested { get; set; } = false;
    }
}
