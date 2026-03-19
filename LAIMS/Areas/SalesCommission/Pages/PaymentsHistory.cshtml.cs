using LAIMS.Interfaces.Commissions;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;


namespace LAIMS.Areas.SalesCommission.Pages
{
	[Authorize(Roles = "Commission Payment")]
	public class PaymentsHistoryModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyCommissionRepository _policyCommissionRepository;
        public DataTable PaymentsDT;
        public string SearchPageUrl;
        public PaymentsHistoryModel(UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IPolicyCommissionRepository policyCommissionRepository)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _policyCommissionRepository = policyCommissionRepository;
        }
        public void OnGet()
        {
            PaymentsDT = _policyCommissionRepository.GetPayments(); 
        }
    }
}
