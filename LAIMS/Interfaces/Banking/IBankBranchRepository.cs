using LAIMS.Models.Banking;

namespace LAIMS.Interfaces.Banking
{
    public interface IBankBranchRepository
    {
        void AddBankBranch(BankBranch bankBranch);
        List<BankBranch> GetAllBankBranches();
        void UpdateBankBranch(BankBranch bankBranch);
        void DeleteBankBranch(int memberID);
    }
}
