using LAIMS.Interfaces.Documents;
using LAIMS.Models.Documents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class testpdfdownloadModel : PageModel
    {
        private readonly IMediaUploadRepository _mediaUploadRepository;
        public testpdfdownloadModel(IMediaUploadRepository mediaUploadRepository)
        {
            _mediaUploadRepository = mediaUploadRepository;
        }
        public void OnGet()
        {
        }
        public IActionResult OnGetDownloadPdfFromDatabase(Guid docId)
        {
            MediaUpload mediaUpload = _mediaUploadRepository.GetMediaUploadById(docId);
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
