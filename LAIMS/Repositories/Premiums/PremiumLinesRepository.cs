using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Premiums
{
    public class PremiumLineRepository: IPremiumLineRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public PremiumLineRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public void CreatePremiumLines(PremiumLine premiumLines)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"
                INSERT INTO [dbo].[PremiumLines] 
                (PremiumHeaderID, SuspenseID, PaymentMethodID, PaymentProviderID, 
                InternalAccountsID, ExchangeRateID, SourceID, Source, CurrencyID, 
                Amount, Reference, Details, PaymentDate, AddedBy, AddedOn) 
                VALUES 
                (@PremiumHeaderID, @SuspenseID, @PaymentMethodID, @PaymentProviderID, 
                @InternalAccountsID, @ExchangeRateID, @SourceID, @Source, @CurrencyID, 
                @Amount, @Reference, @Details, @PaymentDate, @AddedBy, @AddedOn)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PremiumHeaderID", premiumLines.PremiumHeaderID);
                    command.Parameters.AddWithValue("@SuspenseID", premiumLines.SuspenseID);
                    command.Parameters.AddWithValue("@PaymentMethodID", premiumLines.PaymentMethodID);
                    command.Parameters.AddWithValue("@PaymentProviderID", premiumLines.PaymentProviderID);
                    command.Parameters.AddWithValue("@InternalAccountsID", premiumLines.InternalAccountsID);
                    command.Parameters.AddWithValue("@ExchangeRateID", premiumLines.ExchangeRateID);
                    command.Parameters.AddWithValue("@SourceID", premiumLines.SourceID);
                    command.Parameters.AddWithValue("@Source", premiumLines.Source ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CurrencyID", premiumLines.CurrencyID);
                    command.Parameters.AddWithValue("@Amount", premiumLines.Amount);
                    command.Parameters.AddWithValue("@Reference", premiumLines.Reference ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Details", premiumLines.Details ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PaymentDate", premiumLines.PaymentDate);
                    command.Parameters.AddWithValue("@AddedBy", premiumLines.AddedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", premiumLines.AddedOn);

                    command.ExecuteNonQuery();
                }
            }
        }

        public PremiumLine ReadPremiumLines(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM [dbo].[PremiumLines] WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToPremiumLines(reader);
                        }
                    }
                }

                return null;
            }
        }

        public void UpdatePremiumLines(PremiumLine premiumLines)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"
                UPDATE [dbo].[PremiumLines] 
                SET PremiumHeaderID = @PremiumHeaderID, SuspenseID = @SuspenseID, 
                    PaymentMethodID = @PaymentMethodID, PaymentProviderID = @PaymentProviderID, 
                    InternalAccountsID = @InternalAccountsID, ExchangeRateID = @ExchangeRateID, 
                    SourceID = @SourceID, Source = @Source, CurrencyID = @CurrencyID, 
                    Amount = @Amount, Reference = @Reference, Details = @Details, 
                    PaymentDate = @PaymentDate, AddedBy = @AddedBy, AddedOn = @AddedOn 
                WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", premiumLines.ID);
                    command.Parameters.AddWithValue("@PremiumHeaderID", premiumLines.PremiumHeaderID);
                    command.Parameters.AddWithValue("@SuspenseID", premiumLines.SuspenseID);
                    command.Parameters.AddWithValue("@PaymentMethodID", premiumLines.PaymentMethodID);
                    command.Parameters.AddWithValue("@PaymentProviderID", premiumLines.PaymentProviderID);
                    command.Parameters.AddWithValue("@InternalAccountsID", premiumLines.InternalAccountsID);
                    command.Parameters.AddWithValue("@ExchangeRateID", premiumLines.ExchangeRateID);
                    command.Parameters.AddWithValue("@SourceID", premiumLines.SourceID);
                    command.Parameters.AddWithValue("@Source", premiumLines.Source ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CurrencyID", premiumLines.CurrencyID);
                    command.Parameters.AddWithValue("@Amount", premiumLines.Amount);
                    command.Parameters.AddWithValue("@Reference", premiumLines.Reference ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Details", premiumLines.Details ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PaymentDate", premiumLines.PaymentDate);
                    command.Parameters.AddWithValue("@AddedBy", premiumLines.AddedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", premiumLines.AddedOn);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeletePremiumLines(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM [dbo].[PremiumLines] WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        private PremiumLine MapToPremiumLines(SqlDataReader reader)
        {
            return new PremiumLine
            {
                ID = (int)reader["ID"],
                PremiumHeaderID = (int)reader["PremiumHeaderID"],
                SuspenseID = (int)reader["SuspenseID"],
                PaymentMethodID = (int)reader["PaymentMethodID"],
                PaymentProviderID = (int)reader["PaymentProviderID"],
                InternalAccountsID = (int)reader["InternalAccountsID"],
                ExchangeRateID = (int)reader["ExchangeRateID"],
                SourceID = (int)reader["SourceID"],
                Source = reader["Source"] == DBNull.Value ? null : (string)reader["Source"],
                CurrencyID = (int)reader["CurrencyID"],
                Amount = (decimal)reader["Amount"],
                Reference = reader["Reference"] == DBNull.Value ? null : (string)reader["Reference"],
                Details = reader["Details"] == DBNull.Value ? null : (string)reader["Details"],
                PaymentDate = (DateTime)reader["PaymentDate"],
                AddedBy = reader["AddedBy"] == DBNull.Value ? null : (string)reader["AddedBy"],
                AddedOn = (DateTime)reader["AddedOn"]
            };
        }
    } 
}
