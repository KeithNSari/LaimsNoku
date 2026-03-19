using Azure.Core;
using LAIMS.Interfaces.Investments;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.Investments;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Policies;
using LAIMS.Models.Security;
using LAIMS.Pages.PolicySetUps.Pages.PolicyTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.Investments.Pages
{
    public class PolicyUnitsIndexModel : PageModel
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
        public decimal AvailableBalance { get; set; }
        [BindProperty ]
        public List<UnitsPricesList> UnitPrices { get; set; } 
        public PolicyUnitsIndexModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository, 
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
             
            AvailableBalance = 0.00m;
            SearchTerm = search;
            SearchPageUrl = "PolicyUnitsIndex";
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                AvailableBalance = _policyRepository.GetInvestmentContentBalance(SearchTerm);
                PoliciesDT = _policyRepository.PolicyInvestmentSummary(SearchTerm);
                UnitTrustBalancesDT = _unitTrustRepository.GetTotals(SearchTerm);
                TransactionHistoryDT = _unitTrustRepository.GetLatestPurchaseTransactions(SearchTerm);
                UnitPrices = _unitsPricesListRepository.GetCurrentPrices();
                ResultsCount = PoliciesDT.Rows.Count;
            }
        }
        public IActionResult OnPost(Guid[] UnitTrustIDS, decimal[] PurchaseAmounts)
        {
            string url = "/Investments/PolicyUnitsIndex?handler=search&search=" + SearchTerm;
            try
            {
                using (TransactionScope TS = new TransactionScope())
                {
                    int CurrencyID = _policyRepository.GetPolicyCurrency(SearchTerm); 
                    string addedBy = _userManager.GetUserId(User).ToString();
                    AvailableBalance = _policyRepository.GetInvestmentContentBalance(SearchTerm);
                    Guid policyID=_policyRepository.GetPolicyID(SearchTerm); 
                   
                    if (AvailableBalance >= PurchaseAmounts.Sum())
                    {                        
                        //decimal totalValue = 0m;
                        for (int i = 0; i < UnitTrustIDS.Length; i++)
                        {
                            Guid trustID = UnitTrustIDS[i];
                            UnitsPricesList unitLatestPricing = _unitsPricesListRepository.GetLatestPrice(CurrencyID, trustID);
                            if (unitLatestPricing.OfferPrice > 0)
                            {
                                decimal purchaseAmount = PurchaseAmounts[i];
                                decimal units = purchaseAmount/ unitLatestPricing.OfferPrice;

                                if (units > 0)
                                {
                                    int ID = _unitsPricesListRepository.GetPolicyUnitsHeader(policyID, trustID);
                                    int policyunitsID = ID == 0 ? _unitsPricesListRepository.InsertPolicyUnitsHeader(policyID, trustID, addedBy) : ID;
                                    _unitsPricesListRepository.Buy(policyID, trustID, policyunitsID, units, unitLatestPricing.ID, 3, addedBy);
                                    _unitsPricesListRepository.UpdateInvestmentContentBalance(policyID, purchaseAmount);
                                }
                                //totalValue += purchaseAmount;
                            }  
                        }
                    }
                    else
                    {
                        throw new Exception("Insufficient balance!");
                    }
                    TS.Complete();
                }
                return Redirect("PolicyUnitsIndex?handler=search&search="+ SearchTerm);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = url });
            }
        }
    }
}
