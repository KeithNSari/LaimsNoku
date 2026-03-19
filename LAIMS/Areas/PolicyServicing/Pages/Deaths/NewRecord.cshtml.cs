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
    public class NewRecordModel : PageModel
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
        public Member Member { get; set; }
        [BindProperty]
        public Guid MemberUID { get; set; }
        [BindProperty]
        public Guid RequestID { get; set; }
        [BindProperty]
        public List<DeathRecord> DeathRecords { get; set; }
        public NewRecordModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository,
            IPolicyPremiumRepository policyPremiumRepository,IMemberRepository memberRepository, 
            IPolicyClaimRepository policyClaimRepository, IPolicyPremiumLineRepository policyPremiumLineRepository)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _policyClaimRepository = policyClaimRepository;
            _policyPremiumLineRepository = policyPremiumLineRepository;
            _policyPremiumRepository = policyPremiumRepository;
            _memberRepository = memberRepository;
        }
        public void OnGet(Guid id)
        {
            MemberUID = id;
            Member = _memberRepository.GetMemberById(id);
            LoadEventCausesSelectList(DeathEventID);
        }
        private void LoadEventCausesSelectList(Guid EventTypeID)
        {
            foreach (EventTypeCause eventTypeCause in _policyClaimRepository.GetEventTypeCauses(EventTypeID))
            {
                EventTypeCauseList.Add(new SelectListItem
                {
                    Value = eventTypeCause.ID.ToString(),
                    Text = eventTypeCause.Cause
                });
            }
        }
        public IActionResult OnPost()
        {
            try
            {
                if (DeathRecord.DateOfDeath > DateTime.Today) throw new Exception("Date of death cannot be later than today!");
                string AddedBy = _userManager.GetUserId(User).ToString();
                Guid RequestID = Guid.NewGuid();
                DeathRecord.RequestID = RequestID;
                DeathRecord.MemberID= _memberRepository.GetID(MemberUID);
                DeathRecord.AddedBy = AddedBy;
                if (_policyClaimRepository.CheckDeathRecordExistence(DeathRecord.MemberID) > 0)
                {
                    throw new Exception("A death record already exists for this member!");
                }
                _policyClaimRepository.AddNewDeathRecord(DeathRecord);
                _policyRepository.PolicyServicingMessagesAdd(Guid.Empty,MemberUID, 8, "REF: Death record on member", AddedBy);
                //change below method to accommodate memberuid, currently work around
                _policyRepository.InsertPolicyServicingRequests(MemberUID, RequestID, 6, 8, AddedBy, DateTime.Now, 0);
                return Redirect("Details?reqid=" + RequestID + "&id=" + MemberUID);
            }
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = "Details?reqid=" + RequestID + "&id=" + MemberUID });
			} 
        } 
    }
}