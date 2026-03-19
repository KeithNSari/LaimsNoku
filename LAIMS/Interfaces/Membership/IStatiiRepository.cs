using LAIMS.Models.Membership;

namespace LAIMS.Interfaces.Membership
{
    public interface IStatiiRepository
    {
        List<Statii> GetAllMemberStatii();
        List<Statii> GetAllSelectablePolicyStatii();
        Statii GetStatiiByID(int statiiID);
        void AddStatii(Statii statii);
        void UpdateStatii(Statii statii);
        void DeleteStatii(int statiiID);
    }
}
