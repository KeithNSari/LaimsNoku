using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Claims;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Security;
using LAIMS.Models.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.PolicyServicing.Pages.Deaths
{
    public class DetailsModel : PageModel
    { 
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IMemberRepository _memberRepository;
            private readonly IPolicyClaimRepository _policyClaimRepository;
            private readonly IPolicyPremiumLineRepository _policyPremiumLineRepository;
            private readonly IPolicyPremiumRepository _policyPremiumRepository;
            private readonly IPolicyRepository _policyRepository;
            private Guid DeathEventID = Guid.Parse("053A901D-2A31-4471-9985-D5C977209019");
            public List<SelectListItem> EventTypeCauseList = new List<SelectListItem>();
            [BindProperty]
            public DeathRecord DeathRecord { get; set; }
        [BindProperty]
        public List<DeathRecord> DeathRecords { get; set; }
        public Member Member { get; set; }
            [BindProperty]
            public Guid MemberUID { get; set; }
            [BindProperty]
            public Guid RequestID { get; set; } 
        public DataTable AssociatedPoliciesDT { get; set; }
            public DetailsModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository,
                IPolicyPremiumRepository policyPremiumRepository, IMemberRepository memberRepository,
                IPolicyClaimRepository policyClaimRepository, IPolicyPremiumLineRepository policyPremiumLineRepository)
            {
                _userManager = userManager;
                _policyRepository = policyRepository;
                _policyClaimRepository = policyClaimRepository;
                _policyPremiumLineRepository = policyPremiumLineRepository;
                _policyPremiumRepository = policyPremiumRepository;
                _memberRepository = memberRepository;
            }
            public void OnGet(Guid id, Guid reqid)
            {
                RequestID = reqid;
                MemberUID = id;
                Member = _memberRepository.GetMemberById(id);
                LoadDeathRecords(RequestID);
                AssociatedPoliciesDT = _policyRepository.GetMemberAssociatedPolicies(MemberUID);
            }
            private void LoadDeathRecords(Guid RequestID)
            {             
               DeathRecords = _policyClaimRepository.GetAllDeathRecords(RequestID);
            }
    }
}
