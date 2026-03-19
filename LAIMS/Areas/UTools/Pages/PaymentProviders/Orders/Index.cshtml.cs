using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Documents;
using LAIMS.Models.Security;
using LAIMS.Repositories.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.UTools.Pages.PaymentProviders.Orders
{
    [Authorize(Roles = "Payment Servicing")]
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyPremiumRepository _policyPremiumRepository;
        private readonly IMediaUploadRepository _mediaUploadRepository;
        public DataTable OrdersDT;
        public IndexModel (UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment, IPolicyPremiumRepository policyPremiumRepository, IMediaUploadRepository mediaUploadRepository)
        {
            _policyPremiumRepository = policyPremiumRepository;
            _mediaUploadRepository = mediaUploadRepository;
        }
        public void OnGet()
        {
            OrdersDT = _policyPremiumRepository.GetLatestOrders();
        }
        public IActionResult OnPost(Guid id)
        {
            MediaUpload mediaUpload = _mediaUploadRepository.GetMediaUploadById(id);
            if (mediaUpload.Data == null || mediaUpload.FileName == null)
            {
                return NotFound();
            }
            var stream = new MemoryStream(mediaUpload.Data);
            var fileStreamResult = new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = mediaUpload.FileName
            };
            HttpContext.Response.RegisterForDispose(stream);
            return fileStreamResult;
        }
    }
}
