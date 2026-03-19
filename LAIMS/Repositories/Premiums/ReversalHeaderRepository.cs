using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;
using System.Data;
namespace LAIMS.Repositories.Premiums
{
    public class ReversalHeaderRepository : IReversalHeaderRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public ReversalHeaderRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        // Insert a new ReversalHeader record
        public int InsertReversalHeader(ReversalHeader reversalHeader)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"INSERT INTO ReversalHeader (CurrencyID, Amount, ReversalReason, ReversalComment, Reversed, ReversedOn, ReversedBy)
                             VALUES (@CurrencyID, @Amount, @ReversalReason, @ReversalComment, @Reversed, @ReversedOn, @ReversedBy);
                             SELECT SCOPE_IDENTITY();"; // Retrieve the ID of the inserted record
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CurrencyID", reversalHeader.CurrencyID);
                command.Parameters.AddWithValue("@Amount", reversalHeader.Amount);
                command.Parameters.AddWithValue("@ReversalReason", reversalHeader.ReversalReason);
                command.Parameters.AddWithValue("@ReversalComment", (object)reversalHeader.ReversalComment ?? DBNull.Value);
                command.Parameters.AddWithValue("@Reversed", reversalHeader.Reversed);
                command.Parameters.AddWithValue("@ReversedOn", reversalHeader.ReversedOn);
                command.Parameters.AddWithValue("@ReversedBy", reversalHeader.ReversedBy);
                connection.Open();
                object insertedId = command.ExecuteScalar();
                if (insertedId != DBNull.Value && insertedId != null)
                {
                    return Convert.ToInt32(insertedId);
                }
                throw new InvalidOperationException("Insert operation failed or ID not returned.");
            }
        }
        public void UpdatePolicyStatus(Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "Policies_UpdateStatus";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.ExecuteNonQuery();
                }
            }
        }
        // Retrieve a ReversalHeader record by ID
        public ReversalHeader GetReversalHeaderById(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"SELECT ID, CurrencyID, Amount, ReversalReason, ReversalComment, Reversed, ReversedOn, ReversedBy
                             FROM ReversalHeader
                             WHERE ID = @Id;";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return MapReversalHeaderFromReader(reader);
                }
                return null;
            }
        }

        // Retrieve all ReversalHeader records
        public List<ReversalHeader> GetAllReversalHeaders()
        {
            List<ReversalHeader> reversalHeaders = new List<ReversalHeader>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"SELECT ID, CurrencyID, Amount, ReversalReason, ReversalComment, Reversed, ReversedOn, ReversedBy
                             FROM ReversalHeader;";

                SqlCommand command = new SqlCommand(query, connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ReversalHeader reversalHeader = MapReversalHeaderFromReader(reader);
                    reversalHeaders.Add(reversalHeader);
                }
            }

            return reversalHeaders;
        }
        public DataTable GetReversalHistory()
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            string query = "SELECT TOP (100) [Currencies].[Name] AS [Currency],[Amount],[StatiiReasons].[Reason],[ReversalComment],[Reversed],[ReversedOn],[AspNetUsers].[UserName] FROM [dbo].[ReversalHeader] LEFT JOIN [Currencies] ON [Currencies].[ID]= [ReversalHeader].[CurrencyID] LEFT JOIN [StatiiReasons] ON [StatiiReasons].[ReasonID]=[ReversalHeader].[ReversalReason] LEFT JOIN [AspNetUsers] ON [AspNetUsers].[ID]=[ReversalHeader].[ReversedBy] ORDER BY [ReversalHeader].[ReversedOn] DESC";
            cmd.CommandText = query; 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }

        // Update an existing ReversalHeader record
        public void UpdateReversalHeader(ReversalHeader reversalHeader)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"UPDATE ReversalHeader
                             SET CurrencyID = @CurrencyID,
                                 Amount = @Amount,
                                 ReversalReason = @ReversalReason,
                                 ReversalComment = @ReversalComment,
                                 Reversed = @Reversed,
                                 ReversedOn = @ReversedOn,
                                 ReversedBy = @ReversedBy
                             WHERE ID = @Id;";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CurrencyID", reversalHeader.CurrencyID);
                command.Parameters.AddWithValue("@Amount", reversalHeader.Amount);
                command.Parameters.AddWithValue("@ReversalReason", reversalHeader.ReversalReason);
                command.Parameters.AddWithValue("@ReversalComment", (object)reversalHeader.ReversalComment ?? DBNull.Value);
                command.Parameters.AddWithValue("@Reversed", reversalHeader.Reversed);
                command.Parameters.AddWithValue("@ReversedOn", reversalHeader.ReversedOn);
                command.Parameters.AddWithValue("@ReversedBy", reversalHeader.ReversedBy);
                command.Parameters.AddWithValue("@Id", reversalHeader.ID);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Delete a ReversalHeader record by ID
        public void DeleteReversalHeader(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"DELETE FROM ReversalHeader
                             WHERE ID = @Id;";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Helper method to map ReversalHeader from SqlDataReader
        private ReversalHeader MapReversalHeaderFromReader(SqlDataReader reader)
        {
            return new ReversalHeader
            {
                ID = (int)reader["ID"],
                CurrencyID = (int)reader["CurrencyID"],
                Amount = (decimal)reader["Amount"],
                ReversalReason = (int)reader["ReversalReason"],
                ReversalComment = reader["ReversalComment"] == DBNull.Value ? null : (string)reader["ReversalComment"],
                Reversed = (byte)reader["Reversed"],
                ReversedOn = (DateTime)reader["ReversedOn"],
                ReversedBy = (string)reader["ReversedBy"]
            };
        }
    }
}