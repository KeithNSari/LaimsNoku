using LAIMS.Interfaces.Utilities;
using LAIMS.Models.Security;
using LAIMS.Models.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.UTools.Pages.Holidays
{
	[Authorize(Roles = "Billing History")]
	public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHolidayRepository _holidayRepository;
        [BindProperty]
        public Holiday Holiday { get; set; }
        [BindProperty]
        public List<Holiday> HolidayList { get; set; }  
        public IndexModel(UserManager<ApplicationUser> userManager,IHolidayRepository holidayRepository)
        {
           _holidayRepository=holidayRepository;
        }
        public void OnGet()
        {
            HolidayList = _holidayRepository.GetAllHolidays(DateTime.Today.Year);
        }
        public void OnPost()
        {
            Holiday.Year = DateTime.Today.Year;
            _holidayRepository.Create(Holiday);
            HolidayList = _holidayRepository.GetAllHolidays(DateTime.Today.Year);
        }
    }
}
