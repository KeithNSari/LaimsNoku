using System.Data;
using LAIMS.Interfaces;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LAIMS.Areas.UTools.Pages.PaymentProviders.Orders
{
    [Authorize(Roles = "Premium Servicing Initiator")]
    public class DebitOrderDetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IPaymentProviderRepository _paymentProviderRepository;
        private readonly IPaymentRepository _paymentRepository;   
        private readonly IUploadData _dataUpload;
        public DebitOrderDetailsModel(UserManager<ApplicationUser> userManager
            ,IWebHostEnvironment environment,
            IPaymentProviderRepository paymentProviderRepository,
            IPaymentRepository paymentRepository, IUploadData dataUpload)
        {           
            _userManager = userManager;
            _paymentProviderRepository = paymentProviderRepository;
            _paymentRepository = paymentRepository;
            _userManager = userManager;
            _environment = environment;
            _dataUpload = dataUpload;
        }
        public List<PaymentProvider> DebitOrderProviders { get; set; }
        public List<SelectListItem> DebitOrderProviderList = new List<SelectListItem>();
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public int PaymentProviderID { get; set; }
        public DataTable BatchesDT { get; set; }

        [BindProperty]
        public IFormFile Upload { get; set; }
        private void LoadDebitOrderProvidersSelectList()
        {
            foreach (PaymentProvider paymentProvider in DebitOrderProviders)
            {
                DebitOrderProviderList.Add(new SelectListItem
                {
                    Value = paymentProvider.ID.ToString(),
                    Text = paymentProvider.ProviderName
                });
            }
        }
        public IActionResult OnGet()
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                DebitOrderProviders = _paymentProviderRepository.GetPaymentProviders(1);
                LoadDebitOrderProvidersSelectList();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new
                {
                    errorMessage = "An error occurred during processing. " + ex.Message,
                    returnUrl = ReturnUrl
                });
            }
            return Page();

        }
        public IActionResult OnPost()
        {
            try
            {
                if (Upload != null)
                {

                    string AddedBy = _userManager.GetUserId(User).ToString();
                    string file = _dataUpload.Documentupload(Upload);
                    long batchID = Convert.ToInt64(DateTime.Now.ToString("yyMMddmmss"));
                    _paymentRepository.UploadDebitOrders(file, batchID,PaymentProviderID, AddedBy); 
                    Response.Redirect("/PaymentCollection/PolicySuspense?handler=batch&batchID=" + batchID);
                }               
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new
                {
                    errorMessage = "An error occurred during processing. " + ex.Message,
                    returnUrl = ReturnUrl
                });
            }
            return Page();
        }
    }
}
