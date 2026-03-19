using LAIMS.Interfaces.Documents;
using LAIMS.Models.Documents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class pdfdownloaderModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMediaUploadRepository _mediaUploadRepository;
        public pdfdownloaderModel(IWebHostEnvironment webHostEnvironment, IMediaUploadRepository mediaUploadRepository)
        {
            _webHostEnvironment = webHostEnvironment;
            _mediaUploadRepository = mediaUploadRepository;
        }
        public void OnGet()
        {
        }
        //public IActionResult OnGetDownloadPdfFromDatabase(Guid docId)
        //{
        //    MediaUpload mediaUpload = _mediaUploadRepository.GetMediaUploadById(docId);
        //    if (mediaUpload.Data == null || mediaUpload.FileName == null)
        //    {
        //        return NotFound();
        //    }
        //    return File(mediaUpload.Data, "application/pdf", mediaUpload.FileName);
        //}
        public IActionResult OnGetDownloadPdfFromDatabase(Guid docId)
        {
            string pdfFileName = "Mathematics-Syllabus-min.pdf";
            var pdfFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            
            // Combine the folder path with the requested PDF file name
            string pdfFilePath = Path.Combine(pdfFolderPath, pdfFileName);

            // Check if the file exists
            if (System.IO.File.Exists(pdfFilePath))
            {
                // Read the file content
                byte[] fileBytes = System.IO.File.ReadAllBytes(pdfFilePath);

                // Return the file as a response with "application/pdf" content type
                return new FileContentResult(fileBytes, "application/pdf")
                {
                    FileDownloadName = "Invoice_Report.pdf"
                };

            }
            else
            {
                // If the file does not exist, return a HttpNotFound response
                return NotFound();
            }
        }
    }
} 