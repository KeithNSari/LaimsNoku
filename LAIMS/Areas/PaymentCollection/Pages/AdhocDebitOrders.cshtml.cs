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
	[Authorize(Roles = "Premium Servicing Initiator, Payment Servicing")]
	public class AdhocDebitOrdersModel : PageModel
    {
        public DateTime CollectionDate { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymentProviderRepository _paymentProviderRepository;
        public List<SelectListItem> DebitOrderProviderList = new List<SelectListItem>();
        public List<PaymentProvider> DebitOrderProviders { get; set; }
        public AdhocDebitOrdersModel(UserManager<ApplicationUser> userManager,
            IPaymentProviderRepository paymentProviderRepository)
        {
            _userManager = userManager;
            _paymentProviderRepository = paymentProviderRepository;
        }
        [BindProperty(SupportsGet = true)]
        public int PaymentProviderID { get; set; } 
        public DataTable BatchesDT { get; set; }
        public IActionResult OnGet()
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                DebitOrderProviders = _paymentProviderRepository.GetPaymentProviders(1);
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
                    Value = paymentProvider.ID.ToString(),
                    Text = paymentProvider.ProviderName
                });
            }
        }         
        public IActionResult OnPost()
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                long batchID = Convert.ToInt64(DateTime.Now.ToString("yyMMddmmss"));
                using (TransactionScope TS = new TransactionScope())
                {
                    int billingDay = DateTime.Today.Day;
                    int PCCID = _paymentProviderRepository.GetFirstPCCID(PaymentProviderID);
                    _paymentProviderRepository.BillDebitOrderPremiums(batchID, PaymentProviderID,billingDay,PCCID);
                    _paymentProviderRepository.AddBillID(batchID, billingDay, DateTime.Today);
                    _paymentProviderRepository.AddBilledPolicies(batchID);
                    _paymentProviderRepository.AddBillHeaders(batchID, billingDay, DateTime.Today);
                    _paymentProviderRepository.AddBatch(PCCID, batchID,1, DateTime.Now);
                    TS.Complete();
                }
                return Redirect("StopOrderBilling");
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
        public IActionResult OnGetDownLoadCSV(int PCCID)
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                string procName = _paymentProviderRepository.GetSPName(PCCID);
                DataTable data = new DataTable();
                if (!string.IsNullOrEmpty(procName))
                {
                    data = _paymentProviderRepository.GetStopOrderData(procName);
                }
                if (data == null || data.Rows.Count == 0)
                {
                    return BadRequest("No data found.");
                }


                var csvContent = DataTableToCsvString(data);
                var csvBytes = Encoding.UTF8.GetBytes(csvContent);
                var fileName = procName + ".csv";
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
                sb.Append("\"" + column.ColumnName.Replace("\"", "\"\"") + "\",");
            }
            sb.AppendLine();

            // Write data rows
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    sb.Append("\"" + item.ToString().Replace("\"", "\"\"") + "\",");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
        private string DataTableToFile(DataTable dataTable, string delimiter)
        {
            var sb = new StringBuilder();

            // Determine the maximum length of each column's values
            int[] maxLengths = new int[dataTable.Columns.Count];
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                maxLengths[i] = Math.Max(
                    dataTable.Columns[i].ColumnName.Length,
                    dataTable.AsEnumerable()
                        .Max(row => (row[i]?.ToString() ?? "").Length)
                ); // Get the maximum length of values in the column, including column name
            }

            // Write header row with padded values
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                sb.Append(PadRight(dataTable.Columns[i].ColumnName, maxLengths[i]) + delimiter);
            }
            sb.AppendLine();

            // Write data rows with padded values
            foreach (DataRow row in dataTable.Rows)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    sb.Append(PadRight((row[i]?.ToString() ?? ""), maxLengths[i]) + delimiter);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        // Helper method to pad a string with spaces to a specified length
        private string PadRight(string input, int length)
        {
            return input.PadRight(length);
        }
    }
}
