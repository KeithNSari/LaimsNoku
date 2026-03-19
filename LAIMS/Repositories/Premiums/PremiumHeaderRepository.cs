using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Premiums
{
    public class PremiumHeaderRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public PremiumHeaderRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public void CreatePremiumHeader(PremiumHeader premiumHeader)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"
                INSERT INTO [dbo].[PremiumHeader] 
                (BillingID, DocumentNo, TotalAmount, DatePaymentReceived, 
                DatePaymentRecorded, AddedBy, AddedOn) 
                VALUES 
                (@BillingID, @DocumentNo, @TotalAmount, @DatePaymentReceived, 
                @DatePaymentRecorded, @AddedBy, @AddedOn)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BillingID", premiumHeader.BillingID);
                    command.Parameters.AddWithValue("@DocumentNo", premiumHeader.DocumentNo ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TotalAmount", premiumHeader.TotalAmount);
                    command.Parameters.AddWithValue("@DatePaymentReceived", premiumHeader.DatePaymentReceived);
                    command.Parameters.AddWithValue("@DatePaymentRecorded", premiumHeader.DatePaymentRecorded);
                    command.Parameters.AddWithValue("@AddedBy", premiumHeader.AddedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", premiumHeader.AddedOn);

                    command.ExecuteNonQuery();
                }
            }
        }

        public PremiumHeader ReadPremiumHeader(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM [dbo].[PremiumHeader] WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToPremiumHeader(reader);
                        }
                    }
                }

                return null;
            }
        }

        public void UpdatePremiumHeader(PremiumHeader premiumHeader)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"
                UPDATE [dbo].[PremiumHeader] 
                SET BillingID = @BillingID, DocumentNo = @DocumentNo, 
                    TotalAmount = @TotalAmount, DatePaymentReceived = @DatePaymentReceived, 
                    DatePaymentRecorded = @DatePaymentRecorded, AddedBy = @AddedBy, 
                    AddedOn = @AddedOn 
                WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", premiumHeader.ID);
                    command.Parameters.AddWithValue("@BillingID", premiumHeader.BillingID);
                    command.Parameters.AddWithValue("@DocumentNo", premiumHeader.DocumentNo ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TotalAmount", premiumHeader.TotalAmount);
                    command.Parameters.AddWithValue("@DatePaymentReceived", premiumHeader.DatePaymentReceived);
                    command.Parameters.AddWithValue("@DatePaymentRecorded", premiumHeader.DatePaymentRecorded);
                    command.Parameters.AddWithValue("@AddedBy", premiumHeader.AddedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", premiumHeader.AddedOn);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeletePremiumHeader(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM [dbo].[PremiumHeader] WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        private PremiumHeader MapToPremiumHeader(SqlDataReader reader)
        {
            return new PremiumHeader
            {
                ID = (int)reader["ID"],
                BillingID = (int)reader["BillingID"],
                DocumentNo = reader["DocumentNo"] == DBNull.Value ? null : (string)reader["DocumentNo"],
                TotalAmount = (decimal)reader["TotalAmount"],
                DatePaymentReceived = (DateTime)reader["DatePaymentReceived"],
                DatePaymentRecorded = (DateTime)reader["DatePaymentRecorded"],
                AddedBy = reader["AddedBy"] == DBNull.Value ? null : (string)reader["AddedBy"],
                AddedOn = (DateTime)reader["AddedOn"]
            };
        }
    }
}
