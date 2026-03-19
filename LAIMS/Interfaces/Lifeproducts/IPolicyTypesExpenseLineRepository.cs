using LAIMS.Models.LifeProducts;

namespace LAIMS.Interfaces.Lifeproducts
{
    public interface IPolicyTypesExpenseLineRepository
    {
        void AddPolicyTypesExpenseLine(PolicyTypesExpenseLine expenseLine);
        PolicyTypesExpenseLine GetPolicyTypesExpenseLineById(int id);
        void UpdatePolicyTypesExpenseLine(PolicyTypesExpenseLine expenseLine);
        void DeletePolicyTypesExpenseLine(int id);
    }
}
