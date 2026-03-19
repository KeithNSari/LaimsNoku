using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Text.RegularExpressions;

namespace LAIMS.Areas.NewBusiness.Pages.Membership
{
    [Authorize(Roles = "New Business Initiator")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        private readonly IGenderRepository _genderRepository;
        private readonly ITitleRepository _titleRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMaritalStatusRepository _maritalStatusRepository;

        public CreateModel(
            UserManager<ApplicationUser> userManager,
            IMemberRepository memberRepository,
            IGenderRepository genderRepository,
            ITitleRepository titleRepository,
            ICountryRepository countryRepository,
            IMaritalStatusRepository maritalStatusRepository)
        {
            _userManager = userManager;
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            _genderRepository = genderRepository ?? throw new ArgumentNullException(nameof(genderRepository));
            _titleRepository = titleRepository ?? throw new ArgumentNullException(nameof(titleRepository));
            _countryRepository = countryRepository ?? throw new ArgumentNullException(nameof(countryRepository));
            _maritalStatusRepository = maritalStatusRepository ?? throw new ArgumentNullException(nameof(maritalStatusRepository));
        }

        [BindProperty]
        public Member Member { get; set; }

        public List<Gender> Genders { get; set; }
        public List<Title> Titles { get; set; }
        public List<Country> Countries { get; set; }
        public List<MaritalStatus> MaritalStatii { get; set; } 
        
        public DataTable MembersDT;
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        public string SearchPageUrl = "SearchMembers";
        public IActionResult OnGet()
        {
            try
            {
				Genders = _genderRepository.GetAllGenders();
				Titles = _titleRepository.GetAllTitles();
				Countries = _countryRepository.GetAllCountries();
				MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
				string AddedBy = _userManager.GetUserId(User);
				MembersDT = _memberRepository.GetMyEntries(AddedBy);
                return Page();
			}
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
			}
        }

        public IActionResult OnPost()
        {
            ReturnUrl = Request.Path + Request.QueryString;
            try
            {               
                if (string.IsNullOrEmpty(Member.NationalID) && (string.IsNullOrEmpty(Member.BirthCertificate) && (string.IsNullOrEmpty(Member.Passport))))
                {
                    throw new Exception("At least one form of Identity is required!");
                }
                if(Member.NationalID != null)
                {
                    string pattern1 = @"(^\d{2})-(\d{4,7})\s([A-Za-z]{1}\s(\d{2}$))";
                    string pattern2 = @"(^\d{2})-(\d{4,7})([A-Za-z]{1}(\d{2}$))";
                    Regex regex1 = new Regex(pattern1);
                    Regex regex2 = new Regex(pattern2); 
                    if (!(regex1.IsMatch(Member.NationalID)|| regex2.IsMatch(Member.NationalID)))
                    {
                        throw new Exception("Invalid ID format"); 
                    }
                }
                if (Member.NationalID == null) Member.NationalID = string.Empty;
                if (Member.BirthCertificate == null) Member.BirthCertificate = string.Empty;
                if (Member.Passport == null) Member.Passport = string.Empty;
                if ((Member.NationalID!=string.Empty) &&  (_memberRepository.CheckNationalIDExistence(Member.NationalID)))
                {
                    throw new Exception("A member with this National ID already exists. Perform a search using the National ID to cross check if this is the same member!");
                }
                if ((Member.BirthCertificate != string.Empty) && (_memberRepository.CheckBirthCertificateExistence(Member.BirthCertificate)))
                {
                    throw new Exception("A member with this birth certificate already exists. Perform a search using the birth certificate to cross check if this is the same member!");
                }
                if ((Member.Passport != string.Empty) && (_memberRepository.CheckPassportExistence(Member.Passport)))
                {
                    throw new Exception("A member with this passport already exists. Perform a search using the passport number to cross check if this is the same member!");
                }
                if (Member.DOB > DateTime.Today)
                {
                    throw new Exception("Date of birth cannot be in the future!");
                }
                Member.IsOrganisation = 0;
                Member.UID = Guid.NewGuid();
                string AddedBy = _userManager.GetUserId(User).ToString();
                Member.AddedBy = AddedBy;
                Member.AddedOn = DateTime.Now;
                _memberRepository.AddMember(Member);
                return Redirect("ConfirmIdentity?id=" + Member.UID.ToString());
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostProfile(Guid id)
        {
            if(_memberRepository.CheckIDConfirm(id))
            {
                return Redirect("MemberProfile?id=" + id);
            }
            return Redirect("ConfirmIdentity?id=" + id);
        }
    }
}
