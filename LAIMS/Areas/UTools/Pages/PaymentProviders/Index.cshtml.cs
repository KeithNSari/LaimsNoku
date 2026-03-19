using LAIMS.Interfaces.Banking;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Banking;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Membership;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LAIMS.Areas.UTools.Pages.PaymentProviders
{
	[Authorize(Roles = "Payment Servicing")]
	public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository; 
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IPaymentProviderRepository _paymentProviderRepository;
        private readonly IMemberBankAccountRepository _memberBankAccountRepository;
        private readonly IPremiumCollectionConfigHeaderRepository _premiumCollectionConfigHeaderRepository;
        private readonly IBankRepository _bankRepository;
        public List<SelectListItem> Currencies = new List<SelectListItem>();
        public string SearchPageUrl;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } 
        [BindProperty]
        public bool OrganisationFound { get; private set; }
        public bool ShowAddSection => !OrganisationFound && !string.IsNullOrEmpty(SearchTerm);
        public List<SelectListItem> OrganisationsList = new List<SelectListItem>(); 
        public DataTable EntriesDT { get; set; }
        [BindProperty]
        public int Aggregated { get; set; } = 0;
        public IndexModel(UserManager<ApplicationUser> userManager,
            IMemberRepository memberRepository, 
            ICurrencyRepository currencyRepository,
            IPaymentProviderRepository paymentProviderRepository,
            IMemberBankAccountRepository memberBankAccountRepository,
            IPremiumCollectionConfigHeaderRepository premiumCollectionConfigHeaderRepository,
            IBankRepository bankRepository )
        {
            _userManager = userManager;
            _memberRepository = memberRepository; 
            _currencyRepository = currencyRepository;
            _paymentProviderRepository = paymentProviderRepository;
            _memberBankAccountRepository = memberBankAccountRepository;
            _premiumCollectionConfigHeaderRepository = premiumCollectionConfigHeaderRepository;
            _bankRepository = bankRepository;
        }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        [BindProperty]
        public Guid OrgID { get; set; }
        [BindProperty]
        public string OrgName { get; set; }
        [BindProperty]
        public int PaymentMethodID { get; set; } = 1;
        [BindProperty]
        public string AccountFormat { get; set; }
        [BindProperty]
        public string AccountFormatDescription { get; set; }
        [BindProperty]
        public string StoredProcedureName { get; set; }
        [BindProperty]
        public string InternalAccount { get; set; }
        [BindProperty]
        public int CurrencyID { get; set; }

        public void OnGet()
        {
            EntriesDT = _paymentProviderRepository.GetDebitOrderProviders();
        }
        public IActionResult OnGetSearch(string? search)
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                SearchTerm = search;
                SearchPageUrl = "/UTools/PaymentProviders/Index";
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    LoadCurrencies();
                    List<Member> OrgList = _memberRepository.SearchOrganisations(SearchTerm);
                    ResultsCount = OrgList.Count;
                    if (ResultsCount > 0)
                    { 
                        OrganisationFound = true;
                        LoadOrganisationsSelectList(OrgList);
                    }
                }
                EntriesDT = _paymentProviderRepository.GetDebitOrderProviders();
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        private void LoadOrganisationsSelectList(List<Member> OrgList)
        {
            foreach (Member organisation in OrgList)
            {
                OrganisationsList.Add(new SelectListItem
                {
                    Value = organisation.UID.ToString(),
                    Text = organisation.Name1
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
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                if (!IsValidRegex(AccountFormat)) throw new Exception("Invalid regex for account number format!");
                string AddedBy = _userManager.GetUserId(User).ToString();
                if (OrgID == Guid.Empty)
                {
                    Member NewDOP = new()
                    {
                        Name1 = OrgName,
                        UID = Guid.NewGuid(),
                        IsOrganisation = 1
                    };
                    _memberRepository.AddMember(NewDOP);
                    OrgID = NewDOP.UID;
                }
                int memberID = _memberRepository.GetID(OrgID);
                if (memberID > -1)
                {
                    Bank bank = new()
                    {
                        BankID = memberID,
                        Code = string.Empty,
                        BankAccountNoFormat = AccountFormat,
                        FormatDescription = AccountFormatDescription,
                        AddedBy = AddedBy,
                        AddedOn = DateTime.Now
                    };
                    int bankID = _bankRepository.AddBank(bank); //Adds bank only if it does not exist and returns id, if it exists fetches bank id
                    PaymentProvider paymentProvider = new PaymentProvider()
                    {
                        MemberID = memberID,
                        PaymentMethodID = 1,// 1 is debit order
                        AddedBy = AddedBy
                    };
                    //Adds payment provider only if it does not exist and returns id, if it exists fetches provider id
                    int paymentProviderID = _paymentProviderRepository.AddPaymentProvider(paymentProvider); 
                    PremiumCollectionConfigHeader premiumCollectionConfigHeader = new()
                    {
                        PaymentMethodID = 1,
                        PaymentProviderID = paymentProviderID,
                        InternalBankAccountID = 0,
                        StoredProcedureName = StoredProcedureName.Replace(" ", ""),
                        Aggregated = 0,
                        CurrencyID = CurrencyID,
                        AddedBy = AddedBy
                    };
                    _premiumCollectionConfigHeaderRepository.Create(premiumCollectionConfigHeader);
                } 
                //SearchPageUrl = "/UTools/PaymentProviders/Index";
                //if (!string.IsNullOrEmpty(SearchTerm))
                //{
                //    LoadCurrencies();
                //    List<Member> OrgList = _memberRepository.SearchOrganisations(SearchTerm);
                //    ResultsCount = OrgList.Count;
                //    if (ResultsCount > 0)
                //    { 
                //        OrganisationFound = true;
                //        LoadOrganisationsSelectList(OrgList);
                //    }
                //}
               // EntriesDT = _paymentProviderRepository.GetDebitOrderProviders();
                return Redirect("~/UTools/PaymentProviders/Index");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }

        static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return false;
            try
            {
                // Attempt to create a Regex object with the given pattern
                Regex.Match("", pattern);
                return true; // If no exception is thrown, the regex is valid
            }
            catch (ArgumentException)
            {
                // ArgumentException is thrown when the regex pattern is invalid
                return false;
            }
        }
        public IActionResult OnPostRemove(int entryid)
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                string AddedBy = _userManager.GetUserId(User).ToString();
                DateTime AddedOn = DateTime.Now;
                _premiumCollectionConfigHeaderRepository.ArchivePremiumCollectionConfigHeader(entryid, AddedBy, AddedOn);
                return Redirect("~/UTools/PaymentProviders/Index");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
}
