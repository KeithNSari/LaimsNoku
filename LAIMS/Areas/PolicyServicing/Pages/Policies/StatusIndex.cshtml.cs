using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.PolicyServicing.Pages.Policies
{
    public class StatusIndexModel : PageModel
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
        public StatusIndexModel(UserManager<ApplicationUser> userManager,
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
    }
}
