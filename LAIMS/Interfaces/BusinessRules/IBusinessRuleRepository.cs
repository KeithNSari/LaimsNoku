using LAIMS.Models.BusinessRules;
using System.Data;

namespace LAIMS.Interfaces.BusinessRules
{
    public interface IBusinessRuleRepository
    {
        int CheckExistence(BusinessRule businessRule);
        void CreateBusinessRule(BusinessRule businessRule);
        void AddPolicyStatiiOverride(PolicyStatiiOverride policyOverride);
        BusinessRule GetBusinessRuleById(int entryNo);
        List<string> GetStoredProcedures();
        List<BusinessRule> GetAllBusinessRules();
        DataTable Get();
        void UpdateBusinessRule(BusinessRule businessRule);
        void ArchiveBusinessRule(Guid ID, string AddedBy, DateTime AddedOn);
        void DeleteBusinessRule(int entryNo);
        //Check Rules Section
        StatusReport CheckRules(Guid ObjectID, string ValidationGroup, BusinessRulesParameters newBusinessParameters);
        List<StatusReport> GetAllChecks(Guid ObjectID, string ValidationGroup, BusinessRulesParameters newBusinessParameters);

    }
}
