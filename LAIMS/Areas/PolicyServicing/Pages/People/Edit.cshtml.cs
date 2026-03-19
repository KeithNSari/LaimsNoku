using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace LAIMS.Areas.PolicyServicing.Pages.People
{
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        private readonly IGenderRepository _genderRepository;
        private readonly ITitleRepository _titleRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMaritalStatusRepository _maritalStatusRepository;

        public EditModel(
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


        public void OnGet(Guid id)
        {
            try
            {
                Member = _memberRepository.GetMemberById(id);
                if (Member == null)
                {
                    RedirectToPage("Create"); // Redirect to the index page or handle as needed
                }
                if (!_memberRepository.CheckIDConfirm(id))
                {
                    Redirect("ConfirmIdentity?id=" + id);
                }
                Genders = _genderRepository.GetAllGenders();
                Titles = _titleRepository.GetAllTitles();
                Countries = _countryRepository.GetAllCountries();
                MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
        }

        public IActionResult OnPost(Guid id)
        {
            try
            {
                string AddedBy = _userManager.GetUserId(User).ToString();
                _memberRepository.CopyMember(id, AddedBy);
                Member.UID = id;
                Genders = _genderRepository.GetAllGenders();
                Titles = _titleRepository.GetAllTitles();
                Countries = _countryRepository.GetAllCountries();
                MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
                if (string.IsNullOrEmpty(Member.NationalID) && (string.IsNullOrEmpty(Member.BirthCertificate) && (string.IsNullOrEmpty(Member.Passport))))
                {
                    throw new Exception("At least one form of Identity is required!");
                }
                if (Member.NationalID != null)
                {
                    string pattern1 = @"(^\d{2})-(\d{4,7})\s([A-Za-z]{1}\s(\d{2}$))";
                    string pattern2 = @"(^\d{2})-(\d{4,7})([A-Za-z]{1}(\d{2}$))";
                    Regex regex1 = new Regex(pattern1);
                    Regex regex2 = new Regex(pattern2);
                    if (!(regex1.IsMatch(Member.NationalID) || regex2.IsMatch(Member.NationalID)))
                    {
                        throw new Exception("Invalid ID format");
                    }
                }
                if (Member.NationalID == null) Member.NationalID = string.Empty;
                if (Member.BirthCertificate == null) Member.BirthCertificate = string.Empty;
                if (Member.Passport == null) Member.Passport = string.Empty;
                if (!string.IsNullOrEmpty(Member.NationalID) && _memberRepository.CheckOtherNationalID(id, Member.NationalID))
                {
                    throw new Exception("A different member with the provided National ID already exists.");
                }
                if (!string.IsNullOrEmpty(Member.BirthCertificate) && _memberRepository.CheckOtherBirthCertificate(id, Member.BirthCertificate))
                {
                    throw new Exception("A different member with the provided Birth Certificate already exists.");
                }
                if (!string.IsNullOrEmpty(Member.Passport) && _memberRepository.CheckOtherPassport(id, Member.Passport))
                {
                    throw new Exception("A different member with the passport already exists.");
                }
                if (Member.DOB > DateTime.Today)
                {
                    throw new Exception("Date of birth cannot be in the future!");
                } 
                _memberRepository.UpdateMemberStaging(Member);
                return Redirect("Details?id=" + id.ToString());
            }
            catch (Exception ex)
            {               
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
            //finally
            //{
            //    Genders = _genderRepository.GetAllGenders();
            //    Titles = _titleRepository.GetAllTitles();
            //    Countries = _countryRepository.GetAllCountries();
            //    MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
            //}
            return Page();
        }
    }
}
