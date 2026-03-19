using LAIMS.Models.LifeProducts;

namespace LAIMS.Interfaces.Lifeproducts
{
    public interface IExpenseTypeRepository
    {
        List<ExpenseType> GetAllExpenseTypes();
        ExpenseType GetExpenseTypeById(int id);
        void AddExpenseType(ExpenseType expenseType);
        void UpdateExpenseType(ExpenseType expenseType);
        void DeleteExpenseType(int id);
    }
}
