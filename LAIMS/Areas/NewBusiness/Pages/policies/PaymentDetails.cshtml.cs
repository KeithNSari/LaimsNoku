using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Membership;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class PaymentDetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        private readonly IGenderRepository _genderRepository;
        private readonly ITitleRepository _titleRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMaritalStatusRepository _maritalStatusRepository; 
        private readonly ILIRoleRepository _lIRoleRepository;
        private readonly IPolicyTypeRepository _policyTypeRepository; 
        private readonly IPolicyRepository _policyRepository;
        private readonly IPolicyPremiumRepository _policyPremiumRepository;
        public List<SelectListItem> MyProducts = new List<SelectListItem>();
        public List<SelectListItem> MyRelationships = new List<SelectListItem>();
        public List<SelectListItem> LIRoles = new List<SelectListItem>();
        public List<SelectListItem> GenderList = new List<SelectListItem>();
        public List<SelectListItem> TitleList = new List<SelectListItem>();
        public List<SelectListItem> MaritalStatiiSelectList = new List<SelectListItem>();
        public List<SelectListItem> CountrySelectList = new List<SelectListItem>();
        [BindProperty]
        public List<PolicyBeneficiaryLineDetail> PolicyBeneficiaryLineDetails { get; set; }
        public PaymentDetailsModel(UserManager<ApplicationUser> userManager,
            IMemberRepository memberRepository,
            IGenderRepository genderRepository,
            ITitleRepository titleRepository,
            ICountryRepository countryRepository,
            IMaritalStatusRepository maritalStatusRepository, 
            IPolicyTypeRepository policyTypeRepository,
            IPolicyRepository policyRepository, 
            ILIRoleRepository lIRoleRepository, 
            IPolicyPremiumRepository policyPremiumRepository)
        {
            _userManager = userManager;
            _memberRepository = memberRepository;
            _genderRepository = genderRepository;
            _titleRepository = titleRepository;
            _countryRepository = countryRepository;
            _maritalStatusRepository = maritalStatusRepository;
            _policyTypeRepository = policyTypeRepository; 
            _policyRepository = policyRepository; 
            _lIRoleRepository = lIRoleRepository; 
            _policyPremiumRepository = policyPremiumRepository;
        }
        [BindProperty]
        public Guid ProposerID { get; set; }
        [BindProperty]
        public int PremiumPayerID { get; set; }
        [BindProperty]
        public Member PremiumPayer { get; set; }
        [BindProperty]
        public Member NewMember { get; set; } 
        public List<Gender> Genders { get; set; }
        public List<Title> Titles { get; set; }
        public List<Country> Countries { get; set; }
        public List<MaritalStatus> MaritalStatii { get; set; }
        public DataTable MembersDT; 
        [BindProperty]
        public string? PolicyName { get; set; }
        [BindProperty]
        public int IDType { get; set; }
        [BindProperty]
        public string? IDContent { get; set; }
        [BindProperty]
        public string? ConfirmIDContent { get; set; } 
        [BindProperty]
        public int LIRoleID { get; set; } = 0;
        [BindProperty]
        public bool IsPremiumPayer { get; set; } = false; 

        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public IActionResult OnGet(Guid id, Guid policyTypeid, Guid policyid)
        {
            Member Proposer = _memberRepository.GetMemberById(id);
            if (Proposer == null)
            {
                return NotFound();
            }
            ProposerID = id;
            ReturnUrl = Request.Path + Request.QueryString;
            PolicyTypeID = policyTypeid;
            PolicyID = policyid;
            PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
           
            Genders = _genderRepository.GetAllGenders();
            Titles = _titleRepository.GetAllTitles();
            Countries = _countryRepository.GetAllCountries();
            MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
            PremiumPayerID = _policyPremiumRepository.GetPremiumPayer(policyid);
            if (PremiumPayerID > 0)
            {
                PremiumPayer = _memberRepository.GetMemberById(PremiumPayerID);
            }
            
            //LoadProductsSelectList(policyTypeid);
            //LoadRelationshipsSelectList();
            //LoadLIRolesSelectList();
            //LoadGenderSelectList();
            //LoadTitleSelectList();
            //LoadMaritalSelectList();
            //LoadCountrySelectList();
            return Page();
        }
    }
}
