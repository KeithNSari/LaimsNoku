using LAIMS.Models.Claims;
using System.Data;

namespace LAIMS.Interfaces.Claims
{
    public interface IPremiumWaiverRepository
    {
        DataTable GetClaimTypeLines(Guid productId);
        DataTable GetClaimTypes();
        void AddClaimTypeLine(ClaimTypeLine claimTypeLine);
        void ArchiveClaimTypeLine(int id, string archivedBy);

        DataTable GetDisabilityPremiumWaivers();
        void AddDisabilityPremiumWaiver(DisabilityPremiumWaiver waiver);

        DataTable GetDeathPremiumWaivers();
        void AddDeathPremiumWaiver(DeathPremiumWaiver waiver);
    }
}
