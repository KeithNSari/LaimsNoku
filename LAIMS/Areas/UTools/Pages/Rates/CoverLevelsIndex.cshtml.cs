using LAIMS.Interfaces;
using LAIMS.Interfaces.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.UTools.Pages.Rates
{
    [Authorize(Roles = "Payment Servicing")]
    public class CoverLevelsIndexModel : PageModel
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUploadData _dataUpload;

        public CoverLevelsIndexModel(IPaymentRepository paymentRepository, IUploadData dataUpload)
        {
            _paymentRepository = paymentRepository;
            _dataUpload = dataUpload;
        }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public DataTable CoverLevelsDT { get; set; }

        public void OnGet()
        {
            try
            {
                CoverLevelsDT = _paymentRepository.GetCoverLevels();
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
                        throw new Exception("This file extension is not allowed!");
                    }

                    using (TransactionScope TS = new TransactionScope())
                    {
                        string file = _dataUpload.Documentupload(Upload);
                        var fileContents = _dataUpload.ExcelData(file);
                        _paymentRepository.UploadCoverLevels(fileContents.DataDT);
                        TS.Complete();
                    }
                }

                Response.Redirect("CoverLevelsIndex");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
            finally
            {
                CoverLevelsDT = _paymentRepository.GetCoverLevels();
            }
        }
    }
}
