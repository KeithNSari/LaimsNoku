using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Security;
using LAIMS.Repositories.Lifeproducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.UTools.Pages.ManageCurrencies
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrencyRepository _currencyRepository;
        public  CreateModel(UserManager<ApplicationUser> userManager,ICurrencyRepository currencyRepository)
        {
            _userManager = userManager;
            _currencyRepository = currencyRepository;
        }
        [BindProperty]
        public Currency Currency { get; set; }
        public DataTable CurrenciesDT { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        public void OnGet()
        {            
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                CurrenciesDT = _currencyRepository.Get();
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
                //if (!ModelState.IsValid)
                //{
                //    
                //    return Page();
                //}
                Currency.AddedBy = _userManager.GetUserId(User).ToString();
                Currency.AddedOn = DateTime.Now;
                if (_currencyRepository.CheckExistence(Currency) == 0)
                {
                    _currencyRepository.InsertCurrency(Currency);
                }
                else
                {
                    throw new Exception("This currency already exists!");
                }
                return Redirect("Create");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            } 
        }
        public IActionResult OnPostRemove(int ID)
        {          
            try
            {
                Currency.ID = ID;
                Currency.AddedBy = _userManager.GetUserId(User).ToString();
                Currency.AddedOn = DateTime.Now;
                _currencyRepository.ArchiveCurrency(Currency);
                return RedirectToPage("Create");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            } 
        }
    }
}
