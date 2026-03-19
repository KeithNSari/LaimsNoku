using LAIMS.Models.Investments;

namespace LAIMS.Interfaces.Investments
{
    public interface IPolicyUnitsRepository
    {
        // Create a new PolicyUnits record
        void Create(PolicyUnit policyUnits);

        // Read a PolicyUnits record by ID
        PolicyUnit Read(int id);
        // Read all PolicyUnits records by PolicyID
        List<PolicyUnit> GetByPolicy(Guid PolicyID);

        // Read all PolicyUnits records
        List<PolicyUnit> ReadAll();

        // Update a PolicyUnits record
        void Update(PolicyUnit policyUnits);

        // Delete a PolicyUnits record
        void Delete(int id);
    }
}
