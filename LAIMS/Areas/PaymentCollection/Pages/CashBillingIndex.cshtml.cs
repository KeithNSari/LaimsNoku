using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Transactions;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using LAIMS.Models.Security;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
namespace LAIMS.Areas.PaymentCollection.Pages
{
	[Authorize(Roles = "Premium Servicing Initiator")]
	public class CashBillingIndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymentProviderRepository _paymentProviderRepository;
        private readonly ICurrencyRepository _currencyRepository;      
        public CashBillingIndexModel(UserManager<ApplicationUser> userManager,
         IPaymentProviderRepository paymentProviderRepository, ICurrencyRepository currencyRepository)
        {
            _userManager = userManager;
            _paymentProviderRepository = paymentProviderRepository;
            _currencyRepository = currencyRepository;
        }
        public List<SelectListItem> Currencies = new List<SelectListItem>();
        [BindProperty]
        public DateTime CollectionDate { get; set; }
        [BindProperty]
        public int CurrencyID { get; set; }

        public DataTable BatchesDT { get; set; }
        public IActionResult OnGet()
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                CollectionDate = DateTime.Today;
                LoadCurrencies();
                BatchesDT = _paymentProviderRepository.GetLatestBillingBatches(3); 
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new
                {
                    errorMessage = "An error occurred during processing. " + ex.Message,
                    returnUrl = ReturnUrl
                });
            }
        }
        private void LoadCurrencies()
        {
            foreach (Currency currency in _currencyRepository.GetAllCurrencies())
            {
                Currencies.Add(new SelectListItem
                {
                    Value = currency.ID.ToString(),
                    Text = currency.CurrencyName
                });
            }
        }
        public IActionResult OnPost()
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                long batchID = Convert.ToInt64(DateTime.Now.ToString("yyMMddHHmmss"));
                int PCCID = 0;//No PCCID is required for cash payments
                using (TransactionScope TS = new TransactionScope())
                {
                    int billingDay = CollectionDate.Day;
                    if (_paymentProviderRepository.BillDirectPaymentPremiums(batchID, billingDay, CurrencyID))
                    {
                        _paymentProviderRepository.AddBillID(batchID, billingDay, DateTime.Today);
                        _paymentProviderRepository.AddBilledPolicies(batchID);
                        _paymentProviderRepository.AddBillHeaders(batchID, billingDay, DateTime.Today);
                        _paymentProviderRepository.AddBatch(PCCID, batchID, 3, DateTime.Now);
                    }
                    TS.Complete();
                }
                return Redirect("CashBillingIndex");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new
                {
                    errorMessage = "An error occurred during processing. " + ex.Message,
                    returnUrl = ReturnUrl
                });
            }
        }
        public IActionResult OnGetDownLoadCSV(long BatchID)
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                DataTable data = _paymentProviderRepository.GetBillingBatchData(BatchID);
                if (data == null || data.Rows.Count == 0)
                {
                    return BadRequest("No data found.");
                }
                var csvContent = DataTableToCsvString(data);
                var csvBytes = Encoding.UTF8.GetBytes(csvContent);
                var fileName = BatchID + ".csv";
                // Returning the CSV file as a download
                return File(csvBytes, "text/csv", fileName);

            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new
                {
                    errorMessage = "An error occurred during processing. " + ex.Message,
                    returnUrl = ReturnUrl
                });
            }
        }
        private string DataTableToCsvString(DataTable dataTable)
        {
            var sb = new StringBuilder();

            // Write header row
            foreach (DataColumn column in dataTable.Columns)
            {
                string columnName = string.IsNullOrEmpty(column.ColumnName) ? "" : column.ColumnName;
                sb.Append("\"" + columnName.Replace("\"", "\"\"") + "\",");
            }
            sb.AppendLine();

            // Write data rows
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    string cellValue = "\"" + item.ToString().Replace("\"", "\"\"") + "\"";
                    sb.Append(cellValue + ",");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
