using LAIMS.Models.Membership;
using System.Data;

namespace LAIMS.Interfaces.Membership
{
    public interface IMemberContactRepository
    {
        bool CheckMemberContactExistence(MemberContact memberContact);
        void InsertMemberContact(MemberContact memberContact);
        DataTable Get(Guid UID);
        List<MemberContact> GetAllMemberContacts(Guid MemberUID);
        MemberContact GetMemberContact(Guid MemberUID, int ContactID);
        void UpdateMemberContact(MemberContact memberContact);
        void ArchiveMemberContact(Guid MemberUID, int ContactID, string AddedBy);
        void DeleteMemberContact(int memberContactId);
    }
}
