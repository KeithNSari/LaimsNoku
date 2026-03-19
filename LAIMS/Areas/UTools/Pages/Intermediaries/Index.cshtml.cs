using LAIMS.Interfaces;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.UTools.Pages.Intermediaries
{
    public class IndexModel : PageModel
    {
        public DataTable DT;
        private readonly IWebHostEnvironment _environment;
        private readonly IUploadData _dataUpload;

        public IndexModel(IWebHostEnvironment environment,
            IUploadData dataUpload,
            UserManager<ApplicationUser> userManager
            )
        {
            _environment = environment;
            _dataUpload = dataUpload;
        }
        [BindProperty]
        public IFormFile Upload { get; set; }
        public async Task OnPostAsync()
        {
            if (Upload != null)
            {
                string file = _dataUpload.Documentupload(Upload);
                DT = _dataUpload.ExcelDataTable(file);
            }
        }
        public void OnGet()
        {
        }
    }
}
