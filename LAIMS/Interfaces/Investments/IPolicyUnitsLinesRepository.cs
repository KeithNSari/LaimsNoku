using LAIMS.Models.Investments;

namespace LAIMS.Interfaces.Investments
{
    public interface IPolicyUnitsLinesRepository
    {
        // Create a new PolicyUnitsLines record
        void Create(PolicyUnitsLines policyUnitsLines);

        // Read a PolicyUnitsLines record by ID
        PolicyUnitsLines Read(int id);
        void DebitPolicyUnits(Guid policyId, Guid UnitTrustID, int UnitPricesListID, decimal TransactionUnits, string AddedBy);
        // Read all PolicyUnitsLines records
        List<PolicyUnitsLines> ReadAll();

        // Update a PolicyUnitsLines record
        void Update(PolicyUnitsLines policyUnitsLines);

        // Delete a PolicyUnitsLines record
        void Delete(int id);
    }
}
