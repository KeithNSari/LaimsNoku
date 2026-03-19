using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Text;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
	[Authorize(Roles = "New Business Initiator")]
	public class SelectPolicyModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        public DataTable PolicyTypesDT;
        [BindProperty]
        public Guid ApplicantID { get; set; }
        public SelectPolicyModel(UserManager<ApplicationUser> userManager, 
            IPolicyTypeRepository policyTypeRepository,
            IPolicyRepository policyRepository,
            IMemberRepository memberRepository,
            IBusinessRuleRepository businessRuleRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
            _policyRepository = policyRepository;
            _memberRepository = memberRepository;
            _businessRuleRepository = businessRuleRepository;
        }
        public IActionResult OnGet(string id)
        {
            string AddedBy = _userManager.GetUserId(User).ToString();
            if (string.IsNullOrWhiteSpace(id)) { return NotFound(); }
            if (!_memberRepository.CheckIDConfirm(Guid.Parse(id)))
            {
               return Redirect("~/NewBusiness/Membership/ConfirmIdentity?id=" + id);
            }
            if (Guid.TryParse(id, out Guid _applicantID))
            {
                ApplicantID = _applicantID;
                PolicyTypesDT = _policyTypeRepository.GetByDesignation(AddedBy);
            }
            else
            {
                return Page();
            }
            return Page();
        }
        public IActionResult OnPost(string id, string policyTypeid)
        {
            string AddedBy = _userManager.GetUserId(User).ToString();
            //try
            //{
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(policyTypeid))
                {
                    return Page();
                }
                Guid PolicyTypeID=Guid.Parse(policyTypeid);               
                bool policyTypeDesignationPermissionAllow = _policyTypeRepository.CheckDesignationPermission(AddedBy, PolicyTypeID);
                if (!policyTypeDesignationPermissionAllow) throw new Exception("You are not authorised to create this type of policy!");
                Guid policyID = Guid.NewGuid();
                Policy policy = new Policy();
                policy.ID = policyID;
                policy.MemberUID = Guid.Parse(id);
                policy.PolicyType = PolicyTypeID;
                policy.PolicyStatus = 1;               
                policy.AddedBy = AddedBy;
                policy.AddedOn = DateTime.Now;
                _policyRepository.InsertPolicy(policy);
                BusinessRulesParameters newBusinessParameters = new BusinessRulesParameters();
                newBusinessParameters.PolicyTypeID = policy.PolicyType;
                newBusinessParameters.ProposerUID = policy.MemberUID;
                newBusinessParameters.MemberUID = policy.MemberUID; 
                List<StatusReport> statusReports = _businessRuleRepository.GetAllChecks(policy.PolicyType, "Proposer", newBusinessParameters);
                foreach (StatusReport statusReport in statusReports)
                {
                    _policyRepository.UpdatePolicyStatus(policyID,1, statusReport.RuleID,statusReport.StatusID, statusReport.StatusReasonID, statusReport.StatusMessage, AddedBy);
                } 
                var failedRules = statusReports.Where(sr => sr.SuccessStatus == 0).ToList();
                if (failedRules.Any())
                { 
                    StringBuilder error = new();
                    error.Append("Application rejected: ");
                    foreach (var failedRule in failedRules)
                    {
                        error.Append(failedRule.StatusMessage + ". ");
                    }
                    error.Remove(error.Length - 1, 1);
                    throw new Exception(error.ToString());
                }
				return Redirect("~/NewBusiness/policies/StatusHistory?id=" + id + "&policytypeid=" + policyTypeid + "&policyID=" + policyID); 
            //}
            //catch (Exception ex)
            //{
            //    ViewData["ErrorMessage"] = "OOPS: " + ex.Message;
            //}
            if (Guid.TryParse(id, out Guid _applicantID))
            {
                ApplicantID = _applicantID;
                PolicyTypesDT = _policyTypeRepository.GetByDesignation(AddedBy);
            } 
            return Page(); 
        }
    }
} 
