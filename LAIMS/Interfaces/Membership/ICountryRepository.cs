using LAIMS.Models.Membership; 

namespace LAIMS.Interfaces.Membership
{
    public interface ICountryRepository
    {
        List<Country> GetAllCountries();
        Country GetCountryById(int countryId);
        Country GetCountryName(string countryName);
        void AddCountry(Country country);
        int AddCity(int CountryID, string CityName, string AddedBy, DateTime AddedOn);
    }
}
