using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.PolicySetUps.Pages.Questionnaires
{
    [Authorize(Roles = "Admin")]
    public class PreviewResponsesModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
