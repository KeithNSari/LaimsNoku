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
    public class EditModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly ICurrencyRepository _currencyRepository;
        public List<SelectListItem> Currencies = new List<SelectListItem>();
        public EditModel(UserManager<ApplicationUser> userManager, IPolicyTypeRepository policyTypeRepository, ICurrencyRepository currencyRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
            _currencyRepository = currencyRepository;
        }
        [BindProperty]
        public PolicyType MyPolicyType { get; set; } = default!;
        public void OnGet(Guid id)
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                LoadCurrencies();
                MyPolicyType = _policyTypeRepository.GetPolicyTypeFull(id);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }           
        }
        public IActionResult OnPost(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // return Page();
                }
                if (MyPolicyType.MinimumTerm > MyPolicyType.MaximumTerm)
                {
                    throw new Exception("Minimum Term must be less or equal to Maximum Term");
                }
                if (MyPolicyType.LifeAssuredMinAge > MyPolicyType.LifeAssuredMaxAge)
                {
                    throw new Exception("Life Assured Minimum Age must be less or equal to Life Assured Maximum Age");
                }
                if (MyPolicyType.ProposerMinAge > MyPolicyType.ProposerMaxAge)
                {
                    throw new Exception("Proposer Minimum Age must be less or equal to Proposer Maximum Age");
                }
                MyPolicyType.ID = id;
                if (_policyTypeRepository.CheckExistenceOther(MyPolicyType) > 0)
                {
                    throw new Exception("Another active policy with this name already exists!");
                }
                if (_policyTypeRepository.CheckExistence(MyPolicyType) > 0)
                {
                    _policyTypeRepository.UpdatePolicyType(MyPolicyType);
                    RedirectToPage("Details", id);
                }
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
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
    }
}
