using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.PolicyServicing.Pages.Policies
{
    public class StatusApprovalModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyRepository _policyRepository;
        private readonly IStatiiRepository _statiiRepository;
        private readonly IStatiiReasonsRepository _statiiReasonsRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IPolicyStatiiStagingRepository _policyStatiiStagingRepository;
        [BindProperty]
        public Policy Policy { get; set; }
        public Member Member { get; set; }
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
        public DataTable ProposedStatusDT { get; set; }
        public DataTable StatiiHistoryDT { get; set; }
        [BindProperty]
        public int StatusID { get; set; }
        [BindProperty]
        public string StatusComment { get; set; }
        public StatusApprovalModel(UserManager<ApplicationUser> userManager,
           IStatiiReasonsRepository statiiReasonsRepository,
           IPolicyRepository policyRepository,
           IPolicyStatiiStagingRepository policyStatiiStagingRepository,
           IMemberRepository memberRepository,
           IStatiiRepository statiiRepository)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _statiiRepository = statiiRepository;
            _statiiReasonsRepository = statiiReasonsRepository;
            _memberRepository = memberRepository;
            _policyStatiiStagingRepository = policyStatiiStagingRepository;
        }
        public void OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            PolicyID = policyid;
            MemberID = id;
            PolicyTypeID = policyTypeid;
            RequestID = reqid;
            Policy = _policyRepository.GetPolicyById(policyid);
            Member = _memberRepository.GetMemberById(id);
            StatiiHistoryDT = _policyRepository.GetPolicyStatusHistory(policyid);
            ProposedStatusDT = _policyStatiiStagingRepository.GetStatusHistory(RequestID);
        }
        public IActionResult OnPost()
        {
            string addedBy = _userManager.GetUserId(User).ToString();
            if (StatusID == 10)
            {
                using (TransactionScope TS = new TransactionScope())
                {
                    _policyRepository.PolicyServicingMessagesAdd(PolicyID, MemberID, 6, "REF: Policy status updated.", addedBy);
                    _policyStatiiStagingRepository.UpdatePolicyStatus(RequestID, addedBy);
                    _policyRepository.UpdatePolicyServicingRequests(RequestID, 10);
                    TS.Complete();
                }
            }
            else if (StatusID == 9)
            {
                _policyRepository.UpdatePolicyServicingRequests(RequestID, 9);
                _policyRepository.PolicyServicingMessagesAdd(PolicyID, MemberID, 6, "REF:  Policy status update rejected.", addedBy);
            }
            else if (StatusID == 8)
            {
                _policyRepository.UpdatePolicyServicingRequests(RequestID, 8);
                _policyRepository.PolicyServicingMessagesAdd(PolicyID, MemberID, 6, "REF: More information requested for  policy status update.", addedBy);
            }
            return Redirect("StatusIndex?id=" + MemberID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
        }
    }
}
