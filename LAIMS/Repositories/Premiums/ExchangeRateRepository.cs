using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;
using System.Data;
namespace LAIMS.Repositories.Premiums
{ 
	public class ExchangeRateRepository: IExchangeRateRepository
	{
		private IConfiguration _configuration;
		private IWebHostEnvironment _environment;
		private readonly string Database; 

		public ExchangeRateRepository(IConfiguration configuration, IWebHostEnvironment environment)
		{
			_configuration = configuration;
			_environment = environment;
			Database = _configuration.GetConnectionString("DefaultConnection");
		}

		public void SaveExchangeRate(ExchangeRate exchangeRate)
		{
			using (SqlConnection conn = new SqlConnection(Database))
			{
				string query = "INSERT INTO ExchangeRates (BaseCurrency, OtherCurrency, Value, EffectiveDate, AddedBy, AddedOn) VALUES (@BaseCurrency, @OtherCurrency, @Value, @EffectiveDate, @AddedBy, @AddedOn)";
				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@BaseCurrency", exchangeRate.BaseCurrency);
					cmd.Parameters.AddWithValue("@OtherCurrency", exchangeRate.OtherCurrency);
					cmd.Parameters.AddWithValue("@Value", exchangeRate.Value);
					cmd.Parameters.AddWithValue("@EffectiveDate", exchangeRate.EffectiveDate);
					cmd.Parameters.AddWithValue("@AddedBy", exchangeRate.AddedBy);
					cmd.Parameters.AddWithValue("@AddedOn", exchangeRate.AddedOn);

					conn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		public decimal? GetLatestExchangeRate(int baseCurrency, int otherCurrency)
		{
			using (SqlConnection conn = new SqlConnection(Database))
			{
				string query = "SELECT TOP 1 Value FROM ExchangeRates WHERE BaseCurrency = @BaseCurrency AND OtherCurrency = @OtherCurrency AND EffectiveDate <= @CurrentDate ORDER BY EffectiveDate DESC, AddedOn DESC";
				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@BaseCurrency", baseCurrency);
					cmd.Parameters.AddWithValue("@OtherCurrency", otherCurrency);
					cmd.Parameters.AddWithValue("@CurrentDate", DateTime.Now.Date);

					conn.Open();
					object result = cmd.ExecuteScalar();
					return result != null ? (decimal?)result : null;
				}
			}
		}
		public DataTable GetLatestExchangeRates()
		{
			using (SqlConnection conn = new SqlConnection(Database))
			{
				string query = @"SELECT TOP 100 er.EntryNo, c1.Name AS BaseCurrency, c2.Name AS OtherCurrency, er.Value, er.EffectiveDate, er.AddedBy, er.AddedOn
                            FROM ExchangeRates er
                            JOIN Currencies c1 ON er.BaseCurrency = c1.Id
                            JOIN Currencies c2 ON er.OtherCurrency = c2.Id
                            ORDER BY er.EntryNo DESC";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					conn.Open();
					using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
					{
						DataTable dataTable = new DataTable();
						adapter.Fill(dataTable);
						return dataTable;
					}
				}
			}
		}
	}

}
