using Azure.Core;
using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Claims;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Policies;
using LAIMS.Models.Security;
using LAIMS.Models.Utilities;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;

namespace LAIMS.Areas.Claims.Pages
{
    [Authorize(Roles = "Claims Initiator")]
    public class DeathClaimsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyClaimRepository _policyClaimRepository;
        private readonly IPolicyPremiumLineRepository _policyPremiumLineRepository;
        private readonly IPolicyPremiumRepository _policyPremiumRepository;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        private Guid DeathEventID = Guid.Parse("053A901D-2A31-4471-9985-D5C977209019");
        public List<SelectListItem> EventTypeCauseList = new List<SelectListItem>();
        public DataTable BeneficiariesDT;
        [BindProperty ]
        public DeathRecord DeathRecord { get; set; }
        [BindProperty]
        public Policy Policy { get; set; }

        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid ProposerID { get; set; }
        [BindProperty]
        public Guid RequestID { get; set; }
        [BindProperty ]
        public List<DeathRecord> DeathRecords { get; set; }
		[BindProperty(SupportsGet = true)]
		public string ReturnUrl { get; set; }
		public DeathClaimsModel(UserManager<ApplicationUser> userManager, IPolicyPremiumRepository policyPremiumRepository,
            IPolicyClaimRepository policyClaimRepository, IPolicyPremiumLineRepository policyPremiumLineRepository,
            IBusinessRuleRepository businessRuleRepository)
        {
            _userManager = userManager;
            _policyClaimRepository = policyClaimRepository;
            _policyPremiumLineRepository=policyPremiumLineRepository;
            _policyPremiumRepository = policyPremiumRepository;
            _businessRuleRepository = businessRuleRepository;
        }
        public IActionResult OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
		{
            try
            {
				ReturnUrl = Request.Path + Request.QueryString;
				if (_policyClaimRepository.CountDeathRecords(policyid) == 0)
                {
                    throw new Exception("No members in this policy have an existing death record!");
                }
                LoadData(id, policyTypeid, policyid, reqid);
            }
            catch (Exception ex)
            {
				return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
			}
			return Page();
		}
        private void LoadData(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            try
            {
				ProposerID = id;
				PolicyTypeID = policyTypeid;
				PolicyID = policyid;
				RequestID = reqid;
				BeneficiariesDT = _policyClaimRepository.GetDeceasedBeneficiariesByClaimType(policyid, 1);
			}
			catch (Exception ex)
			{
				ViewData["ErrorMessage"] = "OOPS: " + ex.Message;
			}			
		}  
        public IActionResult OnPostNext(Guid id, Guid policyTypeid, Guid policyid, Guid reqid, int[] selectedRecords)
        {
            try
            {
                if (selectedRecords == null)
                {
                    throw new Exception("You must select at least one deceased person for this type of claim.");
                } 
				int claimID = _policyClaimRepository.GetClaimIDByRequestID(reqid);
                foreach (int record in selectedRecords)
                {
                    int memberID = record;

                    _policyClaimRepository.AddDeaths(memberID, claimID);
                }
                return Redirect("Claimants?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                LoadData(id, policyTypeid, policyid, reqid);
                ViewData["ErrorMessage"] += Environment.NewLine + "OOPS: " + ex.Message;
            }
            return Page();
        }
    }
}
