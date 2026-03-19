using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Investments;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.Investments;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.Claims.Pages
{
    public class PUSellModel : PageModel
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitTrustRepository _unitTrustRepository;
        private readonly IUnitsPricesListRepository _unitsPricesListRepository;
        private readonly IPolicyClaimRepository _policyClaimRepository;
        public DataTable PoliciesDT;
        public DataTable UnitTrustBalancesDT;
        public DataTable TransactionHistoryDT;
        [BindProperty]
        public Guid ProposerID { get; set; }
        [BindProperty]
        public Guid RequestID { get; set; }
        [BindProperty]
        public string PolicyNo { get; set; }
        [BindProperty]
        public List<UnitsPricesList> UnitPrices { get; set; }
        public bool PurchaseMade { get; set; } = false;
        public PUSellModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository,
            IWebHostEnvironment webHostEnvironment, IUnitsPricesListRepository unitsPricesListRepository,
            IUnitTrustRepository unitTrustRepository, IPolicyClaimRepository policyClaimRepository)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _webHostEnvironment = webHostEnvironment;
            _unitTrustRepository = unitTrustRepository;
            _unitsPricesListRepository = unitsPricesListRepository;
            _policyClaimRepository = policyClaimRepository;
        }
        public void OnGet(Guid id, Guid reqid)
        {
            ProposerID = id;
            RequestID = reqid;
            PolicyNo = _policyClaimRepository.GetPolicyNo(RequestID);
            int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
            PurchaseMade = _policyClaimRepository.PurchaseMade(claimID);
            PoliciesDT = _policyRepository.PolicyInvestmentSummary(PolicyNo);
            UnitTrustBalancesDT = _unitTrustRepository.GetSalesDetails(claimID);
            TransactionHistoryDT = _unitTrustRepository.GetLatestSalesTransactions(PolicyNo);
        }
        public IActionResult OnPost()
        {
            string url = "/Claims/PUSell?handler=search&search=" + PolicyNo;
            try
            {
                using (TransactionScope TS = new TransactionScope())
                {
                    int CurrencyID = _policyRepository.GetPolicyCurrency(PolicyNo);
                    string addedBy = _userManager.GetUserId(User).ToString();
                    Guid policyID = _policyRepository.GetPolicyID(PolicyNo);
                    int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
                    Guid trustID = _policyClaimRepository.GetClaimTrust(claimID);
                    UnitsPricesList unitLatestPricing = _unitsPricesListRepository.GetLatestPrice(CurrencyID, trustID);
                    if (unitLatestPricing.BidPrice > 0)
                    {
                        decimal purchaseQuantity = _policyClaimRepository.GetProposedUnits(claimID);
                        decimal totalAvailableUnits = _unitTrustRepository.GetTotalAvailableunits(policyID, trustID);
                        if ((purchaseQuantity > 0) && (purchaseQuantity <= totalAvailableUnits))
                        {
                            int ID = _unitsPricesListRepository.GetPolicyUnitsHeader(policyID, trustID);
                            int policyunitsHeaderID = ID == 0 ? _unitsPricesListRepository.InsertPolicyUnitsHeader(policyID, trustID, addedBy) : ID;
                            _unitsPricesListRepository.Sell(policyID, trustID, claimID, policyunitsHeaderID, purchaseQuantity, unitLatestPricing.ID, 2, addedBy);
                        }
                        else if (purchaseQuantity > totalAvailableUnits)
                        {
                            throw new Exception("Units purchased cannot exceed available quantity!");
                        }
                        //totalValue += purchaseAmount;
                    }
                    PurchaseMade = _policyClaimRepository.PurchaseMade(claimID);
                    TS.Complete();
                }
                return Redirect("PUSell?handler=search&search=" + PolicyNo);
        }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = url
    });
            }
        }
    }
}
