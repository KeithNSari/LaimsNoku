using LAIMS.Models.Premiums;
using System.Data;

namespace LAIMS.Interfaces.Premiums
{
    public interface IPolicyTypesExpenseRepository
    {
        List<PolicyTypesExpense> GetAll();
        DataTable Get(Guid PolicyTypeID);
        PolicyTypesExpense GetById(int id);
        void Add(PolicyTypesExpense policyTypesExpense);
        void Update(PolicyTypesExpense policyTypesExpense);
        void Archive(int id, Guid PolicyTypeID, string AddedBy);
    }
}
