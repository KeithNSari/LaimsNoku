using LAIMS.Models.Premiums;

namespace LAIMS.Interfaces.Premiums
{
    public interface IPremiumLineRepository
    {
        void CreatePremiumLines(PremiumLine premiumLines);
        PremiumLine ReadPremiumLines(int id);
        void UpdatePremiumLines(PremiumLine premiumLines);
        void DeletePremiumLines(int id);
    }

}
