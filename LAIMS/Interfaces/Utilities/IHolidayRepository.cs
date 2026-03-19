using LAIMS.Models.Utilities;
namespace LAIMS.Interfaces.Utilities
{
    public interface IHolidayRepository
    {
        void Create(Holiday holiday);
        Holiday Read(int id);
        List<Holiday> GetAllHolidays(int year);
        void Update(Holiday holiday);
        void Delete(int id);
    }
}
