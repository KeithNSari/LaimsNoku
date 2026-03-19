using LAIMS.Interfaces.Banking;
using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Banking;
using LAIMS.Models.Claims;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LAIMS.Areas.Claims.Pages
{
    [Authorize(Roles = "Claims Initiator")]
    public class ClaimDetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyBeneficiaryLineRepository _policyBeneficiaryLineRepository;
        private readonly IPolicyClaimRepository _policyClaimRepository;
        private readonly IPolicyClaimsLineRepository _policyClaimsLineRepository;
        private readonly IBankRepository _bankRepository;
        public List<SelectListItem> BanksSelectList = new List<SelectListItem>();        
        public List<SelectListItem> ServicesProviderList = new List<SelectListItem>();

        [BindProperty]       
        public Guid PolicyID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid ProposerID { get; set; }
        [BindProperty]
        public Guid RequestID { get; set; }
        [BindProperty]
        public List<int> SelectedRecords { get; set; }
        [BindProperty]
        public List<PolicyBeneficiaryLineDetail> PolicyBeneficiaryLineDetails { get; set; }
        [BindProperty ]
        public ServicesProvider ServicesProvider { get; set; }
        [BindProperty]
        public bool ServicesProviderFound { get;set; }
        [BindProperty]
        public int ServicesProviderID { get; set; }
        [BindProperty]
        public string ServicesProviderSearch { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; } 
        public ClaimDetailsModel(UserManager<ApplicationUser> userManager, 
            IPolicyBeneficiaryLineRepository policyBeneficiaryLineRepository,
            IPolicyClaimRepository policyClaimRepository,
            IPolicyClaimsLineRepository policyClaimsLineRepository,
            IBankRepository bankRepository)
        {
            _userManager = userManager;
            _policyBeneficiaryLineRepository = policyBeneficiaryLineRepository;
            _policyClaimRepository = policyClaimRepository;
            _policyClaimsLineRepository = policyClaimsLineRepository;
            _bankRepository = bankRepository;
        }
        public void OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            ProposerID = id;
            PolicyTypeID = policyTypeid;
            PolicyID = policyid;
            RequestID = reqid;
            PolicyBeneficiaryLineDetails = _policyBeneficiaryLineRepository.GetPolicyBeneficiaryLineDetailsByPolicyID(policyid);
            LoadBanksSelectList();
        }
        public IActionResult OnPostSubmitSelectedRecords()
        {           
            if (SelectedRecords != null && SelectedRecords.Any())
            {
                int headerID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
                if (headerID == 0) throw new Exception("Invalid request!");
                foreach (var selectedLineId in SelectedRecords)
                { 
                    var selectedRecord = PolicyBeneficiaryLineDetails.FirstOrDefault(record => record.LineId == selectedLineId);
                    if (selectedRecord != null)
                    {
                        PolicyClaimsLine policyClaimsLine = new()
                        {
                            HeaderID = headerID,
                            PolicyBeneficiariesLineID = selectedRecord.LineId 
                        };
                        _policyClaimsLineRepository.AddLine(policyClaimsLine);
                    }                   
                }
            }            
            return Redirect("ClaimDocuments?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
        }
        private void LoadBanksSelectList()
        {
            List<Bank> banksList = _bankRepository.GetAllBanks();
            foreach (Bank bank in banksList)
            {
                BanksSelectList.Add(new SelectListItem
                {
                    Value = bank.BankID.ToString(),
                    Text = bank.BankName
                });
            }
        }
        public void OnPostSearchProviders(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            ProposerID = id;
            PolicyTypeID = policyTypeid;
            PolicyID = policyid;
            RequestID = reqid;
            PolicyBeneficiaryLineDetails = _policyBeneficiaryLineRepository.GetPolicyBeneficiaryLineDetailsByPolicyID(policyid);
            LoadBanksSelectList();
            List<Member> OrgList = _policyClaimRepository.SearchServiceProviders(ServicesProviderSearch);
            ResultsCount = OrgList.Count;
            if (ResultsCount > 0)
            {
                ServicesProviderFound = true;
                LoadServicesProviderSelectList(OrgList);
            }

        }
        public void OnPostSearchProviders()
        {
            ReturnUrl = Request.Path + Request.QueryString;
             
        }
        private void LoadServicesProviderSelectList(List<Member> OrgList)
        {
            foreach (Member organisation in OrgList)
            {
                ServicesProviderList.Add(new SelectListItem
                {
                    Value = organisation.ID.ToString(),
                    Text = organisation.Name1
                });
            }
        }
    }
}
