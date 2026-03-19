using LAIMS.Models.Premiums;
using System.Data;

namespace LAIMS.Interfaces.Premiums
{
    public interface IPremiumCollectionConfigHeaderRepository
    {
        void Create(PremiumCollectionConfigHeader configHeader);
        bool CheckPremiumCollectionConfigHeaderExistence(int PaymentProviderID, string StopOrderName, string StopOrderCode, int CurrencyID);  
        int AddLines(int MemberID, int PaymentMethodID, DateTime CollectionDay, string AddedBy);
        PremiumCollectionConfigHeader Read(int id);
        DataTable Get();
        DataTable GetByPaymentMethod(int PaymentMethodID);
        DataTable GetLatestByPaymentMethod(int PaymentMethodID);
        DataTable GetLines();
        DataTable SearchByPaymentMethod(int PaymentMethodID, string SearchTerm);
        void Update(PremiumCollectionConfigHeader configHeader);
        void Delete(int id);
        void ArchivePremiumCollectionConfigHeader(int ID, string AddedBy, DateTime AddedOn);
    }
}
