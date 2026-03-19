using LAIMS.Models.Membership;

namespace LAIMS.Interfaces.Membership
{
    public interface ICityRepository
    {
        void InsertCity(City city);
        public List<City> GetCitiesByCountry(int CountryID);
        List<City> GetAllCities();
        void UpdateCity(City city);
        void DeleteCity(int cityId);
    }
}
