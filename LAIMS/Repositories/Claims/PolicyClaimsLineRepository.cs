using LAIMS.Interfaces.Claims;
using LAIMS.Models.Claims;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Claims
{
    public class PolicyClaimsLineRepository: IPolicyClaimsLineRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private readonly string Database;

        public PolicyClaimsLineRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public PolicyClaimsLine GetLineById(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM PolicyClaimsLines WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapData(reader);
                        }
                    }
                }

                return null;
            }
        }
        public List<PolicyClaimsLine> GetLinesByHeaderID(int headerID)
        {
            List<PolicyClaimsLine> lines = new List<PolicyClaimsLine>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM PolicyClaimsLines WHERE HeaderID = @HeaderID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HeaderID", headerID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lines.Add(MapData(reader));
                        }
                    }
                }
            }

            return lines;
        }

        public void AddLine(PolicyClaimsLine line)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"DECLARE @Cover decimal(18,2)=0; SELECT @Cover=[Cover] FROM [dbo].[PolicyBeneficiariesLines] WHERE [ID]=@PolicyBeneficiariesLineID
                INSERT INTO PolicyClaimsLines
                (HeaderID, PTLBenefitID, PolicyBeneficiariesLineID, PolicyUnitsID, Amount)
                VALUES
                (@HeaderID, @PTLBenefitID, @PolicyBeneficiariesLineID, @PolicyUnitsID, @Cover);
                SELECT SCOPE_IDENTITY();";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    MapParameters(command, line);
                    line.ID = Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void UpdateLine(PolicyClaimsLine line)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"
                UPDATE PolicyClaimsLines
                SET HeaderID = @HeaderID, PTLBenefitID = @PTLBenefitID, PolicyBeneficiariesLineID = @PolicyBeneficiariesLineID,
                    PolicyUnitsID = @PolicyUnitsID, Amount = @Amount
                WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    MapParameters(command, line);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteLine(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM PolicyClaimsLines WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        private PolicyClaimsLine MapData(SqlDataReader reader)
        {
            return new PolicyClaimsLine
            {
                ID = Convert.ToInt32(reader["ID"]),
                HeaderID = Convert.ToInt32(reader["HeaderID"]),
                PTLBenefitID = Convert.ToInt32(reader["PTLBenefitID"]),
                PolicyBeneficiariesLineID = Convert.ToInt32(reader["PolicyBeneficiariesLineID"]),
                PolicyUnitsID = Convert.ToInt32(reader["PolicyUnitsID"]),
                Amount = Convert.ToDecimal(reader["Amount"])
            };
        }

        private void MapParameters(SqlCommand command, PolicyClaimsLine line)
        {
            command.Parameters.AddWithValue("@ID", line.ID);
            command.Parameters.AddWithValue("@HeaderID", line.HeaderID);
            command.Parameters.AddWithValue("@PTLBenefitID", line.PTLBenefitID ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PolicyBeneficiariesLineID", line.PolicyBeneficiariesLineID);
            command.Parameters.AddWithValue("@PolicyUnitsID", line.PolicyUnitsID ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Amount", line.Amount);
        }
    }
}
