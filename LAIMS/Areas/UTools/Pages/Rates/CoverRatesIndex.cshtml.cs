using LAIMS.Interfaces;
using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using LAIMS.Models.Utilities;
using LAIMS.Models.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Transactions;
using LAIMS.Models.Security;

namespace LAIMS.Areas.UTools.Pages.Rates
{
    [Authorize(Roles = "Payment Servicing")]
    public class CoverRatesIndexModel : PageModel
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IUploadData _dataUpload;
        private readonly IMediaUploadRepository _mediaUploadRepository;
        public DataTable RatesDT { get; set; }

        [BindProperty]
        public IFormFile Upload { get; set; }
        [BindProperty]
        public DateTime EffectiveDate { get; set; }
        public CoverRatesIndexModel(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment,
            IPaymentRepository paymentRepository, IMediaUploadRepository mediaUploadRepository, IUploadData dataUpload)
        {
            _paymentRepository = paymentRepository;
            _userManager = userManager;
            _environment = environment;
            _dataUpload = dataUpload;
            _mediaUploadRepository = mediaUploadRepository;
        }
        public void OnGet()
        {
            try
            {
                EffectiveDate = DateTime.Now;
                RatesDT = _paymentRepository.GetLatestCoverRates();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
        }
        public void OnPost()
        {
            try
            {
                if (Upload != null)
                {
                    var extension = Path.GetExtension(Upload.FileName);
                    if (!extension.Contains(".xls"))
                    {
                        throw new Exception($"This file extension is not allowed!");
                    }
                    if (EffectiveDate.Date < DateTime.Today.Date)
                    {
                        throw new Exception("Effective date cannot be in the past!");
                    }
                    using (TransactionScope TS = new TransactionScope())
                    {
                        string AddedBy = _userManager.GetUserId(User).ToString();
                        string file = _dataUpload.Documentupload(Upload);                        
                        Guid mediaUploadID = Guid.NewGuid();
                        var uniqueFileName = Upload.FileName + "_" + mediaUploadID.ToString(); 
                        string contentType = Upload.ContentType;
                         var mediaUpload = new MediaUpload
                        {
                            ID = mediaUploadID,
                            MemberUID = Guid.Empty,
                            DocumentsID = Guid.Parse("C0F2CFF6-D434-4A4D-85CF-62C089479365"), // Cover rate
                            FileName = uniqueFileName,
                            ContentType = contentType,
                            Data = System.IO.File.ReadAllBytes(file),
                            AddedOn = DateTime.Now,
                            AddedBy = _userManager.GetUserId(User).ToString()
                        };
                        _mediaUploadRepository.SaveMediaUpload(mediaUpload);
                        FileContents fileContents = _dataUpload.ExcelData(file);
                        _paymentRepository.UploadRates(fileContents.DataDT, mediaUploadID, EffectiveDate, "Cover Rates", AddedBy);
                        TS.Complete();
                    }
                }
                Response.Redirect("CoverRatesIndex");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
            finally
            {
                RatesDT = _paymentRepository.GetLatestCoverRates(); 
            }
        }
        public IActionResult OnPostDownloadFile(Guid docId)
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
