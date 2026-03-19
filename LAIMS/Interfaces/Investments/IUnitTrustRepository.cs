using LAIMS.Models.Investments;
using System.Data;

namespace LAIMS.Interfaces.Investments
{
    public interface IUnitTrustRepository
    {
        // Create a new UnitTrust record
        void Create(UnitTrust unitTrust);

        // Read a UnitTrust record by ID
        UnitTrust Read(Guid id);

        // Read all UnitTrust records
        List<UnitTrust> ReadAll();
        DataTable GetTotals(string PolicyNo);
        DataTable GetTotals(Guid PolicyID);
        DataTable GetLatestTransactions(Guid PolicyId);
		DataTable GetLatestPurchaseTransactions(Guid PolicyId);
        DataTable GetLatestPurchaseTransactions(string PolicyNo);
        DataTable GetLatestSalesTransactions(string PolicyNo);
        DataTable GetLatestSalesTransactions(Guid PolicyId);
        DataTable GetSalesDetails(string PolicyNo);
        DataTable GetSalesDetails(int ClaimID);
        decimal GetTotalAvailableunits(Guid PolicyId, Guid TrustID);
        // Update a UnitTrust record
        void Update(UnitTrust unitTrust);

        // Delete a UnitTrust record
        void Delete(Guid id);
    }
}
