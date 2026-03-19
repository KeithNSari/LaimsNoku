using LAIMS.Models.Membership;
using System.Data;

namespace LAIMS.Interfaces.Membership
{
    public interface IMemberRepository
    {
        List<Member> GetAllMembers();
        Member GetMemberById(Guid memberId);
        Member GetStagingMemberById(Guid memberId);
        Member GetMemberById(string ID);
        Member GetMemberById(int ID);
        Guid GetOrganisationIDByName(string OrganisationName);
        DataTable GetOrganisation(Guid UID);
        DataTable GetLatestOrganisations();
        DataTable SearchOrganisations2(string SearchTerm);
        DataTable GetMyEntries(string AddedBy);
        int GetID(Guid UID);
        Guid GetUID(int ID);
        DataTable Search(string SearchTerm);
        List<Member> SearchOrganisations(string SearchTerm);
        void AddMember(Member member);
        bool ConfirmID(Guid ID, string NationalID, string PassPort, string BirthCertificate);
        bool CheckIDConfirm(Guid ID);
        bool CheckOther(Guid ID, string NationalID, string PassPort, string BirthCertificate);
        bool CheckNationalIDExistence(string NationalID);
        bool CheckPassportExistence(string Passport);
        bool CheckBirthCertificateExistence(string BirthCertificate);
        bool CheckOtherNationalID(Guid ID, string NationalID);
        bool CheckOtherBirthCertificate(Guid ID, string BirthCertificate);
        bool CheckOtherPassport(Guid ID, string Passport); 
        int CheckMemberNameExistence(string MemberName);
        bool SetConfirmation(Guid ID, int Status);
        void UpdateMember(Member member);
        int GetGender(int MemberID);
        void DeleteMember(int memberId);
        bool CopyMember(Guid UID, string AddedBy);
        void UpdateMemberStaging(Member member);
        bool UpdateMemberFromCopy(Guid UID, Guid RequestID, string AddedBy);
        bool UpdateCopyStatus(Guid UID, Guid RequestID, int StatusID, string StatusComment, string AddedBy);
        DataTable GetStaging();
        DataTable SearchStaging(string SearchTerm);
    }
}
