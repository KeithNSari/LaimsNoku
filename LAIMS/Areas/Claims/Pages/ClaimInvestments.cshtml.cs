using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Investments;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Investments;
using LAIMS.Models.Policies;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Transactions;

namespace LAIMS.Areas.Claims.Pages
{
    public class ClaimInvestmentsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyBeneficiaryLineRepository _policyBeneficiaryLineRepository;
        private readonly IPolicyClaimRepository _policyClaimRepository;
        private readonly IPolicyClaimsLineRepository _policyClaimsLineRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IPolicyUnitsRepository _policyUnitsRepository;
        private readonly IPolicyUnitsLinesRepository _policyUnitsLinesRepository;

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
        public class CartRow
        {
            public string UnitTrust { get; set; }
            public Guid UnitTrustID { get; set; }
            public int UnitPriceListID { get; set; }
            public decimal OfferPrice { get; set; }
            public decimal AvailableQuantity { get; set; }
            public decimal Quantity { get; set; }
            public decimal TotalRowCost => OfferPrice * Quantity;
        } 
        public List<CartRow> Cart = new List<CartRow>();
        public ClaimInvestmentsModel(UserManager<ApplicationUser> userManager,
           IPolicyRepository policyRepository, 
           IPolicyBeneficiaryLineRepository policyBeneficiaryLineRepository,
           IPolicyClaimRepository policyClaimRepository,
           IPolicyClaimsLineRepository policyClaimsLineRepository,
           IPolicyUnitsRepository policyUnitsRepository,
           IPolicyUnitsLinesRepository policyUnitsLinesRepository)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _policyBeneficiaryLineRepository = policyBeneficiaryLineRepository;
            _policyClaimRepository = policyClaimRepository;
            _policyClaimsLineRepository = policyClaimsLineRepository;
            _policyUnitsRepository = policyUnitsRepository;
            _policyUnitsLinesRepository = policyUnitsLinesRepository;
        }
        public void OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            ProposerID = id;
            PolicyTypeID = policyTypeid;
            PolicyID = policyid;
            RequestID = reqid;
            Policy = _policyRepository.GetPolicyById(policyid);  
            Cart= LoadCart(policyid);
        }
        private List<CartRow> LoadCart(Guid PolicyID)
        {
            List<CartRow> cart = new List<CartRow>();
            List<PolicyUnit> policyUnits = _policyUnitsRepository.GetByPolicy(PolicyID);
            foreach (PolicyUnit policyUnit in policyUnits)
            {
                CartRow cartRow = new()
                {
                    UnitTrust = policyUnit.UnitTrust,
                    UnitTrustID = policyUnit.UnitTrustID,
                    AvailableQuantity = policyUnit.TotalUnits,
                    UnitPriceListID= policyUnit.UnitPricesListID,
                    OfferPrice = policyUnit.OfferPrice
                };
                cart.Add(cartRow);
            }  
            return cart;
        } 
        public IActionResult OnPostUpdateCart(List<CartRow> cartRows)
        {
            using(TransactionScope TS= new TransactionScope())
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                Cart = LoadCart(PolicyID);
                if (Cart.Count > 0)
                {
                    decimal transactionTotal = 0;
                    for (var i = 0; i < cartRows.Count; i++)
                    {
                        var existingRow = Cart.FirstOrDefault(row => row.UnitTrustID == cartRows[i].UnitTrustID);
                        if (existingRow != null)
                        {
                            decimal qty = cartRows[i].Quantity;
                            if (qty > 0)
                            {
                                decimal offerPrice = Cart[i].OfferPrice;
                                int unitPricesListID = Cart[i].UnitPriceListID;
                                Guid unitTrustID = Cart[i].UnitTrustID;
                                _policyUnitsLinesRepository.DebitPolicyUnits(PolicyID, unitTrustID, unitPricesListID, qty, addedBy);
                                transactionTotal += qty * offerPrice;
                            }                           
                        }
                    }
                    if (transactionTotal > 0)
                    {
                        _policyRepository.DebitInvestmentBalance(PolicyID, transactionTotal);
                    }
                }
                TS.Complete();
            }           
            return Redirect("ClaimDocuments?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
        }
    }
}
