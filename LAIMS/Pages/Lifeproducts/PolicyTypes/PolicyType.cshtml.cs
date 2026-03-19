using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using LAIMS.Repositories.Lifeproducts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Pages.Lifeproducts.PolicyTypes
{
    public class PolicyTypeModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly ICurrencyRepository _currencyRepository;
        // public IList<PolicyType> PolicyTypes { get; set; } = default!;
        public List<SelectListItem> Currencies = new List<SelectListItem>();
        public PolicyTypeModel(UserManager<IdentityUser> userManager, IPolicyTypeRepository policyTypeRepository, ICurrencyRepository currencyRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
            _currencyRepository = currencyRepository;
        }
        [BindProperty]
        public PolicyType NewPolicyType { get; set; } = default!;
        public void OnGet()
        {
            LoadCurrencies();
            // PolicyTypes = _policyTypeRepository.GetAllPolicyTypes();  
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
            if (!ModelState.IsValid)
            {
                LoadCurrencies();
                // return Page();
            }

            NewPolicyType.ID = Guid.NewGuid();
            NewPolicyType.Current = 1;
            NewPolicyType.AddedBy = _userManager.GetUserId(User).ToString();
            //if (_productRepository.CheckExistence(Product) == 0)
            //{
            _policyTypeRepository.InsertPolicyType(NewPolicyType);
            //}
            //}
            return RedirectToPage("Details");
        }
    }
}
