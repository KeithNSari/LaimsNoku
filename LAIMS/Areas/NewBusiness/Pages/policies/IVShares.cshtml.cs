using Azure.Core;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Investments;
using LAIMS.Models.Policies;
using LAIMS.Models.Questionnaires;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
	[Authorize(Roles = "New Business Initiator")]
	public class IVSharesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyBeneficiaryRepository _policyBeneficiaryRepository;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IPolicyRepository _policyRepository;
        public IVSharesModel(UserManager<ApplicationUser> userManager,
            IPolicyTypeRepository policyTypeRepository,
            IPolicyBeneficiaryRepository policyBeneficiaryRepository,
            IPolicyRepository policyRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
            _policyBeneficiaryRepository = policyBeneficiaryRepository;
            _policyRepository = policyRepository; 
        }
        [BindProperty]
        public string? PolicyName { get; set; }
        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid ProposerID { get; set; }
        [BindProperty ]
        public DataTable BeneficiarySharesDT { get; set; }
        public IActionResult OnGet(Guid id, Guid policyTypeid, Guid policyid)
        {
            string ReturnUrl = Request.Path + Request.QueryString;            
            try
            {
                ProposerID = id;
                PolicyTypeID = policyTypeid;
                PolicyID = policyid; 
                BeneficiarySharesDT = _policyBeneficiaryRepository.GetBeneficiaryShares(policyid);
                PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
				_policyRepository.UpdatePolicyStage(PolicyID, 3);
				return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }         
        }
        public void OnPostAddShares(int[] PBIDS, decimal[] Shares)
        {
            try
            {
                ViewData["ErrorMessage"] = "";
                using (TransactionScope TS = new TransactionScope())
                {
                    string addedBy = _userManager.GetUserId(User).ToString();
                    _policyBeneficiaryRepository.ArchivePBLSplit(PolicyID, addedBy);
                    Guid batchID = Guid.NewGuid();
                    decimal totalValue = 0m;
                    for (int i = 0; i < PBIDS.Length; i++)
                    {
                        int recordID = PBIDS[i];
                        decimal shareValue = Shares[i];
                        totalValue += shareValue;
                        if (shareValue > 0)
                        {
                            PBLSplit pBLSplit = new()
                            {
                                BatchID = batchID,
                                PolicyID = PolicyID,
                                PolicyBeneficiaryID = recordID,
                                SplitPercentage = shareValue,
                                AddedBy = addedBy,
                                AddedOn = DateTime.Now
                            };
                            _policyBeneficiaryRepository.AddPBLSplit(pBLSplit);
                        }
                    }
                    if (totalValue != 100) throw new Exception("Split must add up to a 100%!");				
					ViewData["ErrorMessage"] = "Updated!"; //not an error, change to notification
                    TS.Complete();
                }
                BeneficiarySharesDT = _policyBeneficiaryRepository.GetBeneficiaryShares(PolicyID);
                PolicyName = _policyTypeRepository.GetPolicyTypeName(PolicyTypeID);
            }
            catch (Exception ex)
            {
                BeneficiarySharesDT = _policyBeneficiaryRepository.GetBeneficiaryShares(PolicyID);
                ViewData["ErrorMessage"] = "OOPS: " + ex.Message;
            } 
        }
        public IActionResult OnPostNext(Guid id, Guid policyTypeid, Guid policyid)
        {
            try
            {
                decimal splitTotal=_policyBeneficiaryRepository.GetSplitTotal(policyid);
                if (splitTotal != 100) throw new Exception("Please fill in the required split values first!");
                return Redirect("Documents?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
            }
            catch (Exception ex)
            {
                BeneficiarySharesDT = _policyBeneficiaryRepository.GetBeneficiaryShares(PolicyID);
                ViewData["ErrorMessage"] = "OOPS: " + ex.Message;
            }
            return Page();
        }
    }
}
