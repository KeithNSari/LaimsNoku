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
    public class FundConfigsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IUnitTrustRepository _unitTrustRepository;
        private readonly IUnitTrustsLineRepository _unitTrustsLineRepository;
        private readonly IUnitsPricesListRepository _unitsPricesListRepository;
        public List<SelectListItem> Currencies = new List<SelectListItem>();
        public List<SelectListItem> UnitTrusts = new List<SelectListItem>();
        [BindProperty]
        public Guid UnitTrustID { get; set; }
        [BindProperty]
        public UnitTrustsLine UnitTrustsLine { get; set; }
        public DataTable ComponentsDT { get; set; }
        public FundConfigsModel(UserManager<ApplicationUser> userManager, ICurrencyRepository currencyRepository,
            IUnitTrustRepository unitTrustRepository, IUnitTrustsLineRepository unitTrustsLineRepository,
            IUnitsPricesListRepository unitsPricesListRepository)
        {
            _userManager = userManager;
            _currencyRepository = currencyRepository;
            _unitsPricesListRepository = unitsPricesListRepository;
            _unitTrustRepository = unitTrustRepository;
            _unitTrustsLineRepository = unitTrustsLineRepository;
        }
        public void OnGet()
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                ComponentsDT = _unitTrustsLineRepository.GetLatest();
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
                if ( 
                    UnitTrustsLine.EffectiveDate < DateTime.Now.Date)
                {
                    throw new Exception("Effective date cannot be in the past");
                }
                UnitTrustsLine.AddedOn = DateTime.Now;
                UnitTrustsLine.AddedBy = _userManager.GetUserId(User).ToString();
                UnitTrustsLine.HeaderID = UnitTrustID;
                _unitTrustsLineRepository.Add(UnitTrustsLine);
                return Redirect("fundconfigs");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
            ComponentsDT = _unitTrustsLineRepository.GetLatest();
            LoadUnitTrusts();
            LoadCurrencies();
            return Page();
        }
    }
}