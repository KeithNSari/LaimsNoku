using LAIMS.Interfaces.BatchJobs;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.UTools.Pages.BatchJobs
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJobsRepository _jobsRepository;
        public IndexModel(UserManager<ApplicationUser> userManager,IJobsRepository jobsRepository)
        {
            _userManager = userManager;
            _jobsRepository = jobsRepository;
        }
        public DataTable JobsDT { get; set; }
        public void OnGet()
        {
            JobsDT = _jobsRepository.GetLatest(); 
        }
    }
}
