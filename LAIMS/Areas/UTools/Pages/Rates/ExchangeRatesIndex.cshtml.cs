using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;

namespace LAIMS.Areas.UTools.Pages.Rates
{
	[Authorize(Roles = "New Business Approver")]
	public class ExchangeRatesIndexModel : PageModel
    {
		[BindProperty(SupportsGet = true)]
		public string ReturnUrl { get; set; }
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ICurrencyRepository _currencyRepository;
        private readonly IExchangeRateRepository _exchangeRateRepository;

		public List<SelectListItem> Currencies = new List<SelectListItem>();
		public DataTable LatestExchangeRates { get; set; }

		public ExchangeRatesIndexModel (UserManager<ApplicationUser> userManager, ICurrencyRepository currencyRepository
            , IExchangeRateRepository exchangeRateRepository)
        {
            _currencyRepository = currencyRepository;
            _exchangeRateRepository = exchangeRateRepository;
            _userManager=userManager;
        }
		[BindProperty]
		public ExchangeRate ExchangeRate { get; set; }
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
		private void LoadLatestExchangeRates()
		{
			LatestExchangeRates = _exchangeRateRepository.GetLatestExchangeRates();
		}
		public void OnGet()
        {
			try
			{
				ReturnUrl = Request.Path + Request.QueryString; 
				LoadCurrencies();
				LoadLatestExchangeRates();
			}
			catch (Exception ex)
			{
				ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
			}
		}
		public IActionResult OnPost()
		{
			try
			{
				if (ExchangeRate.BaseCurrency == ExchangeRate.OtherCurrency) throw new Exception("Base and other currency must be different!");
				ExchangeRate.AddedBy = User.Identity.Name;
				ExchangeRate.AddedOn = DateTime.UtcNow;
				_exchangeRateRepository.SaveExchangeRate(ExchangeRate);
				return RedirectToPage();
			}
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
			}
		}
    }
}
