using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using LAIMS.Models.Security;
using LAIMS.Repositories.Membership;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.NewBusiness.Pages.Membership
{
    public class ContactDetailsModel : PageModel
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IContactTypeRepository _contactTypeRepository;
        private readonly IMemberContactRepository _memberContactRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;
        public DataTable ContactsDT;
        [BindProperty]
        public MemberContact MemberContact { get; set; }
                public Member Member { get; set; }
        public SelectList ContactTypes { get; set; }
        public List<Country> Countries { get; set; }
        public SelectList Cities { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CountryID { get; set; }

        [BindProperty]
        public int CityID { get; set; }

        [BindProperty]
        public string CustomCity { get; set; }
        public ContactDetailsModel(UserManager<ApplicationUser> userManager, IMemberRepository memberRepository,IContactTypeRepository contactTypeRepository, IMemberContactRepository memberContactRepository, ICountryRepository countryRepository, ICityRepository cityRepository)
        {
            _userManager = userManager;
            _memberContactRepository = memberContactRepository;
            _contactTypeRepository = contactTypeRepository;
            _memberContactRepository = memberContactRepository; 
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
        }
        private readonly UserManager<ApplicationUser> _userManager;
        public IActionResult OnGet(Guid id)
        {
            Member = _memberRepository.GetMemberById(id);
            if (Member == null)
            {
                return NotFound();
            }
            ContactTypes = new SelectList(_contactTypeRepository.GetAllContactTypes(), "ID", "Type");
            Countries = _countryRepository.GetAllCountries();
            ContactsDT = _memberContactRepository.Get(id);
            return Page();
        }
        public JsonResult OnGetCitiesByCountry()
        {
            var cities = _cityRepository.GetCitiesByCountry(CountryID);
            return new JsonResult(cities);
        }
    }
}
