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
namespace LAIMS.Areas.PaymentCollection.Pages
{
    [Authorize(Roles = "Premium Servicing Initiator")]
	public class DebitOrdersIndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymentProviderRepository _paymentProviderRepository;
        public List<SelectListItem> DebitOrderProviderList = new List<SelectListItem>();
        public List<PaymentProvider> DebitOrderProviders { get; set; }
        public DebitOrdersIndexModel(UserManager<ApplicationUser> userManager,
            IPaymentProviderRepository paymentProviderRepository)
        {
            _userManager = userManager;
            _paymentProviderRepository = paymentProviderRepository;
        }
        [BindProperty(SupportsGet = true)]
        public int PCCID { get; set; }
        [BindProperty]
        public DateTime CollectionDate { get; set; }

        public DataTable BatchesDT { get; set; }
        public IActionResult OnGet()
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                CollectionDate = DateTime.Today;
                DebitOrderProviders = _paymentProviderRepository.GetDebitOrderProviderList();
                LoadDebitOrderProvidersSelectList();
                BatchesDT = _paymentProviderRepository.GetLatestDebitOrders();
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
        private void LoadDebitOrderProvidersSelectList()
        {
            foreach (PaymentProvider paymentProvider in DebitOrderProviders)
            {
                DebitOrderProviderList.Add(new SelectListItem
                {
                    Value = paymentProvider.PCCID.ToString(),
                    Text = paymentProvider.ProviderName
                });
            }
        }
        public IActionResult OnPost()
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                long batchID = Convert.ToInt64(DateTime.Now.ToString("yyMMddHHmmss"));
                using (TransactionScope TS = new TransactionScope())
                {
                   int billingDay = CollectionDate.Day;  
                   if(_paymentProviderRepository.BillDebitOrderPremiums(batchID, billingDay, PCCID))
                    {
						_paymentProviderRepository.AddBillID(batchID, billingDay, DateTime.Today);
						_paymentProviderRepository.AddBilledPolicies(batchID);
						_paymentProviderRepository.AddBillHeaders(batchID, billingDay, DateTime.Today);
						_paymentProviderRepository.AddBatch(PCCID, batchID, 1, DateTime.Now);
					}                  
                    TS.Complete();
                }
                return Redirect("DebitOrdersIndex");
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
        public IActionResult OnGetDownLoadCSV(int PCCID, long BatchID)
        {
            string ReturnUrl = Request.Path + Request.QueryString;          
            try
            {
                string procName = _paymentProviderRepository.GetSPName(PCCID);
                string orderName = _paymentProviderRepository.GetDebitOrderName(PCCID);
                DataTable data = new DataTable();
                if (!string.IsNullOrEmpty(procName))
                {
                    data = _paymentProviderRepository.GetDebitOrderData(procName, BatchID);
                }
                if (data == null || data.Rows.Count == 0)
                {
                    return BadRequest("No data found.");
                }
                var csvContent = DataTableToCsvString(data);
                var csvBytes = Encoding.UTF8.GetBytes(csvContent);
                var fileName = orderName + ".csv";
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
