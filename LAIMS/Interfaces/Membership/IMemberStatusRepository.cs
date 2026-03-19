using LAIMS.Models.Membership;
using System.Data;

namespace LAIMS.Interfaces.Membership
{
    public interface IMemberStatusRepository
    {
        List<MemberStatus> GetAllMemberStatus();
        DataTable GetMemberStatusHistory(Guid MemberUID);
        void AddMemberStatus(MemberStatus memberStatus);
        void UpdateMemberStatus(MemberStatus memberStatus);
        void DeleteMemberStatus(int id);
    }
}
