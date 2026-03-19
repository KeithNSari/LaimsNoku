using LAIMS.Models.Premiums;

namespace LAIMS.Interfaces.Premiums
{
    public interface IPremiumHeaderRepository
    {
        void CreatePremiumHeader(PremiumHeader premiumHeader);
        PremiumHeader ReadPremiumHeader(int id);
        void UpdatePremiumHeader(PremiumHeader premiumHeader);
        void DeletePremiumHeader(int id);
    }
}
