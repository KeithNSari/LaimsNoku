using LAIMS.Models.Investments;
using System.Data;

namespace LAIMS.Interfaces.Investments
{
    public interface IUnitTrustsLineRepository
    {
        void Add(UnitTrustsLine unitTrustsLine);
        UnitTrustsLine Get(int id);
        List<UnitTrustsLine> GetAll();
        void Update(UnitTrustsLine unitTrustsLine);
        void Delete(int id);
        DataTable GetLatest();
    }
}
