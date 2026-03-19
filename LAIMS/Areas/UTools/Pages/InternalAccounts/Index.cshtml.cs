using LAIMS.Interfaces.Banking;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Utilities;
using LAIMS.Models.Banking;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Security;
using LAIMS.Models.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.UTools.Pages.InternalAccounts
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberBankAccountRepository _memberBankAccountRepository;
        private readonly ICurrencyRepository _currencyRepository;
        public List<SelectListItem> Currencies = new List<SelectListItem>();
        [BindProperty]
        public MemberBankAccount MemberBankAccount { get; set; }
        [BindProperty]
        public DataTable AccountsDT { get; set; }
        [BindProperty]
        public string BankAccountConfirm {get;set;}
        public IndexModel(UserManager<ApplicationUser> userManager, IMemberBankAccountRepository memberBankAccountRepository,
            ICurrencyRepository currencyRepository)
        {
            _userManager = userManager; 
            _memberBankAccountRepository = memberBankAccountRepository;
            _currencyRepository = currencyRepository;
        }
        public void OnGet()
        {
            try
            { 
                LoadCurrencies();
                AccountsDT = _memberBankAccountRepository.GetInternalAccounts();
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
        public void OnPost()
        {
            try
            {
                if (MemberBankAccount.BankAccountNo != BankAccountConfirm) throw new Exception("Account No and confirmation value should match!");
                    
                MemberBankAccount.BankID = 0;
                MemberBankAccount.AddedOn = DateTime.Now;
                MemberBankAccount.AddedBy= _userManager.GetUserId(User).ToString();
                MemberBankAccount.IsInternalAccount = 1;
                _memberBankAccountRepository.AddMemberBankAccount(MemberBankAccount);               
                AccountsDT = _memberBankAccountRepository.GetInternalAccounts();
                Response.Redirect("/UTools/InternalAccounts/Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
            finally
            {
                LoadCurrencies();
            }
        }
        public void OnPostArchive(int id)
        {
            try
            {
                string archivedBy= _userManager.GetUserId(User).ToString();
                _memberBankAccountRepository.ArchiveMemberAccount(id, archivedBy, DateTime.Now);  
                Response.Redirect("/UTools/InternalAccounts/Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
            finally
            {
                LoadCurrencies();
            }
        }

    }
}
