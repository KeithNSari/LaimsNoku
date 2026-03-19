using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Lifeproducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.PolicySetUps.Pages.PolicyTypes
{
    [Authorize(Roles = "Admin")]
    public class PTExpensesModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IExpenseTypeRepository _expenseTypeRepository;
        private readonly IPolicyTypesExpenseRepository _policyTypesExpenseRepository;
        private readonly ICurrencyRepository _currencyRepository;
        public List<SelectListItem> PolicyTypesList = new List<SelectListItem>();
        public List<SelectListItem>  ExpenseTypesList = new List<SelectListItem>();
        public List<SelectListItem> Currencies = new List<SelectListItem>();
        public DataTable ComponentsDT = new DataTable();
        [BindProperty]
        public PolicyType MyPolicyType { get; set; } = default!;
        [BindProperty]
        public Guid  PolicyTypeID { get; set; }
		[BindProperty]
		public int StageID { get; set; }
		[BindProperty]
		public int ApplicationTypeID { get; set; }
		[BindProperty]
        public PolicyTypesExpense PolicyTypesExpense { get; set; } = default!;       
        public PTExpensesModel(UserManager<ApplicationUser> userManager, IPolicyTypeRepository policyTypeRepository,
            IExpenseTypeRepository expenseTypeRepository, ICurrencyRepository currencyRepository, IPolicyTypesExpenseRepository policyTypesExpenseRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
            _expenseTypeRepository =expenseTypeRepository;
            _policyTypesExpenseRepository = policyTypesExpenseRepository;
            _currencyRepository = currencyRepository;
        }
        private void LoadPolicyTypesList()
        {
            foreach (PolicyType policyType in _policyTypeRepository.GetAll())
            {
                PolicyTypesList.Add(new SelectListItem
                {
                    Value =policyType.ID.ToString(),
                    Text = policyType.Name 
                });
            }
        }
        private void LoadExpenseTypesList()
        {
            foreach (ExpenseType expenseType in _expenseTypeRepository.GetAllExpenseTypes())
            {
                ExpenseTypesList.Add(new SelectListItem
                {
                    Value = expenseType.ID.ToString(),
                    Text = expenseType.Type
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
        public void OnGet(string? policyID)
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                if (policyID != null)
                {
                    Guid myPolicyID = Guid.Parse(policyID);
                    PolicyTypeID = myPolicyID;
                    ComponentsDT = _policyTypesExpenseRepository.Get(myPolicyID);
                }
                LoadPolicyTypesList();
                LoadExpenseTypesList();
                LoadCurrencies(); 
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
                if (PolicyTypesExpense.Amount < 0)
                {
                    throw new Exception("Quantity/ Amount should be  or equal to 0.");
                }
                if ((PolicyTypesExpense.Ispercentage==1) && (PolicyTypesExpense.Amount>100))
                {
                    throw new Exception("You have selected a %, quantity should be greater than 0 and not exceeding 100.");
                }
                if ((PolicyTypesExpense.Ispercentage == 1) && (PolicyTypesExpense.CurrencyID > 0))
                {
                    throw new Exception("Currency cannot be specified for a percentage!");
                }
                //the only place where we allow end month to be less than start month is when end month is 0 signifying no end month
                if ((PolicyTypesExpense.StartMonth > PolicyTypesExpense.EndMonth) && (PolicyTypesExpense.EndMonth!=0))
                {
                    throw new Exception("Start month cannot be greater than end month!");
                } 
                PolicyTypesExpense.PolicyTypeID = PolicyTypeID;
                PolicyTypesExpense.AddedOn = DateTime.Now;
                PolicyTypesExpense.Current = 1;
                PolicyTypesExpense.AddedBy = _userManager.GetUserId(User).ToString();
                _policyTypesExpenseRepository.Add(PolicyTypesExpense);
                return Redirect("ptexpenses?policyid=" + PolicyTypeID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostRemove(Guid policyid, int id)
        {
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                _policyTypesExpenseRepository.Archive(id, policyid, addedBy);
              // ComponentsDT = _policyTypesExpenseRepository.Get(policyid);
                return Redirect("ptexpenses?policyid=" + policyid);
            }
            catch (Exception ex)
            {
               return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        //public void OnGetHistory(Guid policyID)
        //{
        //    LoadPolicyTypesList();
        //    LoadExpenseTypesList();
        //    LoadCurrencies();
        //    ComponentsDT = _policyTypesExpenseRepository.Get(policyID);
        //}
    }
}
