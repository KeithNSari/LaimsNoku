using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.NewBusiness.Pages.Membership
{
    public class ClientProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        private readonly IContactTypeRepository _contactTypeRepository;
        private readonly IMemberContactRepository _memberContactRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;
        public DataTable ContactsDT;
        [BindProperty]
        public MemberContact MemberContact { get; set; }
        public Member Member { get; set; }
        public List<ContactType> ContactTypes { get; set; }
        public List<Country> Countries { get; set; }
        public SelectList Cities { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CountryID { get; set; }

        [BindProperty]
        public int CityID { get; set; }

        [BindProperty]
        public string CustomCity { get; set; }
        public ClientProfileModel(UserManager<ApplicationUser> userManager, IMemberRepository memberRepository, 
            IContactTypeRepository contactTypeRepository, IMemberContactRepository memberContactRepository, 
            ICountryRepository countryRepository, ICityRepository cityRepository)
        {
            _userManager = userManager;
            _memberContactRepository = memberContactRepository;
            _contactTypeRepository = contactTypeRepository;
            _memberContactRepository = memberContactRepository;
            _memberRepository = memberRepository;
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
        }
       
        public IActionResult OnGet(Guid id)
        {
            Member = _memberRepository.GetMemberById(id);
            if (Member == null)
            {
                return NotFound();
            }
            ContactTypes = _contactTypeRepository.GetAllContactTypes();
            Countries = _countryRepository.GetAllCountries();
            ContactsDT = _memberContactRepository.Get(id);
            return Page();
        }
        public JsonResult OnGetCitiesByCountry()
        {
            var cities = _cityRepository.GetCitiesByCountry(CountryID);
            return new JsonResult(cities);
        }
        public IActionResult OnPost(Guid id)
        {
            string AddedBy = _userManager.GetUserId(User).ToString();
            MemberContact.AddedBy = AddedBy;
            if (CityID == 0)
            {
                if (string.IsNullOrEmpty(CustomCity))
                {
                    throw new Exception("you must enter or select a city");
                }
                CityID = _countryRepository.AddCity(MemberContact.CountryID, CustomCity, AddedBy, DateTime.Now);
            }
            MemberContact.City = CityID;
            MemberContact.MemberUID = id;
            _memberContactRepository.InsertMemberContact(MemberContact);
            ContactTypes = _contactTypeRepository.GetAllContactTypes();
            Countries = _countryRepository.GetAllCountries();
            ContactsDT = _memberContactRepository.Get(id);
            return Redirect("mcontacts?id=" + id.ToString());
        }
        public IActionResult OnPostArchive(Guid MemberUID, int ContactID)
        {
            string archivedBy = _userManager.GetUserId(User).ToString();
            _memberContactRepository.ArchiveMemberContact(MemberUID, ContactID, archivedBy);
            return Redirect("mcontacts?id=" + MemberUID.ToString());
        }
    }
}
