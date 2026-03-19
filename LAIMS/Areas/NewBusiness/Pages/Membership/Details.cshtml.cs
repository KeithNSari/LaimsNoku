using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LAIMS.Areas.NewBusiness.Pages.Membership
{
    public class DetailsModel : PageModel
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IMemberContactRepository _memberContactRepository;
        private readonly IContactTypeRepository _contactTypeRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;


        [BindProperty]
        public MemberContact MemberContact { get; set; }

        public SelectList ContactTypes { get; set; }
        public List<Country> Countries { get; set; }
        public SelectList Cities { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CountryID { get; set; }

        [BindProperty]
        public int CityID { get; set; }

        [BindProperty]
        public string CustomCity { get; set; }

        public DetailsModel(IMemberRepository memberRepository, IMemberContactRepository memberContactRepository, IContactTypeRepository contactTypeRepository, ICountryRepository countryRepository, ICityRepository cityRepository)
        {
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            _memberContactRepository = memberContactRepository;
            _contactTypeRepository = contactTypeRepository;
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
        }

        public Member Member { get; set; }

        public IActionResult OnGet(Guid id)
        {
            Member = _memberRepository.GetMemberById(id);
            if (Member == null)
            {
                return NotFound();
            }
            ContactTypes = new SelectList(_contactTypeRepository.GetAllContactTypes(), "ID", "Type");
            Countries = _countryRepository.GetAllCountries();
            return Page();
        }
        public IActionResult OnPostSaveContact()
        {
            if (!ModelState.IsValid)
            {
                ContactTypes = new SelectList(_contactTypeRepository.GetAllContactTypes(), "ID", "Type");
                //Countries = new SelectList(_countryRepository.GetAllCountries(), "CountryID", "CountryName");
                //Cities = new SelectList(new List<City>(), "ID", "CityName");
                return Page();
            }
            _memberContactRepository.InsertMemberContact(MemberContact);
            return RedirectToPage("./Index"); // Redirect to the page where you list all MemberContacts
        }
        public JsonResult OnGetCitiesByCountry()
        {
            var cities = _cityRepository.GetCitiesByCountry(CountryID);
            return new JsonResult(cities);
        }
    }
}
