using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Premiums
{
    public class ReversalLineRepository: IReversalLineRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public ReversalLineRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public void InsertReversalLine(ReversalLine reversalLine)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"INSERT INTO ReversalLines (HeaderID, OriginalPaymentID, SourceID, Amount, Reversed, ReversedOn, ReversedBy)
                             VALUES (@HeaderID, @OriginalPaymentID, @SourceID, @Amount, @Reversed, @ReversedOn, @ReversedBy);";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@HeaderID", reversalLine.HeaderID);
                command.Parameters.AddWithValue("@OriginalPaymentID", reversalLine.OriginalPaymentID);
                command.Parameters.AddWithValue("@SourceID", reversalLine.SourceID);
                command.Parameters.AddWithValue("@Amount", reversalLine.Amount);
                command.Parameters.AddWithValue("@Reversed", reversalLine.Reversed);
                command.Parameters.AddWithValue("@ReversedOn", reversalLine.ReversedOn);
                command.Parameters.AddWithValue("@ReversedBy", reversalLine.ReversedBy);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public ReversalLine GetReversalLineById(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"SELECT ID, HeaderID, OriginalPaymentID, SourceID, Amount, Reversed, ReversedOn, ReversedBy
                             FROM ReversalLines
                             WHERE ID = @Id;";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return MapReversalLineFromReader(reader);
                }

                return null;
            }
        }

        public List<ReversalLine> GetAllReversalLines()
        {
            List<ReversalLine> reversalLines = new List<ReversalLine>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"SELECT ID, HeaderID, OriginalPaymentID, SourceID, Amount, Reversed, ReversedOn, ReversedBy
                             FROM ReversalLines;";

                SqlCommand command = new SqlCommand(query, connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ReversalLine reversalLine = MapReversalLineFromReader(reader);
                    reversalLines.Add(reversalLine);
                }
            }

            return reversalLines;
        }

        public void UpdateReversalLine(ReversalLine reversalLine)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"UPDATE ReversalLines
                             SET HeaderID = @HeaderID,
                                 OriginalPaymentID = @OriginalPaymentID,
                                 SourceID = @SourceID,
                                 Amount = @Amount,
                                 Reversed = @Reversed,
                                 ReversedOn = @ReversedOn,
                                 ReversedBy = @ReversedBy
                             WHERE ID = @Id;";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@HeaderID", reversalLine.HeaderID);
                command.Parameters.AddWithValue("@OriginalPaymentID", reversalLine.OriginalPaymentID);
                command.Parameters.AddWithValue("@SourceID", reversalLine.SourceID);
                command.Parameters.AddWithValue("@Amount", reversalLine.Amount);
                command.Parameters.AddWithValue("@Reversed", reversalLine.Reversed);
                command.Parameters.AddWithValue("@ReversedOn", reversalLine.ReversedOn);
                command.Parameters.AddWithValue("@ReversedBy", reversalLine.ReversedBy);
                command.Parameters.AddWithValue("@Id", reversalLine.ID);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteReversalLine(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"DELETE FROM ReversalLines
                             WHERE ID = @Id;";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private ReversalLine MapReversalLineFromReader(SqlDataReader reader)
        {
            return new ReversalLine
            {
                ID = (int)reader["ID"],
                HeaderID = (int)reader["HeaderID"],
                OriginalPaymentID = (int)reader["OriginalPaymentID"],
                SourceID = (int)reader["SourceID"],
                Amount = (decimal)reader["Amount"],
                Reversed = (byte)reader["Reversed"],
                ReversedOn = (DateTime)reader["ReversedOn"],
                ReversedBy = (string)reader["ReversedBy"]
            };
        }
    }
}
