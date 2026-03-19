using LAIMS.Models.Banking;
using System.Data;

namespace LAIMS.Interfaces.Banking
{
    public interface IMemberBankAccountRepository
    {
        int AddMemberBankAccount(MemberBankAccount memberBankAccount);
        List<MemberBankAccount> GetAllMemberBankAccounts();
        DataTable GetInternalAccounts();
        void UpdateMemberBankAccount(MemberBankAccount memberBankAccount);
        void ArchiveMemberAccount(int id, string ArchivedBy, DateTime ArchivedOn);
        void DeleteMemberBankAccount(int id);
    }
}
