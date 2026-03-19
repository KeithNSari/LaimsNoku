using LAIMS.Interfaces;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Models.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.UTools.Pages.Rates
{
    [Authorize(Roles = "Payment Servicing")]
    public class AllocationIndexModel : PageModel
    { 
            private readonly IPaymentRepository _paymentRepository;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IWebHostEnvironment _environment;
            private readonly IUploadData _dataUpload;
            public DataTable RatesDT { get; set; }

            [BindProperty]
            public IFormFile Upload { get; set; }
            [BindProperty]
            public DateTime EffectiveDate { get; set; }
            public AllocationIndexModel(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment,
                IPaymentRepository paymentRepository, IUploadData dataUpload)
            {
                _paymentRepository = paymentRepository;
                _userManager = userManager;
                _environment = environment;
                _dataUpload = dataUpload;
            }
            public void OnGet()
            {
                try
                {
                    EffectiveDate = DateTime.Now;
                    RatesDT = _paymentRepository.GetLatestAllocationRates();
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
                            FileContents fileContents = _dataUpload.ExcelData(file);
                            Guid mediaUploadID = Guid.NewGuid();
                            _paymentRepository.UploadRates(fileContents.DataDT, mediaUploadID, EffectiveDate, "Allocation Rates", AddedBy);
                            TS.Complete();
                        }
                    }
                    Response.Redirect("AllocationIndex");
                }
                catch (Exception ex)
                {
                    ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
                }
                finally
                {
                    RatesDT = _paymentRepository.GetLatestAllocationRates();
                }
            } 
    }
}
