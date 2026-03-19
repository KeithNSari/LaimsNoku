using LAIMS.Models.Claims;

namespace LAIMS.Interfaces.Claims
{
    public interface IPolicyClaimsLineRepository
    {
        PolicyClaimsLine GetLineById(int id);
        List<PolicyClaimsLine> GetLinesByHeaderID(int headerID);
        void AddLine(PolicyClaimsLine line);
        void UpdateLine(PolicyClaimsLine line);
        void DeleteLine(int id);
    }
}
