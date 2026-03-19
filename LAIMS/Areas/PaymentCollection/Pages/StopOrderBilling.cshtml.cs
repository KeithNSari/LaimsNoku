using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Transactions;
using System.Text;
using CsvHelper;
using LAIMS.Models.Security;

namespace LAIMS.Areas.PaymentCollection.Pages
{
	[Authorize(Roles = "Premium Servicing Initiator, Payment Servicing")]
	public class StopOrderBillingModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymentProviderRepository _paymentProviderRepository;
        public List<SelectListItem> StopOrderProviderList = new List<SelectListItem>();
        public List<PaymentProvider> StopOrderProviders { get; set; }
        public StopOrderBillingModel(UserManager<ApplicationUser> userManager, 
            IPaymentProviderRepository paymentProviderRepository)
        {
            _userManager = userManager;
            _paymentProviderRepository = paymentProviderRepository;
        }
        [BindProperty(SupportsGet = true)] 
        public int PaymentProviderID { get; set; }
        [BindProperty]
        public int PCCID { get; set; }
        public DataTable BatchesDT { get; set; }
		[BindProperty]
		public DateTime CollectionDate { get; set; }
		public IActionResult OnGet()
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                CollectionDate = DateTime.Today;
                StopOrderProviders = _paymentProviderRepository.GetPaymentProviders(2);
                LoadStopOrderProvidersSelectList();
                BatchesDT = _paymentProviderRepository.GetLatestStopOrders();
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
        private void LoadStopOrderProvidersSelectList()
        {
            foreach (PaymentProvider paymentProvider in StopOrderProviders)
            {
                StopOrderProviderList.Add(new SelectListItem
                {
                    Value = paymentProvider.ID.ToString(),
                    Text = paymentProvider.ProviderName
                });
            }
        }
        public JsonResult OnGetStopOrdersByProvider()
        {
            List<StopOrder> stopOrders = _paymentProviderRepository.GetStopOrdersByProvider(PaymentProviderID);
            return new JsonResult(stopOrders);
        }
        public IActionResult OnPost()
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            long batchID = Convert.ToInt64(DateTime.Now.ToString("yyMMddHHmmss"));
            try
            {
                using (TransactionScope TS = new TransactionScope())
                { 
                    int billingDay = CollectionDate.Day;
                    if(_paymentProviderRepository.BillStopOrderPremiums(batchID, PaymentProviderID, PCCID, CollectionDate))
                    {
						_paymentProviderRepository.AddBillID(batchID, billingDay, CollectionDate.Date);
						_paymentProviderRepository.AddBilledPolicies(batchID);
						_paymentProviderRepository.AddBillHeaders(batchID, billingDay, CollectionDate.Date);
						_paymentProviderRepository.AddBatch(PCCID, batchID, 2, DateTime.Now);
					}                   
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
                //combine next 3 calls into one
                string procName = _paymentProviderRepository.GetSPName(PCCID);
                string stopOrderCode = _paymentProviderRepository.GetStopOrderCode(PCCID);
                int fileFormat = _paymentProviderRepository.GetFileFormat(PCCID);
                DataTable data = new DataTable();
                if (!string.IsNullOrEmpty(procName))
                {
                    data = _paymentProviderRepository.GeStopOrderData(procName, PCCID);
                }
                else
                {
                    throw new Exception("Procedure not Configured!");
                }
                if (data == null || data.Rows.Count == 0)
                {
                    return BadRequest("No data found.");
                }
                 
                if (fileFormat == 1) //prt
                {
                    var csvContent = FormatToPRT(data, "", "ZB Life"); //later adjust to header string or similar
                    var csvBytes = Encoding.UTF8.GetBytes(csvContent);
                    var fileName = stopOrderCode + ".prt";
                    return File(csvBytes, "application/octet-stream", fileName);
                }
                else
                {
                    var csvContent = DataTableToCsvString(data);
                    var csvBytes = Encoding.UTF8.GetBytes(csvContent);
                    var fileName = stopOrderCode + ".csv";
                    // Returning the CSV file as a download
                    return File(csvBytes, "text/csv", fileName);
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

		
        //test
        private DataTable SampleDT()
        {
            // Creating a DataTable with sample data
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("EC No", typeof(string));
            dataTable.Columns.Add("Names", typeof(string));
            dataTable.Columns.Add("AD", typeof(string));
            dataTable.Columns.Add("Payee", typeof(string));
            dataTable.Columns.Add("Ref", typeof(string));
            dataTable.Columns.Add("Id Number", typeof(string));
            dataTable.Columns.Add("From Date", typeof(DateTime));
            dataTable.Columns.Add("To Date", typeof(DateTime));
            dataTable.Columns.Add("Amount", typeof(decimal));
            dataTable.Columns.Add("Branch", typeof(string));
            dataTable.Columns.Add("Bank Account", typeof(string));

            // Adding sample rows to the DataTable
            // Adding sample rows to the DataTable
            dataTable.Rows.Add("0274684T", "n", "KADEMA", "4361", "43993", "US2042212981", "38030374M38", new DateTime(2023, 7, 1), new DateTime(2033, 7, 15), 10.00, "0024000", "1128069547");
            dataTable.Rows.Add("0319944P", "n", "MAGAISA", "4361", "43993", "US2042213174", "58099853N83", new DateTime(2023, 7, 1), new DateTime(2026, 3, 2), 101.00, "0004000", "4564142831409");
            dataTable.Rows.Add("0334124C", "n", "MANGENA", "4361", "43993", "US2042213148", "08292445B03", new DateTime(2023, 7, 1), new DateTime(2027, 2, 16), 30.00, "0004000", "4652588134409");
            return dataTable;
        }
        private string FormatToPRT(DataTable DT, string Title, string Organisation)
        {
            StringBuilder sb = new StringBuilder();
			// Write header row
			int pageCount = 1;
			AddTitle(ref sb, Organisation,pageCount);
            decimal total=0;
            int rowCount = DT.Rows.Count;           
			for (int i=0;i<rowCount;i++)
            {
				if ((i % 58 == 0) && (i > 0))
				{
                    pageCount += 1;
					sb.AppendLine("\f");
					AddTitle(ref sb, Organisation,pageCount);
				}
				DataRow DR = DT.Rows[i];
				sb.Append(PadRight(DR[0].ToString(), 8));
                sb.Append(' ');
                sb.Append(PadRight(DR[1].ToString(), 4));
                sb.Append(' ');
                sb.Append(PadRight(DR[2].ToString(), 20));
                sb.Append(' ');
                sb.Append(PadRight(DR[3].ToString(), 4));
                sb.Append(' ');
                sb.Append(PadRight(DR[4].ToString(), 5));
                sb.Append(' ');
                sb.Append(PadRight(DR[5].ToString(), 13));
                sb.Append(' ');
                sb.Append(PadRight(DR[6].ToString(), 15));
                sb.Append(' ');
                sb.Append(PadRight(DR[7].ToString(), 10));
                sb.Append(' ');
                sb.Append(PadRight(DR[8].ToString(), 10));
                sb.Append(' ');
                sb.Append(PadLeft(DR[9].ToString(), 13));
                sb.Append(' ');
                sb.Append(PadRight(DR[10].ToString(), 7));
                sb.Append(' ');
                sb.Append(PadRight(DR[11].ToString(), 20));
				sb.Append(Environment.NewLine);
                total += Convert.ToDecimal(DR["Amount"]);
			}
			sb.Append(Environment.NewLine);
            sb.AppendLine("Total Records:     "+ rowCount +"                        Total Amount: " + total);
            sb.AppendLine("\f");
            return sb.ToString();
        }
        // Helper method to pad a string with spaces to a specified length
        private string PadRight(string input, int length)
        {
            return input.PadRight(length);
        }
        private string PadLeft(string input, int length)
        {
            return input.PadLeft(length);
        }
        private string PrepadString(string input, int targetLength)
        {
            if (input.Length >= targetLength)
            {
                return input; // No need for padding if the input string is already at or exceeds the target length
            }
            else
            {
                int paddingLength = targetLength - input.Length;
                return new string(' ', paddingLength) + input;
            }
        }
        private string OutputExactLengthString(string input, int targetLength)
        {
            if (input.Length >= targetLength)
            {
                return input.Substring(0, targetLength); // Truncate the input string if it exceeds the target length
            }
            else
            {
                int paddingLength = targetLength - input.Length;
                return new string(' ', paddingLength) + input;
            }
        }
        //List<string> ReadValuesToList(string input)
        //{
        //    // Split the input string based on one or more whitespace characters
        //    string[] values = Regex.Split(input.Trim(), @"\s+");

        //    // Convert the array to a List<string>
        //    List<string> result = new List<string>(values);

        //    return result;
        //}
        string RepeatCharacter(char character, int n)
        {
            return new string(character, n);
        } 
        private void AddTitle(ref StringBuilder sb, string Organisation, int PageCount)
        {
			sb.AppendLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} SPADLOAD PROVISIONAL VALID RECORDS LOADED REPORT (FINAL REPORT TO BE PRODUCED AT MONTH END!!!)".PadRight(95) + "Page:   " + PageCount);
			sb.AppendLine($"          ORGANISATION:  " + Organisation);
			sb.AppendLine("EC No    Tran Names                  AD Payee Ref           Id Number       From Date  To Date           Amount  Branch Bank Account");
			sb.Append(RepeatCharacter('-', 8));
			sb.Append(' ');
			sb.Append(RepeatCharacter('-', 4));
			sb.Append(' ');
			sb.Append(RepeatCharacter('-', 20));
			sb.Append(' ');
			sb.Append(RepeatCharacter('-', 4));
			sb.Append(' ');
			sb.Append(RepeatCharacter('-', 5));
			sb.Append(' ');
			sb.Append(RepeatCharacter('-', 13));
			sb.Append(' ');
			sb.Append(RepeatCharacter('-', 15));
			sb.Append(' ');
			sb.Append(RepeatCharacter('-', 10));
			sb.Append(' ');
			sb.Append(RepeatCharacter('-', 10));
			sb.Append(' ');
			sb.Append(RepeatCharacter('-', 13));
			sb.Append(' ');
			sb.Append(RepeatCharacter('-', 7));
			sb.Append(' ');
			sb.Append(RepeatCharacter('-', 20));
			sb.Append(Environment.NewLine);
		}
    }
}
