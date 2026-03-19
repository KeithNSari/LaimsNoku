using Azure.Core;
using LAIMS.Interfaces.Claims;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.Claims.Pages
{
    [Authorize(Roles = "Claims Initiator")]
    public class ClaimServicesFullListModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPolicyClaimRepository _policyClaimRepository;
        public DataTable ServiceBreakdownDT;
        [BindProperty]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid ProposerID { get; set; }
        [BindProperty]
        public int PolicyBeneficiariesLineID { get; set; }
        [BindProperty]
        public Guid RequestID { get; set; }
        public ClaimServicesFullListModel(UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment
            ,IPolicyClaimRepository policyClaimRepository)
        {
            _userManager = userManager; 
            _webHostEnvironment = webHostEnvironment;
            _policyClaimRepository = policyClaimRepository;
        }
        public void OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            ProposerID = id;
            PolicyTypeID = policyTypeid;
            PolicyID = policyid;
            RequestID = reqid;
            int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID); 
            ServiceBreakdownDT = _policyClaimRepository.GetServiceBreakdown(claimID);
        }
        public IActionResult OnPostAddShares(int[] SVIDS, decimal[] SVShares)
        {
            try
            {
                using (TransactionScope TS = new TransactionScope())
                {
                    string addedBy = _userManager.GetUserId(User).ToString();
                    int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
                    decimal totalValue = 0m;
                    for (int i = 0; i < SVIDS.Length; i++)
                    {
                        int recordID = SVIDS[i];
                        decimal shareValue = SVShares[i];
                        totalValue += shareValue;
                        if (shareValue > 0)
                        {
                            _policyClaimRepository.AddServiceBreakdown(claimID, recordID, shareValue, addedBy); 
                        }
                    }
                    //if (totalValue != 100) throw new Exception("Split must add up to a 100%!"); 
                    TS.Complete();
                }
                return Redirect("ClaimServicesFullList?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostNext()
        {
            try
            {
                return Redirect("ClaimDocuments?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
}
