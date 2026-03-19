using LAIMS.Models.Investments;
using System.Data;

namespace LAIMS.Interfaces.Investments
{
    public interface IUnitsPricesListRepository
    {
        // Create a new UnitsPricesList record
        void Create(UnitsPricesList unitsPricesList);

        // Read a UnitsPricesList record by ID
        UnitsPricesList Read(int id);

        // Read all UnitsPricesList records
        List<UnitsPricesList> GetCurrentPrices();
        UnitsPricesList GetLatestPrice(int CurrencyID, Guid UnitTrustID);
        DataTable GetPriceHistory(Guid UnitTrustID);
        DataTable GetLatest();
        int GetPolicyUnitsHeader(Guid PolicyId, Guid TrustID);
        int InsertPolicyUnitsHeader(Guid PolicyId, Guid TrustID, string AddedBy);
        bool Buy(Guid PolicyId, Guid TrustID, int PolicyUnitsID, decimal Units, int PriceID, int TransactionTypeID, string AddedBy);
        int Sell(Guid PolicyId, Guid TrustID, int PolicyUnitsID, decimal Units, int PriceID, int TransactionTypeID, string AddedBy);
        int Sell(Guid PolicyId, Guid TrustID, int ClaimID, int PolicyUnitsID, decimal Units, int PriceID, int TransactionTypeID, string AddedBy);
        int ProposeSell(int ClaimID, Guid PolicyId, Guid TrustID, int PolicyUnitsID, decimal Units, int PriceID, decimal Amount, string AddedBy);
        void UpdateInvestmentContentBalance(Guid PolicyID, decimal InvestmentContent);
        // Update a UnitsPricesList record
        void Update(UnitsPricesList unitsPricesList);

        // Delete a UnitsPricesList record
        void Delete(int id);
    }
}
