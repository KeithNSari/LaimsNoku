using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.PaymentCollection.Pages
{
    [Authorize(Roles = "Suspense Reconcilliation")]
    public class ReversalHistoryModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReversalHeaderRepository _reversalHeaderRepository; 
        public DataTable ReversalsDT;
        public string SearchPageUrl;
        public ReversalHistoryModel(UserManager<ApplicationUser> userManager, 
            IWebHostEnvironment webHostEnvironment, 
            IReversalHeaderRepository reversalHeaderRepository)
        {
            _userManager = userManager; 
            _webHostEnvironment = webHostEnvironment;
            _reversalHeaderRepository = reversalHeaderRepository;
        }
        public void OnGet()
        {
            ReversalsDT = _reversalHeaderRepository.GetReversalHistory();
        }
    }
}
