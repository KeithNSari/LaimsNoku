using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.Membership;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using System.Data;

namespace LAIMS.Areas.NewBusiness.Pages.Membership
{
    public class EditContactsModel : PageModel
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IContactTypeRepository _contactTypeRepository;
        private readonly IMemberContactRepository _memberContactRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IPolicyRepository _policyRepository; 
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
        public EditContactsModel(UserManager<ApplicationUser> userManager, IMemberRepository memberRepository, 
            IContactTypeRepository contactTypeRepository, IMemberContactRepository memberContactRepository, 
            ICountryRepository countryRepository, ICityRepository cityRepository, IPolicyRepository policyRepository)
        {
            _userManager = userManager;
            _memberContactRepository = memberContactRepository;
            _contactTypeRepository = contactTypeRepository;
            _memberContactRepository = memberContactRepository;
            _memberRepository = memberRepository;
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
            _policyRepository = policyRepository;
        }
        private readonly UserManager<ApplicationUser> _userManager;
        public IActionResult OnGet(Guid MemberUID, int ContactID)
        {
            Member = _memberRepository.GetMemberById(MemberUID);
            if (Member == null)
            {
                return NotFound();
            }
            LoadPageComponents(MemberUID, ContactID);
            return Page();
        }
        public void LoadPageComponents(Guid MemberUID, int ContactID)
        {
            ContactTypes = _contactTypeRepository.GetAllContactTypes();
            Countries = _countryRepository.GetAllCountries();
            MemberContact = _memberContactRepository.GetMemberContact(MemberUID, ContactID);
            ContactTypes = _contactTypeRepository.GetAllContactTypes();
            Countries = _countryRepository.GetAllCountries(); 
        }
        public JsonResult OnGetCitiesByCountry()
        {
            var cities = _cityRepository.GetCitiesByCountry(CountryID);
            return new JsonResult(cities);
        }
        public IActionResult OnPost(Guid MemberUID, int ContactID)
        {
            try
            {
                string AddedBy = _userManager.GetUserId(User).ToString();
                MemberContact.AddedBy = AddedBy;
                if (CityID == 0)
                {
                    if (!string.IsNullOrEmpty(CustomCity))
                    {
                        if (MemberContact.CountryID == 0)
                        {
                            throw new Exception("Country must be selected to add a new city!");
                        }
                        CityID = _countryRepository.AddCity(MemberContact.CountryID, CustomCity, AddedBy, DateTime.Now);
                    }
                    CityID = -1;
                }
                MemberContact.City = CityID;
                MemberContact.MemberUID = MemberUID;
                MemberContact.ID = ContactID;
                _memberContactRepository.UpdateMemberContact(MemberContact);
                _policyRepository.PolicyServicingMessagesAdd(Guid.Empty, MemberUID, 2, "Please note your contact details have been edited. If this change is unexpected, please contact us!", AddedBy);
                return Redirect("MemberProfile?id=" + MemberUID);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
                Member = _memberRepository.GetMemberById(MemberUID);
                if (Member == null)
                {
                    return NotFound();
                }
                LoadPageComponents(MemberUID, ContactID);
                return Page();
            }            
        }
    }
}
