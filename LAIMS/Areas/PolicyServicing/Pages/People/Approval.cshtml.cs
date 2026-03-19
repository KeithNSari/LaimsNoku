using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.PolicyServicing.Pages.People
{
	//[Authorize(Roles = "Policy Servicing Approver")]
	public class ApprovalModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IPolicyPremiumRepository _policyPremiumRepository;
        private readonly IPolicyBeneficiaryLineRepository _policyBeneficiaryLineRepository;
        private readonly IPolicyBeneficiaryRepository _policyBeneficiaryRepository;
        public ApprovalModel(IMemberRepository memberRepository, UserManager<ApplicationUser> userManager,
            IPolicyRepository policyRepository, IPolicyPremiumRepository policyPremiumRepository,
            IPolicyBeneficiaryLineRepository policyBeneficiaryLineRepository,
            IPolicyBeneficiaryRepository policyBeneficiaryRepository)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            _policyPremiumRepository = policyPremiumRepository;
            _policyBeneficiaryLineRepository = policyBeneficiaryLineRepository;
            _policyBeneficiaryRepository = policyBeneficiaryRepository;
        }
        [BindProperty]
        public Guid MemberUID { get; set; }
        [BindProperty ]
        public Guid RequestID { get; set; }
        public Member Member { get; set; }
        public Member StagingMember { get; set; }
        [BindProperty]
        public int StatusID { get; set; }
        [BindProperty]
        public string StatusComment { get; set; }
        [BindProperty]
        public PolicyPremium PolicyPremium { get; set; } 
       
        public IActionResult OnGet(Guid id, Guid requestid)
        {
            MemberUID = id;
            RequestID = requestid;
            Member = _memberRepository.GetMemberById(id);
            if (Member == null)
            {
                return NotFound();
            }
            StagingMember = _memberRepository.GetStagingMemberById(id);
            return Page();
        }
        public IActionResult OnPost()
        {
            string AddedBy = _userManager.GetUserId(User).ToString();
            if (StatusID == 10)
            {
                Member = _memberRepository.GetMemberById(MemberUID);
                StagingMember = _memberRepository.GetStagingMemberById(MemberUID);
                bool genderUpdate = Member.GenderID != StagingMember.GenderID;
                bool ageISDifferenceGreaterThanOneYear = CheckIfAgeDifferenceIsGreaterThanOneYear(Member.DOB.Value, StagingMember.DOB.Value);
                _memberRepository.UpdateMemberFromCopy(MemberUID, RequestID, AddedBy);
                _policyRepository.PolicyServicingMessagesAdd(Guid.Empty, MemberUID, 1, "Please note your personal details have been edited. If this change is unexpected, please contact us!", AddedBy);
               
                if ( genderUpdate||ageISDifferenceGreaterThanOneYear)
                {
                    //CreatePolicyCoverChangeRequests(MemberUID, AddedBy);
                }              
            }
            _memberRepository.UpdateCopyStatus(MemberUID, RequestID, StatusID, StatusComment, AddedBy);
             return Redirect("Reviews");
        }
        private bool CheckIfAgeDifferenceIsGreaterThanOneYear(DateTime startDate, DateTime endDate)
        {
            DateTime earlierDate = startDate < endDate ? startDate : endDate;
            DateTime laterDate = startDate < endDate ? endDate : startDate;

            bool isDifferenceGreaterThanOneYear = (laterDate.Year - earlierDate.Year) -
                ((laterDate.Month < earlierDate.Month ||
                (laterDate.Month == earlierDate.Month && laterDate.Day < earlierDate.Day)) ? 1 : 0) > 1;
            return isDifferenceGreaterThanOneYear;
        }
        private void CreatePolicyCoverChangeRequests(Guid MemberUID, string AddedBy)
        {
            DataTable beneficiaryLinesDT = _policyBeneficiaryLineRepository.GetRiskPolices(MemberUID);
            foreach(DataRow dr in beneficiaryLinesDT.Rows)
            {
                Guid requestID = Guid.NewGuid();
                Guid policyID = Guid.Parse(dr["PolicyID"].ToString());
                Guid productID = Guid.Parse(dr["ProductID"].ToString());
                decimal cover = Convert.ToDecimal(dr["Cover"].ToString());
                int policyBeneficiaryID = Convert.ToInt32(dr["PolicyBeneficiaryID"].ToString());
                int policyBeneficiariesLineID= Convert.ToInt32(dr["PolicyBeneficiariesLineID"].ToString());
                int policyPremiumID=AddRequestPremiumHeader(AddedBy,policyID, requestID);
                int riskGroup = _policyBeneficiaryRepository.GetPolicyBeneficiaryRiskGroup(policyBeneficiaryID);
				decimal premium= _policyPremiumRepository.GetPremiumRates(productID, policyBeneficiaryID , cover,riskGroup);

                _policyBeneficiaryLineRepository.ProposeBeneficiaryLineArchive(policyBeneficiariesLineID, RequestID, AddedBy);

                PolicyBeneficiaryLine policyBeneficiaryLine = new PolicyBeneficiaryLine()
                {
                    HeaderID=policyBeneficiaryID, 
                    PolicyPremiumID = policyPremiumID,
                    RequestID = RequestID,
                    Approved = 0,
                    ProductID=productID,
                    Contribution = premium,
                    Cover=cover,
                    AddedOn = DateTime.Now,
                    AddedBy = AddedBy,
                    Current = 1
                };     
                _policyBeneficiaryLineRepository.InsertPolicyBeneficiaryLine(policyBeneficiaryLine);
                _policyPremiumRepository.UpdatePremiumAmount(policyID, policyPremiumID);
                _policyRepository.InsertPolicyServicingRequests(policyID, RequestID,8,7,AddedBy, DateTime.Now, 0);
            }
        }
        private int AddRequestPremiumHeader(string AddedBy, Guid PolicyID, Guid RequestID)
        {
            PolicyPremium policyPremium = new PolicyPremium();
            policyPremium.HeaderID = PolicyID;
            policyPremium.PremiumPayer = 0; ;
            policyPremium.AddedBy = AddedBy;
            policyPremium.AddedOn = DateTime.Now;
            int policyPremiumID = _policyPremiumRepository.AddPolicyPremium(policyPremium, RequestID);
            return policyPremiumID;
        }

    }
}
