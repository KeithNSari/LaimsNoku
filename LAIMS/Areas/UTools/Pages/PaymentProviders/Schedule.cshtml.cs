using LAIMS.Interfaces.Banking;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.UTools.Pages.PaymentProviders
{
    [Authorize(Roles = "Payment Servicing")]
    public class ScheduleModel : PageModel
    { 
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IPaymentProviderRepository _paymentProviderRepository;
        private readonly IMemberBankAccountRepository _memberBankAccountRepository;
        private readonly IPremiumCollectionConfigHeaderRepository _premiumCollectionConfigHeaderRepository;
        public List<PaymentProvider> PaymentProviders { get; set; }
        public List<SelectListItem> PaymentProvidersList = new List<SelectListItem>();
        public string SearchPageUrl;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int MemberID { get; set; }
        [BindProperty]
        public int PaymentMethodID { get; set; }
        [BindProperty]
        public DateTime BillingDate { get; set; }
        public DataTable EntriesDT { get; set; }
      
        public ScheduleModel(UserManager<ApplicationUser> userManager,
           IMemberRepository memberRepository,
           ICurrencyRepository currencyRepository,
           IPaymentProviderRepository paymentProviderRepository, 
           IPremiumCollectionConfigHeaderRepository premiumCollectionConfigHeaderRepository)
        {
            _userManager = userManager;
            _memberRepository = memberRepository;
            _currencyRepository = currencyRepository;
            _paymentProviderRepository = paymentProviderRepository; 
            _premiumCollectionConfigHeaderRepository = premiumCollectionConfigHeaderRepository;           
        }
        public void OnGet()
        {
            BillingDate = DateTime.Today;
            PaymentProviders = _paymentProviderRepository.GetAllPaymentProviders();
            LoadPaymentProvidersSelectList();
            EntriesDT = _premiumCollectionConfigHeaderRepository.GetLines();
        }
        public void OnGetSearch(string? search)
        {

        }
        private void LoadPaymentProvidersSelectList()
        {
            foreach (PaymentProvider paymentProvider in PaymentProviders)
            {
                PaymentProvidersList.Add(
                    new SelectListItem
                    {
                        Value = paymentProvider.MemberID.ToString(),
                        Text = paymentProvider.ProviderName
                    }
                   );
            }
        }
        public void OnPost()
        {
            string AddedBy = _userManager.GetUserId(User).ToString();
            _premiumCollectionConfigHeaderRepository.AddLines(MemberID, PaymentMethodID,BillingDate,AddedBy);
            EntriesDT = _premiumCollectionConfigHeaderRepository.GetLines();
            BillingDate = DateTime.Today;
            PaymentProviders = _paymentProviderRepository.GetAllPaymentProviders();
            LoadPaymentProvidersSelectList();
        }
    }
}
