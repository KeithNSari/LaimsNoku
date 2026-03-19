using LAIMS.Models.Security;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.Policies;
using LAIMS.Repositories.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<IndexModel> _logger;
        private readonly IPolicyRepository _policyRepository;
        public DataTable LatestPoliciesDT { get; set; }
        public IndexModel(ILogger<IndexModel> logger, UserManager<ApplicationUser> userManager,IPolicyRepository policyRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _policyRepository = policyRepository;
        }
        [BindProperty]
        public ApplicationStats ApplicationStats { get; set; }
        public IActionResult OnGet()
        {		 
			string AddedBy = _userManager.GetUserId(User).ToString();
            ApplicationStats = _policyRepository.GetApplicationStats(AddedBy);
            LatestPoliciesDT = _policyRepository.GetLatestApplications(AddedBy);
            Response.Headers.Append("Content-Type", "text/html");
            return Page();
        } 
	}
}