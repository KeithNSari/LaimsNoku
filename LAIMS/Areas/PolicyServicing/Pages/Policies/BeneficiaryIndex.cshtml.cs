using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Lifeproducts;
using LAIMS.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Text.RegularExpressions;
namespace LAIMS.Areas.PolicyServicing.Pages.Policies
{
    public class BeneficiaryIndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyRepository _policyRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IPolicyBeneficiaryRepository _policyBeneficiaryRepository;
        private readonly IGenderRepository _genderRepository;
        private readonly ITitleRepository _titleRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMaritalStatusRepository _maritalStatusRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        public List<SelectListItem> GenderList = new List<SelectListItem>();
        public List<SelectListItem> TitleList = new List<SelectListItem>();
        public List<SelectListItem> MaritalStatiiSelectList = new List<SelectListItem>();
        public List<SelectListItem> CountrySelectList = new List<SelectListItem>();
        public List<SelectListItem> MyRelationships = new List<SelectListItem>();
        public BeneficiaryIndexModel(UserManager<ApplicationUser> userManager,
            IPolicyTypeRepository policyTypeRepository,
            IMemberRepository memberRepository, IPolicyRepository policyRepository,
            IPolicyBeneficiaryRepository policyBeneficiaryRepository,
            IGenderRepository genderRepository,
            IRelationshipRepository relationshipRepository,
            ITitleRepository titleRepository,
            ICountryRepository countryRepository,
            IMaritalStatusRepository maritalStatusRepository)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _memberRepository = memberRepository;
            _genderRepository = genderRepository;
            _titleRepository = titleRepository;
            _countryRepository = countryRepository;
            _maritalStatusRepository = maritalStatusRepository;
            _relationshipRepository = relationshipRepository;
            _policyBeneficiaryRepository = policyBeneficiaryRepository;
            _policyTypeRepository = policyTypeRepository;
        }
        [BindProperty]
        public Policy Policy { get; set; }
        public Member Member { get; set; }
        public DataTable BeneficiariesDT;
        public List<Gender> Genders { get; set; }
        public List<Title> Titles { get; set; }
        public List<Country> Countries { get; set; }
        public List<MaritalStatus> MaritalStatii { get; set; }
        [BindProperty]
        public int IDType { get; set; }
        [BindProperty]
        public string? IDContent { get; set; }
        [BindProperty]
        public string? ConfirmIDContent { get; set; }

        [BindProperty]
        public int RelationshipID { get; set; } = -1;
        [BindProperty]
        public Guid MemberID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public Member NewMember { get; set; }
        [BindProperty]
        public Guid RequestID { get; set; }
        [BindProperty]
        public DataTable BeneficiarySharesDT { get; set; }

        [BindProperty]
        public DataTable OriginalBeneficiarySharesDT { get; set; }
        public DataTable OriginalBeneficiariesDT;
        [BindProperty]
        public bool HasInvestmentContent { get; set; }

        public IActionResult OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                RequestID = reqid;
                PolicyID = policyid;
                MemberID = id;
                PolicyTypeID = policyTypeid;
                Policy = _policyRepository.GetPolicyById(policyid);
                Member = _memberRepository.GetMemberById(id);
                BeneficiariesDT = _policyBeneficiaryRepository.GetStagingBeneficiaryList(policyid, RequestID);
                OriginalBeneficiariesDT= _policyBeneficiaryRepository.GetBeneficiaryList(policyid); 
                Genders = _genderRepository.GetAllGenders();
                Titles = _titleRepository.GetAllTitles();
                Countries = _countryRepository.GetAllCountries();
                MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
                HasInvestmentContent = _policyTypeRepository.HasInvestmentProduct(PolicyTypeID);
                if (HasInvestmentContent)
                {
                    BeneficiarySharesDT = _policyBeneficiaryRepository.GetStagingBeneficiaryShares(policyid, reqid);
                    OriginalBeneficiarySharesDT = _policyBeneficiaryRepository.GetBeneficiaryShares(policyid);
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
            return Page();
        }
    }
}
