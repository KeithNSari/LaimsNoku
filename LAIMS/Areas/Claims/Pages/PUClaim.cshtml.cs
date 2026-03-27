using Azure.Core;
using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Investments;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Claims;
using LAIMS.Models.Investments;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Policies;
using LAIMS.Pages.NewBusiness.Pages.policies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using LAIMS.Models.Security;

namespace LAIMS.Areas.Claims.Pages
{
   [Authorize(Roles = "Claims Initiator")]
    public class PUClaimModel : PageModel
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitTrustRepository _unitTrustRepository;
        private readonly IUnitsPricesListRepository _unitsPricesListRepository;
        private readonly IPolicyClaimRepository _policyClaimRepository;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        private readonly IObjectRulesStatiiHistoryRepository _objectRulesStatiiHistoryRepository;
        public DataTable PoliciesDT;
        public DataTable UnitTrustBalancesDT;
		public DataTable UnitTransactionsHistoryDT; 
        public DataTable CoverDT;
        public List<SelectListItem> ClaimTypesList { get; set; } = new List<SelectListItem>();
        public string SearchPageUrl;

        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        [BindProperty]
        public List<UnitsPricesList> UnitPrices { get; set; }
        [BindProperty]
        public int ClaimTypeID { get; set; }
        [BindProperty]
        public int ValueMode { get; set; } //1- value is calculated using quantity of units 2- value provided was monetary
        public PUClaimModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository,
            IWebHostEnvironment webHostEnvironment, IUnitsPricesListRepository unitsPricesListRepository,
            IUnitTrustRepository unitTrustRepository, IPolicyClaimRepository policyClaimRepository
            , IBusinessRuleRepository businessRuleRepository,
            IObjectRulesStatiiHistoryRepository objectRulesStatiiHistoryRepository)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _webHostEnvironment = webHostEnvironment;
            _unitTrustRepository = unitTrustRepository;
            _unitsPricesListRepository = unitsPricesListRepository;
            _policyClaimRepository = policyClaimRepository;
            _businessRuleRepository = businessRuleRepository;
            _objectRulesStatiiHistoryRepository = objectRulesStatiiHistoryRepository;
        }
        public void OnGet()
        {

        }
        public IActionResult OnGetSearch(string? search)
        {
			string ReturnUrl = Request.Path + Request.QueryString;
			try
            {
				LoadSearchResults(search);
			}
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
			}
            return Page();			
        }
        private void LoadClaimTypesSelectList()
        {
            ClaimTypesList = new List<SelectListItem>();
            if ((CoverDT == null) || (CoverDT.Rows.Count == 0) || (!CoverDT.Columns.Contains("ProductID")))
            {
                return;
            }

            HashSet<int> addedClaimTypeIds = new HashSet<int>();
            IEnumerable<Guid> productIds = CoverDT.AsEnumerable()
                .Where(row => row["ProductID"] != DBNull.Value)
                .Select(row => Guid.Parse(row["ProductID"].ToString()))
                .Distinct();

            foreach (Guid productId in productIds)
            {
                DataTable claimTypes = _policyClaimRepository.GetClaimTypes(productId);
                foreach (DataRow row in claimTypes.Rows)
                {
                    int claimTypeId = Convert.ToInt32(row["ID"]);
                    if (addedClaimTypeIds.Add(claimTypeId))
                    {
                        ClaimTypesList.Add(new SelectListItem
                        {
                            Value = claimTypeId.ToString(),
                            Text = row["ClaimType"].ToString()
                        });
                    }
                }
            }
        }

        private void LoadSearchResults(string searchTerm)
        {
            SearchTerm = searchTerm?.Trim();
            SearchPageUrl = "PUClaim";
            ResultsCount = 0;
            PoliciesDT = null;
            UnitTrustBalancesDT = null;
            UnitTransactionsHistoryDT = null;
            CoverDT = null;
            ClaimTypesList = new List<SelectListItem>();

            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                return;
            }

            Guid policyID = _policyRepository.GetPolicyID(SearchTerm);
            if (policyID == Guid.Empty)
            {
                return;
            }

            PoliciesDT = _policyRepository.PolicyInvestmentSummary(SearchTerm);
            UnitTrustBalancesDT = _unitTrustRepository.GetSalesDetails(SearchTerm);
            UnitTransactionsHistoryDT = _unitTrustRepository.GetLatestTransactions(policyID);
            ResultsCount = PoliciesDT.Rows.Count;
            CoverDT = _policyClaimRepository.GetNonInvestmentSuppementaryCover(SearchTerm);
            LoadClaimTypesSelectList();
        }

        private bool IsValidClaimTypeSelection()
        {
            return ClaimTypeID > 0 && ClaimTypesList.Any(item => item.Value == ClaimTypeID.ToString());
        }

        public IActionResult OnPost(Guid[] UnitTrustIDS, decimal[] PurchaseQuantities)
        {
            string url = "/Claims/PolicyUnitsSell?handler=search&search=" + SearchTerm;
            try
            {
                Guid policyID = _policyRepository.GetPolicyID(SearchTerm);
                if (policyID == Guid.Empty)
                {
                    throw new Exception("The selected policy could not be found.");
                }

                LoadSearchResults(SearchTerm);
                if (!IsValidClaimTypeSelection())
                {
                    ModelState.AddModelError(nameof(ClaimTypeID), "The selected claim type is not configured for this policy.");
                    return Page();
                }

                Guid RequestID = Guid.NewGuid();
                Guid proposerID = _policyRepository.GetProposerUID(SearchTerm);
                using (TransactionScope TS = new TransactionScope())
                {
                    int CurrencyID = _policyRepository.GetPolicyCurrency(SearchTerm);
                    string addedBy = _userManager.GetUserId(User).ToString();
                    Guid policyTypeID = _policyRepository.GetPolicyTypeID(SearchTerm);
                    PolicyClaim policyClaim = new()
                    {
                        RequestID = RequestID,
                        PolicyID = policyID,
                        ClaimTypeID = ClaimTypeID,
                        ValueMode = ValueMode,
                        AddedOn = DateTime.Now,
                        AddedBy = addedBy,
                        StatusID = 3000,
                        StatusAddedBy = addedBy,
                        StatusDate = DateTime.Now
                    };
					if (ClaimTypeID == 7)
					{
						if (_policyClaimRepository.CountDeathRecords(policyID) == 0)
						{
							throw new Exception("No members in this policy have an existing death record!");
						}
					}
					_policyClaimRepository.AddClaim(policyClaim);
                    decimal totalValue = 0;
                    for (int i = 0; i < UnitTrustIDS.Length; i++)
                    {
                        Guid trustID = UnitTrustIDS[i];
                        UnitsPricesList unitLatestPricing = _unitsPricesListRepository.GetLatestPrice(CurrencyID, trustID);
                        if (unitLatestPricing.BidPrice > 0)
                        {
                            decimal totalAvailableUnits = _unitTrustRepository.GetTotalAvailableunits(policyID, trustID);
                            decimal purchaseQuantity = 0;
                            if (ClaimTypeID == 4)
                            {
                                purchaseQuantity = PurchaseQuantities[i];                               
                            }
                            else
                            {
                                purchaseQuantity = totalAvailableUnits;
                            }
                            decimal amount = purchaseQuantity * unitLatestPricing.BidPrice;
                            if ((purchaseQuantity > 0) && (purchaseQuantity <= totalAvailableUnits))
                            {
                                int ID = _unitsPricesListRepository.GetPolicyUnitsHeader(policyID, trustID);
                                int policyunitsHeaderID = ID == 0 ? _unitsPricesListRepository.InsertPolicyUnitsHeader(policyID, trustID, addedBy) : ID;
                                _unitsPricesListRepository.ProposeSell(policyClaim.ID, policyID, trustID, policyunitsHeaderID, purchaseQuantity, unitLatestPricing.ID, amount, addedBy);
                            }
                            else
                            {
                                throw new Exception("Units purchased must be greater than zero and cannot exceed the available quantity!");
                            }
                            totalValue += amount;                            
                        }						
					}
					if (ClaimTypeID == 7)
					{
						CoverDT = _policyClaimRepository.GetNonInvestmentSuppementaryCover(SearchTerm);
                        LoadClaimTypesSelectList();
						foreach (DataRow DR in CoverDT.Rows)
						{
							totalValue += Convert.ToDecimal(DR["Cover"].ToString());
						}
					}
					_policyClaimRepository.UpdateClaimTotal(RequestID, totalValue);
                    _policyClaimRepository.UpdateStatus(RequestID, 3050, addedBy);
                    CheckInitiationRules(policyTypeID, RequestID, addedBy);
                    TS.Complete();
                }
                return Redirect("PUClaimDocuments?id=" + proposerID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = url });
            }
        }
        private void CheckInitiationRules(Guid PolicyTypeID, Guid RequestID, string AddedBy)
        {
            BusinessRulesParameters claimsSubmissionRulesParameters = new BusinessRulesParameters();
            claimsSubmissionRulesParameters.RequestID = RequestID;
            List<StatusReport> statusReports = _businessRuleRepository.GetAllChecks(PolicyTypeID, "Claims Initiation", claimsSubmissionRulesParameters);
            foreach (StatusReport statusReport in statusReports)
            { 
                ObjectRulesStatiiHistory objectRulesStatiiHistory = new()
                {
                    RequestID = RequestID,
                    SourceID = 5, //allocated to claims, 1 is new business, 2 premiums, 3 commissions, 4 policy servicing
                    MemberID = 0,
                    StatusRuleID = statusReport.RuleID,
                    SuccessStatus = statusReport.SuccessStatus,
                    Status = statusReport.StatusID,
                    StatusReason = statusReport.StatusReasonID,
                    StatusComment = statusReport.StatusMessage,
                    StatusDate = DateTime.Now,
                    StatusAddedBy = AddedBy
                };
                _objectRulesStatiiHistoryRepository.Add(objectRulesStatiiHistory);
                if (statusReport.SuccessStatus == 0)
                {
                    _policyClaimRepository.UpdateStatus(RequestID, 3200, AddedBy);
                    throw new Exception(statusReport.StatusMessage);
                }
            }
        }
    }
}
