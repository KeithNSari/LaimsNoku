using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LAIMS.Areas.PolicySetUps.Pages.PolicyTypes
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly ICurrencyRepository _currencyRepository;
        public List<SelectListItem> Currencies = new List<SelectListItem>();
        public CreateModel(UserManager<ApplicationUser> userManager, IPolicyTypeRepository policyTypeRepository, ICurrencyRepository currencyRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
            _currencyRepository = currencyRepository;
        }
        [BindProperty]
        public PolicyType NewPolicyType { get; set; } = default!;
        public void OnGet()
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                LoadCurrencies();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
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
                if (!ModelState.IsValid)
                {
                    LoadCurrencies();
                    // return Page();
                }
                if (NewPolicyType.MinimumTerm > NewPolicyType.MaximumTerm)
                {
                    throw new Exception("Minimum Term must be less or equal to Maximum Term");
                }
                if(NewPolicyType.LifeAssuredMinAge>NewPolicyType.LifeAssuredMaxAge)
                {
                    throw new Exception("Life Assured Minimum Age must be less or equal to Life Assured Maximum Age");
                }
                if (NewPolicyType.ProposerMinAge > NewPolicyType.ProposerMaxAge)
                {
                    throw new Exception("Proposer Minimum Age must be less or equal to Proposer Maximum Age");
                }
                if (_policyTypeRepository.CheckExistence(NewPolicyType) == 0)
                {
                    NewPolicyType.ID = Guid.NewGuid();
                    NewPolicyType.AddedOn = DateTime.Now;
                    NewPolicyType.Current = 1;
                    NewPolicyType.AddedBy = _userManager.GetUserId(User).ToString();
                    _policyTypeRepository.InsertPolicyType(NewPolicyType);
                }
                else
                {
                    throw new Exception("A record with this name already exists!");
                }
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
}
