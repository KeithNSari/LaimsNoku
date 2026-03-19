using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.NewBusiness.Pages.Membership
{
    public class CountryCitiesModel : PageModel
    {
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;

        [BindProperty(SupportsGet = true)] 
        public int SelectedCountry { get; set; }

        [BindProperty]
        public int SelectedCity { get; set; }

        [BindProperty]
        public string CustomCity { get; set; }

        public List<Country> Countries { get; set; }
        public List<City> Cities { get; set; }

        public CountryCitiesModel(ICountryRepository countryRepository, ICityRepository cityRepository)
        {
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
        }

        public void OnGet()
        {
            Countries = _countryRepository.GetAllCountries();
            Cities = new List<City>();
        }

        public JsonResult OnGetCitiesByCountry(int countryId)
        {
            var cities = _cityRepository.GetCitiesByCountry(countryId);
            return new JsonResult(cities);
        }

        public void OnPost()
        {
            // Handle form submission
        }
    }
}
