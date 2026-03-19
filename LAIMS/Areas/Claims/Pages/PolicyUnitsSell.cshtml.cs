using LAIMS.Interfaces.Investments;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.Investments;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.Claims.Pages
{
    [Authorize(Roles = "Claims Initiator")]
    public class PolicyUnitsSellModel : PageModel
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitTrustRepository _unitTrustRepository;
        private readonly IUnitsPricesListRepository _unitsPricesListRepository;
        public DataTable PoliciesDT;
        public DataTable UnitTrustBalancesDT;
        public DataTable TransactionHistoryDT;
        public string SearchPageUrl;

        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0; 
        [BindProperty]
        public List<UnitsPricesList> UnitPrices { get; set; } 
        public PolicyUnitsSellModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository,
            IWebHostEnvironment webHostEnvironment, IUnitsPricesListRepository unitsPricesListRepository,
            IUnitTrustRepository unitTrustRepository)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _webHostEnvironment = webHostEnvironment;
            _unitTrustRepository = unitTrustRepository;
            _unitsPricesListRepository = unitsPricesListRepository;
        }
        public void OnGet()
        {
        }
        public void OnGetSearch(string? search)
        {
            SearchTerm = search;
            SearchPageUrl = "PolicyUnitSell";
            if (!string.IsNullOrEmpty(SearchTerm))
            { 
                PoliciesDT = _policyRepository.PolicyInvestmentSummary(SearchTerm);
                UnitTrustBalancesDT = _unitTrustRepository.GetSalesDetails(SearchTerm);
                TransactionHistoryDT = _unitTrustRepository.GetLatestSalesTransactions(SearchTerm); 
                ResultsCount = PoliciesDT.Rows.Count;
            }
        }
        public IActionResult OnPost(Guid[] UnitTrustIDS, decimal[] PurchaseQuantities)
        {
            string url = "/Claims/PolicyUnitsSell?handler=search&search=" + SearchTerm;
            try
            {
                using (TransactionScope TS = new TransactionScope())
                {
                    int CurrencyID = _policyRepository.GetPolicyCurrency(SearchTerm);
                    string addedBy = _userManager.GetUserId(User).ToString();
                    Guid policyID = _policyRepository.GetPolicyID(SearchTerm);
                    //decimal totalValue = 0m;
                    for (int i = 0; i < UnitTrustIDS.Length; i++)
                    {
                        Guid trustID = UnitTrustIDS[i];
                        UnitsPricesList unitLatestPricing = _unitsPricesListRepository.GetLatestPrice(CurrencyID, trustID);
                        if (unitLatestPricing.BidPrice > 0)
                        {
                            decimal purchaseQuantity = PurchaseQuantities[i]; 
                            decimal totalAvailableUnits = _unitTrustRepository.GetTotalAvailableunits(policyID, trustID);
                            if ((purchaseQuantity > 0) && (purchaseQuantity <= totalAvailableUnits))
                            {
                                int ID = _unitsPricesListRepository.GetPolicyUnitsHeader(policyID, trustID);
                                int policyunitsHeaderID = ID == 0 ? _unitsPricesListRepository.InsertPolicyUnitsHeader(policyID, trustID, addedBy) : ID;
                                _unitsPricesListRepository.Sell(policyID, trustID, policyunitsHeaderID, purchaseQuantity, unitLatestPricing.ID, 2, addedBy);
                            }
                            else if (purchaseQuantity > totalAvailableUnits)
                            {
                                throw new Exception("Units purchased cannot exceed available quantity!");
                            }
                            //totalValue += purchaseAmount;
                        }
                    }
                    TS.Complete();
                }
                return Redirect("PolicyUnitsSell?handler=search&search=" + SearchTerm);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = url });
            }
        }
    }
}
