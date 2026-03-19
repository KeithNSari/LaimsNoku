using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Repositories.Lifeproducts;
using LAIMS.Repositories.Membership;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Transactions;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class EmploymentDetailsModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        private readonly IEmploymentRepository _employmentRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        private readonly IPolicyRepository _policyRepository;
        public List<SelectListItem> CategoriesList = new List<SelectListItem>();
        public List<SelectListItem> Currencies = new List<SelectListItem>();
        public string SearchPageUrl;	
		
		[BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }
		[BindProperty]
        public Guid ID { get; set; } //id of applicant
		[BindProperty]
		public Guid PolicyID { get; set; }
        [BindProperty]
		public Guid PolicyTypeID { get; set; }
		[BindProperty]
		public bool OrganisationFound { get; private set; }
        public bool ShowAddSection => !OrganisationFound && !string.IsNullOrEmpty(SearchTerm);
        public List<SelectListItem> OrganisationsList = new List<SelectListItem>();  
        [BindProperty]
        public Employment Employment { get; set; } 
        public EmploymentDetailsModel(UserManager<IdentityUser> userManager, 
            IMemberRepository memberRepository,
            IEmploymentRepository employmentRepository, 
            ICurrencyRepository currencyRepository,
            IBusinessRuleRepository businessRuleRepository,
            IPolicyRepository policyRepository)
        {
            _userManager = userManager;
            _memberRepository = memberRepository;
            _employmentRepository = employmentRepository;
            _currencyRepository = currencyRepository;
            _businessRuleRepository = businessRuleRepository;
            _policyRepository = policyRepository;
        } 
        [BindProperty]
        public int ResultsCount { get; set; } = 0; 
		public void OnGet(Guid id, Guid policyTypeid, Guid policyid)
		{ 
			ID = id;
			PolicyTypeID = policyTypeid;
			PolicyID = policyid; 
		}
		public void OnGetSearch(string? search, Guid id, Guid policyTypeid, Guid policyid)
        {
            SearchTerm = search;
            SearchPageUrl = "EmploymentDetails";
            ID = id;
            PolicyTypeID = policyTypeid;
            PolicyID=policyid;
            _policyRepository.UpdatePolicyStage(PolicyID, 7);
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                LoadCategoriesSelectList();
                LoadCurrencies();
                List<Member> OrgList = _employmentRepository.SearchOrganisations(SearchTerm);
                ResultsCount = OrgList.Count;
                if ( ResultsCount> 0)
                {
                    // Employer found
                    OrganisationFound = true;
                    LoadOrganisationsSelectList(OrgList);                   
                }
            }
        }
        private void LoadOrganisationsSelectList(List<Member> OrgList)
        {
            foreach (Member organisation in OrgList)
            {
                OrganisationsList.Add(new SelectListItem
                {
                    Value = organisation.UID.ToString(),
                    Text = organisation.Name1
                });
            }
        }
        private void LoadCategoriesSelectList()
        {
            foreach (EmploymentCategory employmentCategory in _employmentRepository.GetEmploymentCategories())
            {
                CategoriesList.Add(new SelectListItem
                {
                    Value = employmentCategory.ID.ToString(),
                    Text = employmentCategory.Category
                });
            }
        }
        private void LoadCurrencies()
        {
            foreach (Currency currency in _currencyRepository.GetAllCurrencies())
            {
                Currencies.Add(new SelectListItem
                {
                    Value = currency.ID.ToString(),
                    Text = currency.CurrencyName
                });
            }
        }
        public IActionResult OnPost()
        {
            using(TransactionScope TS = new TransactionScope())
            {
                if (Employment.EmployerID == Guid.Empty)
                {
                   Guid employerID = _memberRepository.GetOrganisationIDByName(Employment.Employer);
                    if (employerID==Guid.Empty)
                    {
                        Member NewEmployer = new()
                        {
                            Name1 = Employment.Employer,
                            UID = Guid.NewGuid(),
                            IsOrganisation = 1
                        };
                        _memberRepository.AddMember(NewEmployer);
                        Employment.EmployerID = NewEmployer.UID;
                    }                   
                }
                string addedBy = _userManager.GetUserId(User).ToString();
                Employment.AddedBy = addedBy;
                Employment.ID = Guid.NewGuid();
                _employmentRepository.InsertEmployment(Employment, PolicyID);
                CheckEmploymentRules(PolicyTypeID, PolicyID, ID);
                TS.Complete();
            }
            return Redirect("Submission?id=" + ID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID);
        }
        public IActionResult OnPostNext(Guid id, Guid policyTypeid, Guid policyid)
        {
            //string addedBy = _userManager.GetUserId(User).ToString(); 
            //_policyRepository.UpdatePolicyStatus(policyid, 5);
            CheckEmploymentRules(policyTypeid, policyid, id);
            return Redirect("Submission?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
        }
        private void CheckEmploymentRules(Guid PolicyTypeID,Guid PolicyID,Guid ProposerUID)
        {
            BusinessRulesParameters newBusinessParameters = new BusinessRulesParameters();
            newBusinessParameters.PolicyTypeID = PolicyTypeID;
            newBusinessParameters.PolicyID = PolicyID;
            newBusinessParameters.ProposerUID = ProposerUID;
            StatusReport statusReport = _businessRuleRepository.CheckRules(PolicyTypeID, "Premium Payer Employment Details", newBusinessParameters);
            if (statusReport.SuccessStatus == 0)
            {
                throw new Exception(statusReport.StatusMessage);
            }
        }
    }
}
