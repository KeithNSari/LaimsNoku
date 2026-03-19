using LAIMS.Models.Premiums;
using System.Data;

namespace LAIMS.Interfaces.Premiums
{
    public interface IBillingMessageRepository
    {
        List<BillingMessage> GetAllBillingMessages();
        BillingMessage GetBillingMessageById(int id);
        DataTable GetLatestBillingMessages();
        DataTable GetBillingMessages(long BatchID);
        DataTable GetBillingMessages(Guid PolicyID);
        void AddBillingMessage(BillingMessage billingMessage);
        void UpdateBillingMessage(BillingMessage billingMessage);
        void DeleteBillingMessage(int id);
    }
}
