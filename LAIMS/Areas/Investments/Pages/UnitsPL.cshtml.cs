using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using LAIMS.Repositories.Lifeproducts;
using LAIMS.Interfaces.Investments;
using LAIMS.Models.Investments;
using System.Data;
using LAIMS.Models.Security;

namespace LAIMS.Areas.Investments.Pages
{
	[Authorize(Roles = "Set Units Price List")]
	public class UnitsPLModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IUnitTrustRepository _unitTrustRepository;
        private readonly IUnitsPricesListRepository _unitsPricesListRepository;
        public List<SelectListItem> Currencies = new List<SelectListItem>();
        public List<SelectListItem> UnitTrusts = new List<SelectListItem>();
        [BindProperty]
        public Guid UnitTrustID { get; set; }
        [BindProperty]
        public UnitsPricesList UnitsPricesList { get; set; }
        public DataTable ComponentsDT { get; set; }
        public UnitsPLModel(UserManager<ApplicationUser> userManager, ICurrencyRepository currencyRepository,
            IUnitTrustRepository unitTrustRepository,IUnitsPricesListRepository unitsPricesListRepository)
        {
            _userManager = userManager; 
            _currencyRepository = currencyRepository;
            _unitsPricesListRepository=unitsPricesListRepository;
            _unitTrustRepository = unitTrustRepository;
        }
        public void OnGet(string? trustID)
        {            
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                if (trustID != null)
                {
                    Guid TrustID = Guid.Parse(trustID);                   
                    UnitTrustID = TrustID;
                    ComponentsDT = _unitsPricesListRepository.GetPriceHistory(UnitTrustID);  
                }
                else
                {
                    ComponentsDT = _unitsPricesListRepository.GetLatest();
                }
                LoadUnitTrusts();
                LoadCurrencies();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
        } 
        private void LoadUnitTrusts()
        {
            foreach (UnitTrust unitTrust in _unitTrustRepository.ReadAll())
            {
                UnitTrusts.Add(new SelectListItem
                {
                    Value = unitTrust.ID.ToString(),
                    Text = unitTrust.UnitTrustName
                });
            }
        }
        private void LoadCurrencies()
        {
            foreach (Currency currency in _currencyRepository.GetAllCurrencies())
            {
                Currencies.Add(new SelectListItem
                {
                    Value = currency.ID.ToString(),
                    Text = currency.CurrencyName
                });
            }
        }
        public IActionResult OnPost()
        {
            try
            {
                ViewData["ErrorMessage"] = "";
                if (UnitsPricesList.EffectiveDate<DateTime.Now.Date)
                {
                    throw new Exception("Effective date cannot be in the past");                    
                }
                UnitsPricesList.AddedOn = DateTime.Now;
                UnitsPricesList.AddedBy= _userManager.GetUserId(User).ToString();
                UnitsPricesList.UnitTrustID = UnitTrustID;
                _unitsPricesListRepository.Create(UnitsPricesList);
                return Redirect("unitspl?trustid=" + UnitTrustID);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
            ComponentsDT = _unitsPricesListRepository.GetPriceHistory(UnitTrustID);
            LoadUnitTrusts();
            LoadCurrencies();
            return Page();
        }
    }
}
