using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.PaymentCollection.Pages
{
    public class BillingNotificationsModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBillingMessageRepository _billingMessageRepository;
        public BillingNotificationsModel(UserManager<ApplicationUser> userManager,IBillingMessageRepository billingMessageRepository,
          IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _billingMessageRepository = billingMessageRepository;

        }
        public string SearchPageUrl;
        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; } 
        public DataTable MessagesDT { get; set; }
        public void OnGet()
        {
            MessagesDT = _billingMessageRepository.GetLatestBillingMessages();
        }
        public void OnGetSearch(string? search)
        {

        }
    }
}
