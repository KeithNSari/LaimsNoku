using LAIMS.Interfaces.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LAIMS.Areas.Claims.Pages
{
	[Authorize(Roles = "Claims Initiator")]
	public class InvestmentsIndexModel : PageModel
    {
		private IPolicyClaimRepository _policyClaimRepository;
		public InvestmentsIndexModel(IPolicyClaimRepository policyClaimRepository)
		{
			_policyClaimRepository = policyClaimRepository;
		}
		[BindProperty]
		public int ResultsCount { get; set; } = 0;
		[BindProperty(SupportsGet = true)]
		public string ReturnUrl { get; set; } 

		[BindProperty]
		public DateTime StartDate { get; set; }
		[BindProperty]
		public DateTime EndDate { get; set; }
		public DataTable ClaimsDT;
		public void OnGet()
        {
			try
			{
				StartDate = DateTime.Now;
				EndDate = DateTime.Now;
				ClaimsDT = _policyClaimRepository.GetLatestInvestmentsClaims();
				ResultsCount = ClaimsDT.Rows.Count;
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
				if (StartDate > EndDate) throw new Exception("Start date cannot be greater than end date!");
				ClaimsDT = _policyClaimRepository.SearchInvestmentsClaims(StartDate, EndDate);
				ResultsCount = ClaimsDT.Rows.Count;
			}
			catch (Exception ex)
			{
				ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
			}
		}
		public IActionResult OnPostDownload()
		{
			try
			{
				if (StartDate > EndDate) throw new Exception("Start date cannot be greater than end date!");
				ClaimsDT = _policyClaimRepository.DownloadInvestmentsClaims(StartDate, EndDate);
				ResultsCount = ClaimsDT.Rows.Count;
				if (ResultsCount > 0)
				{
					var csvContent = DataTableToCsvString(ClaimsDT);
					var csvBytes = Encoding.UTF8.GetBytes(csvContent);
					var fileName = "InvestmentClaims_" + StartDate.Date.ToString("yy_MM_dd") + "_" + EndDate.Date.ToString("yy_MM_dd") + ".csv";
					// Returning the CSV file as a download
					return File(csvBytes, "text/csv", fileName);
				}				
			}
			catch (Exception ex)
			{
				ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
			}
			return Page();
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
	}

}
