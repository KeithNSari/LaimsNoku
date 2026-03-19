using LAIMS.Models.Banking;

namespace LAIMS.Interfaces.Banking
{
    public interface IBankRepository
    {
        int AddBank(Bank bank);
		List<Bank> GetAllBanks();
        int GetBankID(int PaymentProviderID);
        BankAccountFormat GetBankAccountFormat(int BankID);
        BankAccountFormat GetBankAccountFormatByPaymentProvider(int PaymentProviderID);
		void UpdateBank(Bank bank);
        void DeleteBank(int memberID);
    }
}
