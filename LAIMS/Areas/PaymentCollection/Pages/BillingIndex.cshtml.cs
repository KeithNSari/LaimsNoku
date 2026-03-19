using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.PaymentCollection.Pages
{
	[Authorize(Roles = "Premium Servicing Initiator")]
	public class BillingIndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBillingHeaderRepository _billingHeaderRepository;
        public BillingIndexModel(UserManager<ApplicationUser> userManager, IBillingHeaderRepository billingHeaderRepository)
        {
            _userManager = userManager;
            _billingHeaderRepository = billingHeaderRepository; 
        }

        public string SearchPageUrl;
        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        public DataTable BatchesDT { get; set; }
        public void OnGet()
        {
            BatchesDT = _billingHeaderRepository.GetBatches(5000);//Status 5000 =started
        }
        public void OnGetSearch(string? search)
        {

        }
    }
}
