using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using Microsoft.Data.SqlClient;
using System.Data;
namespace LAIMS.Repositories.Lifeproducts
{

    public class CurrencyRepository: ICurrencyRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public CurrencyRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
        public int CheckExistence(Currency currency)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @CurrencyId int=0; SELECT @CurrencyId=[Id] FROM Currencies Where ([Name]=@Name Or [ShortCode]=@ShortCode) And ARCHIVED=0; SELECT @CurrencyId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", currency.CurrencyName);
                    command.Parameters.AddWithValue("@ShortCode", currency.ShortCode);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int CheckExistenceOther(Currency currency)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM Currencies Where ([Name]=@Name Or [ShortCode]=@ShortCode) And ([ID]!=@ID) And ARCHIVED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, currency);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void InsertCurrency(Currency currency)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "IF(@Default=1) BEGIN UPDATE [dbo].[Currencies] SET [Default]=0 WHERE [Default]=1 END; INSERT INTO [dbo].[Currencies]([Name],[ShortCode],[Default],[AddedBy],[AddedOn]) VALUES(@Name,@ShortCode,@Default,@AddedBy,@AddedOn)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, currency);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void ArchiveCurrency(Currency currency)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Currencies] Set [Archived]=1,[ArchivedBy]=@AddedBy,[ArchivedOn]=@AddedOn WHERE [Id]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", currency.ID);
                    command.Parameters.AddWithValue("@AddedBy", currency.AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", currency.AddedOn);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateCurrency(Currency currency)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "IF(@Default=1) BEGIN UPDATE [dbo].[Currencies] SET [Default]=0 WHERE ([Default]=1) AND ([ID] !=@ID) END; UPDATE [dbo].[Currencies] Set [Name]=@Name,[ShortCode]=@ShortCode,[Default]=@Default,[AddedBy]=@AddedBy,[AddedOn]=@AddedOn WHERE [Id]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, currency);
                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable Get()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [Id],[Name],[ShortCode],Case [Default] WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END AS [Default],[AddedOn] FROM [dbo].[Currencies] WHERE [Archived]=0 ORDER By [Name] Asc";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public Currency GetCurrency(int ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            Currency currency = new Currency();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM Currencies WHERE [ID]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            currency = MapDataReaderToCurrencyType(reader);                             
                        }
                    }
                }
            }

            return currency;
        }
        public List<Currency> GetAllCurrencies()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<Currency> currencies = new List<Currency>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM Currencies WHERE [Archived]=0";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Currency currency = MapDataReaderToCurrencyType(reader);
                            currencies.Add(currency);
                        }
                    }
                }
            }

            return currencies;
        }
        private void AddParameters(SqlCommand command, Currency currency)
        {
            command.Parameters.AddWithValue("@ID", currency.ID);
            command.Parameters.AddWithValue("@Name", currency.CurrencyName);
            command.Parameters.AddWithValue("@ShortCode", currency.ShortCode);
            command.Parameters.AddWithValue("@Default", currency.Default); 
            command.Parameters.AddWithValue("@AddedBy", currency.AddedBy);
            command.Parameters.AddWithValue("@AddedOn", currency.AddedOn); 
        }
        private Currency MapDataReaderToCurrencyType(SqlDataReader reader)
        {
            return new Currency
            {
                ID = Convert.ToInt32(reader["ID"]),
                ShortCode = reader["ShortCode"].ToString(),
                CurrencyName = reader["Name"].ToString(),
                Default = (byte)reader["Default"],
                AddedOn = reader["AddedOn"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["AddedOn"]),
                AddedBy = reader["AddedBy"].ToString()
            };
        }

    }
}
