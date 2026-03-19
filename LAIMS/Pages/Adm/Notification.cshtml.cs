using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Pages.Adm
{
    [Authorize(Roles = "Admin")]
    public class NotificationModel : PageModel
    {
        [BindProperty]
        public bool Success { get; set; }
        public void OnGet(bool success)
        {
            Success = success;
        }
    }
}
