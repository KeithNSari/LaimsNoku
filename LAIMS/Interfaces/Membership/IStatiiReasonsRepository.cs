using LAIMS.Models.Membership;

namespace LAIMS.Interfaces.Membership
{
    public interface IStatiiReasonsRepository
    {
        List<StatiiReason> GetAllStatiiReasons(int statusID);
        StatiiReason GetStatiiReasonByID(int reasonID);
        void AddStatiiReason(StatiiReason statiiReason);
        void UpdateStatiiReason(StatiiReason statiiReason);
        void DeleteStatiiReason(int statusID, int reasonID);
    }
}
