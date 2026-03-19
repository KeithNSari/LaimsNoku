using LAIMS.Models.LifeProducts;
using System.Data;

namespace LAIMS.Interfaces.Lifeproducts
{
    public interface IPolicyTypeRepository
    {
        int CheckExistence(PolicyType policyType);
        int CheckExistenceOther(PolicyType policyType);
        void InsertPolicyType(PolicyType policyType);
        void UpdatePolicyType(PolicyType policyType);
        void DeletePolicyType(int entryNo);
        PolicyType GetPolicyType(int entryNo);
        PolicyType GetPolicyType(Guid ID);
        PolicyType GetPolicyTypeFull(Guid ID);
        List<PolicyType> GetAllPolicyTypes();
        List<PolicyType> GetAll();
		DataTable Get();
        DataTable Get(Guid ID);
        DataTable GetByDesignation(string UserID);
        string? GetPolicyTypeName(Guid ID);
        bool HasInvestmentProduct(Guid ID);
        bool IsPureInvestmentProduct(Guid ID);
        bool HasRiskProduct(Guid ID);
        bool AdditionalLifeAssuredAllowedCheck(Guid PolicyTypeID);
        bool CheckLIRole(Guid ID, int LIRoleID);
        bool IsLife(Guid ID);
        int GetMaximumTerm(Guid ID);
        int GetMinimumTerm(Guid ID);
        Guid GetID(string PolicyTypeName);
        DataTable GetDesignationPolicyTypes(string UserID);
        bool CheckDesignationPermission(string UserID, Guid PolicyTypeID);
    }
}
