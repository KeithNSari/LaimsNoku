using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class test7Model : PageModel
    {
        [BindProperty]
        public string myid { get; set; }
        public void OnGet(string id)
        {
            myid = id;
        }
    }
}
