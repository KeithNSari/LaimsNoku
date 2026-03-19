using LAIMS.Interfaces.Banking;
using LAIMS.Models.Banking;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Banking
{
    public class BankRepository: IBankRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public BankRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public int AddBank(Bank bank)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"DECLARE @ID int=0; SELECT @ID=[EntryNo] FROM [Banks] WHERE [MemberID]=@MemberID; IF(@ID=0) BEGIN INSERT INTO Banks (MemberID, Code,BankAccountNoFormat,BankAccountNoFormatDesc, AddedBy, AddedOn, Archived, ArchivedBy, ArchivedOn)
                             VALUES (@MemberID, @Code,@BankAccountNoFormat,@BankAccountNoFormatDesc, @AddedBy, @AddedOn, @Archived, @ArchivedBy, @ArchivedOn); SELECT @ID=SCOPE_IDENTITY() END; SELECT @ID ";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MemberID", bank.BankID);
                    command.Parameters.AddWithValue("@Code", bank.Code);
                    command.Parameters.AddWithValue("@BankAccountNoFormat", bank.BankAccountNoFormat);
                    command.Parameters.AddWithValue("@BankAccountNoFormatDesc", bank.FormatDescription);
                    command.Parameters.AddWithValue("@AddedBy", bank.AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", bank.AddedOn);
                    command.Parameters.AddWithValue("@Archived", bank.Archived);
                    command.Parameters.AddWithValue("@ArchivedBy", bank.ArchivedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ArchivedOn", bank.ArchivedOn ?? (object)DBNull.Value);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int GetBankID( int PaymentProviderID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"DECLARE @BankID int=0;SELECT @BankID=[MemberID] FROM [dbo].[PaymentProviders] WHERE [ID]=@PaymentProviderID; SELECT @BankID";
                using (SqlCommand command = new SqlCommand(query, connection))
                { 
                    command.Parameters.AddWithValue("@PaymentProviderID", PaymentProviderID); 
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        // Read all Bank records
        public List<Bank> GetAllBanks()
        {
            List<Bank> banks = new List<Bank>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT [Banks].[EntryNo],[Members].[Name1] AS [BankName],[Banks].[MemberID] AS [BankID],[Code],[Banks].[AddedBy],[Banks].[AddedOn],[Banks].[Archived],[Banks].[ArchivedBy],[Banks].[ArchivedOn] FROM [dbo].[Banks] LEFT JOIN [Members] ON [Members].[ID]=[Banks].[MemberID] WHERE [Members].[Name1] IS NOT NULL ORDER BY [BankName] ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            banks.Add(MapReaderToBank(reader));
                        }
                    }
                }
            }
            return banks;
        }
        public BankAccountFormat GetBankAccountFormat(int BankID)
        {
            BankAccountFormat bankAccountFormat = new();
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT [BankAccountNoFormat],[BankAccountNoFormatDesc] FROM [dbo].[Banks] WHERE [MemberID]=@BankID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BankID", BankID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bankAccountFormat.BankAccountNoFormat = reader["BankAccountNoFormat"].ToString();
                            bankAccountFormat.FormatDescription = reader["BankAccountNoFormatDesc"].ToString();                            
                        }
                    }
                }
            }
            return bankAccountFormat;
        }
		public BankAccountFormat GetBankAccountFormatByPaymentProvider(int PaymentProviderID)
		{
			BankAccountFormat bankAccountFormat = new();
			var Database = _configuration.GetConnectionString("DefaultConnection");
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();

				string query = "DECLARE @MemberID int=0; SELECT @MemberID=[MemberID] FROM [dbo].[PaymentProviders] WHERE [ID]=@PaymentProviderID; SELECT [BankAccountNoFormat],[BankAccountNoFormatDesc] FROM [dbo].[Banks] WHERE [MemberID]=@MemberID";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@PaymentProviderID", PaymentProviderID);
					using (SqlDataReader reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							bankAccountFormat.BankAccountNoFormat = reader["BankAccountNoFormat"].ToString();
							bankAccountFormat.FormatDescription = reader["BankAccountNoFormatDesc"].ToString();
						}
					}
				}
			}
			return bankAccountFormat;
		}
		// Update an existing Bank record
		public void UpdateBank(Bank bank)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"UPDATE Banks
                             SET Code = @Code, AddedBy = @AddedBy, AddedOn = @AddedOn, Archived = @Archived,
                                 ArchivedBy = @ArchivedBy, ArchivedOn = @ArchivedOn
                             WHERE MemberID = @MemberID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MemberID", bank.BankID);
                    command.Parameters.AddWithValue("@Code", bank.Code);
                    command.Parameters.AddWithValue("@AddedBy", bank.AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", bank.AddedOn);
                    command.Parameters.AddWithValue("@Archived", bank.Archived);
                    command.Parameters.AddWithValue("@ArchivedBy", bank.ArchivedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ArchivedOn", bank.ArchivedOn ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Delete a Bank record
        public void DeleteBank(int memberID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM Banks WHERE MemberID = @MemberID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MemberID", memberID);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Helper method to map SqlDataReader to Bank object
        private Bank MapReaderToBank(SqlDataReader reader)
        {
            return new Bank
            {
                EntryNo = (int)reader["EntryNo"],
                BankID = (int)reader["BankID"],
                BankName = (string)reader["BankName"]
                //Code = reader["Code"].ToString(),
                //AddedBy = reader["AddedBy"].ToString(),
                //AddedOn = (DateTime)reader["AddedOn"],
                //Archived = (byte)reader["Archived"],
                //ArchivedBy = reader["ArchivedBy"] != DBNull.Value ? reader["ArchivedBy"].ToString() : null,
                //ArchivedOn = reader["ArchivedOn"] != DBNull.Value ? (DateTime?)reader["ArchivedOn"] : null
                // Add any additional properties as needed
            };
        }
    }
}
