using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class ConfirmDetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IPolicyBeneficiaryRepository _policyBeneficiaryRepository;
        private readonly IPolicyBeneficiaryLineRepository _policyBeneficiaryLineRepository;
        public List<PolicyBeneficiaryLineDetail> PolicyBeneficiaryLineDetails { get; set; }
        public ConfirmDetailsModel(UserManager<ApplicationUser> userManager,
            IMemberRepository memberRepository, 
            IPolicyTypeRepository policyTypeRepository,
            IPolicyBeneficiaryRepository policyBeneficiaryRepository,
            IPolicyBeneficiaryLineRepository policyBeneficiaryLineRepository) 
        {
            _userManager = userManager;
            _memberRepository = memberRepository;
            _policyTypeRepository = policyTypeRepository;
            _policyBeneficiaryRepository = policyBeneficiaryRepository;
            _policyBeneficiaryLineRepository = policyBeneficiaryLineRepository;
        }
        [BindProperty]
        public string? PolicyName { get; set; }
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Member Proposer { get; set; } 
        [BindProperty]
        public Guid ProposerID { get; set; }

        [BindProperty]
        public Guid PolicyID { get; set; }
        public DataTable BeneficiariesDT;
        public DataTable BeneficiaryDetailedDT;
        public IActionResult OnGet(Guid id, Guid policyTypeid, Guid policyid)
        {
            PolicyID = policyid;
            ProposerID = id;
            PolicyTypeID = policyTypeid;
            Proposer = _memberRepository.GetMemberById(id);
            if (Proposer == null)
            {
                return NotFound();
            }
            PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
            BeneficiariesDT = _policyBeneficiaryRepository.GetBeneficiaryList(policyid);
            PolicyBeneficiaryLineDetails = _policyBeneficiaryLineRepository.GetPolicyBeneficiaryLineDetailsByPolicyID(policyid);
            BeneficiaryDetailedDT = _policyBeneficiaryRepository.GetBeneficiaryFullDetails(policyid);
            return Page();
        }
    }
}
