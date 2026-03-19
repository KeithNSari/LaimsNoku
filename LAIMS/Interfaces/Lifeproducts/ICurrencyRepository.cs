using LAIMS.Models.LifeProducts;
using System.Data;

namespace LAIMS.Interfaces.Lifeproducts
{
    public interface ICurrencyRepository
    {
        int CheckExistence(Currency currency);
        int CheckExistenceOther(Currency currency);
        void InsertCurrency(Currency currency);
        void UpdateCurrency(Currency currency);
        void ArchiveCurrency(Currency currency);
        Currency GetCurrency(int ID);
        List<Currency> GetAllCurrencies();
        DataTable Get();
    }
}
