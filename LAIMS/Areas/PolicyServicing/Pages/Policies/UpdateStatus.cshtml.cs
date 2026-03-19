using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages; 
using System.Data;
using System.Transactions;
using LAIMS.Models.Security;

namespace LAIMS.Areas.PolicyServicing.Pages.Policies
{
    public class UpdateStatusModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyRepository _policyRepository;
        private readonly IStatiiRepository _statiiRepository;
        private readonly IStatiiReasonsRepository _statiiReasonsRepository;
        private readonly IPolicyStatiiStagingRepository _policyStatiiStagingRepository;
        private readonly IMemberRepository _memberRepository;
        [BindProperty]
        public Policy Policy { get; set; }
        public Member Member { get; set; }
        [BindProperty]
        public PolicyStatiiStaging PolicyStatiiStaging { get; set; }
        [BindProperty(SupportsGet = true)]
        public int StatusID { get; set; } 
        [BindProperty]
        public int StatusReasonID { get; set; }
        [BindProperty]
        public string StatusComment { get; set; }
        [BindProperty]
        public DateTime EffectiveDate { get; set; }
        public List<Statii> StatiiList { get; set; }
        public List<StatiiReason> StatiiReasons { get; set; }
        public DataTable StatiiHistoryDT { get; set; }
        [BindProperty]
        public Guid RequestID { get; set; }
        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid MemberID { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public UpdateStatusModel(UserManager<ApplicationUser> userManager,
            IStatiiReasonsRepository statiiReasonsRepository,
            IPolicyRepository policyRepository,
            IMemberRepository memberRepository,
            IPolicyStatiiStagingRepository policyStatiiStagingRepository,
            IStatiiRepository statiiRepository)
        {
            _userManager = userManager; 
            _policyRepository = policyRepository;
            _memberRepository = memberRepository;
            _statiiRepository = statiiRepository;
            _statiiReasonsRepository = statiiReasonsRepository;
            _policyStatiiStagingRepository = policyStatiiStagingRepository;
        }
        public void OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            EffectiveDate = DateTime.Now;
            ReturnUrl = Request.Path + Request.QueryString;
            PolicyID = policyid;
            MemberID = id;
            PolicyTypeID = policyTypeid;
            RequestID = reqid;
            Policy = _policyRepository.GetPolicyById(policyid);
            Member = _memberRepository.GetMemberById(id);
            StatiiList = _statiiRepository.GetAllSelectablePolicyStatii();
            StatiiHistoryDT = _policyRepository.GetPolicyStatusHistory(policyid);
        }
        public JsonResult OnGetReasonsByStatus()
        {
            var reasons = _statiiReasonsRepository.GetAllStatiiReasons(StatusID);
            return new JsonResult(reasons);
        }
        public IActionResult OnPost()
        {
            try
            {
                if (EffectiveDate > DateTime.Now) throw new Exception("Effective date cannot be in the future");
                using(TransactionScope TS = new TransactionScope())
                {
                    string AddedBy = _userManager.GetUserId(User).ToString();
                    PolicyStatiiStaging policyStatiiStaging = new()
                    {
                        RequestID = RequestID,
                        PolicyID = PolicyID,
                        PolicyStatus = StatusID,
                        PolicyStatusReason = StatusReasonID,
                        PolicyStatusDate = EffectiveDate,
                        PolicyStatusComment = StatusComment,
                        AddedBy = AddedBy
                    };
                    _policyStatiiStagingRepository.Add(policyStatiiStaging);
                    _policyRepository.InsertPolicyServicingRequests(PolicyID, RequestID, 6, 6, AddedBy, DateTime.Now, 0);
                    TS.Complete();
                }
                return Redirect("StatusIndex?id=" + MemberID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);

            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
}
