using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.UTools.Pages.ManageCurrencies
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrencyRepository _currencyRepository;
        public EditModel(UserManager<ApplicationUser> userManager, ICurrencyRepository currencyRepository)
        {
            _userManager = userManager;
            _currencyRepository = currencyRepository;
        }
        [BindProperty]
        public Currency Currency { get; set; }
        
        public void OnGet(int id)
        {           
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                Currency = _currencyRepository.GetCurrency(id);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
        }
        public IActionResult OnPost(int id)
        {           
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    
                //    return Page();
                //}
                Currency.ID = id;
                Currency.AddedBy = _userManager.GetUserId(User).ToString();
                Currency.AddedOn = DateTime.Now;
                if(_currencyRepository.CheckExistenceOther(Currency )>0)
                {
                    throw new Exception("An active currency with the same name or short code already exists!");
                }
                _currencyRepository.UpdateCurrency(Currency);
                return Redirect("Create");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            } 
        } 
    }
}
