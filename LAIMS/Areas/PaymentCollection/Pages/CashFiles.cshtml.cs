using LAIMS.Interfaces;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.PaymentCollection.Pages
{
	[Authorize(Roles = "Premium Servicing Initiator")]
	public class CashFilesModel : PageModel
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IUploadData _dataUpload;
        public DataTable CashBatchesDT { get; set; }

        [BindProperty]
        public IFormFile Upload { get; set; }
        public CashFilesModel(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment, 
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
                CashBatchesDT = _paymentRepository.GetLatestCashBatchHeaders();
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

                    string AddedBy = _userManager.GetUserId(User).ToString();
                    string file = _dataUpload.Documentupload(Upload);
                    long batchID = Convert.ToInt64(DateTime.Now.ToString("yyMMddmmss"));
                    _paymentRepository.UploadStatement(file, batchID, AddedBy);
                    CashBatchesDT = _paymentRepository.GetLatestCashBatchHeaders();
                }
                Response.Redirect("SystemSuspense");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
            //finally
            //{
            //    CashBatchesDT = _paymentRepository.GetLatestCashBatchHeaders();
            //}
        } 
    }
}
