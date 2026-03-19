using Azure.Core;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Investments;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Questionnaires;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using System.Data;
using System.Transactions;


namespace LAIMS.Areas.PolicyServicing.Pages.Policies
{
    public class BeneficiarySharesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyBeneficiaryRepository _policyBeneficiaryRepository;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IMemberRepository _memberRepository;
        public BeneficiarySharesModel(UserManager<ApplicationUser> userManager,
            IPolicyTypeRepository policyTypeRepository,
            IPolicyBeneficiaryRepository policyBeneficiaryRepository,
            IPolicyRepository policyRepository,
            IMemberRepository memberRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
            _policyBeneficiaryRepository = policyBeneficiaryRepository;
            _policyRepository = policyRepository;
            _memberRepository = memberRepository;
        }
        [BindProperty]
        public Policy Policy { get; set; }
        public Member Member { get; set; }
        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid MemberID { get; set; }
        [BindProperty]
        public DataTable BeneficiarySharesDT { get; set; }
        [BindProperty]
        public Guid RequestID { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        public void OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            RequestID = reqid;
            PolicyID = policyid;
            MemberID = id;
            PolicyTypeID = policyTypeid;
            Policy = _policyRepository.GetPolicyById(policyid);
            Member = _memberRepository.GetMemberById(id); 
            PolicyTypeID = policyTypeid;
            PolicyID = policyid; 
            BeneficiarySharesDT = _policyBeneficiaryRepository.GetStagingBeneficiaryShares(policyid,reqid); 
        }
        public IActionResult OnPostAddShares(int[] PBIDS, decimal[] Shares)
        {
            try
            {
                ViewData["ErrorMessage"] = "";
                using (TransactionScope TS = new TransactionScope())
                {
                    string addedBy = _userManager.GetUserId(User).ToString();
                    _policyBeneficiaryRepository.ArchivePBLSplitStaging(PolicyID, addedBy);
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
                            _policyBeneficiaryRepository.AddPBLSplitStaging(pBLSplit, RequestID);
                        }
                    }
                    if (totalValue != 100) throw new Exception("Split must add up to a 100%!");
                    ViewData["ErrorMessage"] = "Updated!"; //not an error, change to notification
                    TS.Complete();
                }
                return Redirect(ReturnUrl);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostNext(Guid id, Guid policyTypeid, Guid policyid)
        {
            try
            {
                return Redirect("BeneficiaryIndex?id=" + MemberID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            } 
        }
    }
}
