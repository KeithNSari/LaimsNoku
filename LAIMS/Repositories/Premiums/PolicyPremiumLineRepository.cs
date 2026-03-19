using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using LAIMS.Models.Policies;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Premiums
{
    public class PolicyPremiumLineRepository: IPolicyPremiumLineRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PolicyPremiumLineRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public void AddPolicyPremiumLine(PolicyPremiumLine policyPremiumLine)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "INSERT INTO PolicyPremiumsLines (PolicyPremiumsID, ProductID, Premium, StatusID, StatusDate) " +
                             "VALUES (@PolicyPremiumsID, @PolicyProductID, @Premium, @StatusID, @StatusDate)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyPremiumsID", policyPremiumLine.PolicyPremiumsID);
                    command.Parameters.AddWithValue("@PolicyProductID", policyPremiumLine.PolicyProductID);
                    command.Parameters.AddWithValue("@Premium", policyPremiumLine.Premium);
                    command.Parameters.AddWithValue("@StatusID", policyPremiumLine.StatusID);
                    command.Parameters.AddWithValue("@StatusDate", policyPremiumLine.StatusDate);

                    command.ExecuteNonQuery();
                }
            }
        }
        public List<PolicyPremiumLine> GetAllPolicyPremiumLines()
        {
            List<PolicyPremiumLine> policyPremiumLines = new List<PolicyPremiumLine>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "SELECT * FROM PolicyPremiumsLines";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyPremiumLine policyPremiumLine = MapDataReaderToPolicyPremiumLine(reader);
                            policyPremiumLines.Add(policyPremiumLine);
                        }
                    }
                }
            }

            return policyPremiumLines;
        }
        public List<PolicyPremiumLine> GetPolicyPremiumLines(int PolicyPremiumID)
        {
            List<PolicyPremiumLine> policyPremiumLines = new List<PolicyPremiumLine>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "PolicyPremium_GetLines";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("PolicyPremiumID", PolicyPremiumID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyPremiumLine policyPremiumLine = new()
                            {
                                PolicyID = Guid.Parse(reader["PolicyID"].ToString()),
                                PolicyType = Guid.Parse(reader["PolicyType"].ToString()),
                                PolicyProductID = Guid.Parse(reader["ProductID"].ToString()),
                                ID = (int)reader["PolicyPremiumsLineID"]
                            };
                            policyPremiumLines.Add(policyPremiumLine);
                        }
                    }
                }
            }
            return policyPremiumLines;
        }
        public void UpdatePolicyPremiumLine(PolicyPremiumLine policyPremiumLine)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "UPDATE PolicyPremiumsLines SET PolicyPremiumsID = @PolicyPremiumsID, " +
                             "ProductID = @PolicyProductID, Premium = @Premium, StatusID = @StatusID, " +
                             "StatusDate = @StatusDate WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", policyPremiumLine.ID);
                    command.Parameters.AddWithValue("@PolicyPremiumsID", policyPremiumLine.PolicyPremiumsID);
                    command.Parameters.AddWithValue("@PolicyProductID", policyPremiumLine.PolicyProductID);
                    command.Parameters.AddWithValue("@Premium", policyPremiumLine.Premium);
                    command.Parameters.AddWithValue("@StatusID", policyPremiumLine.StatusID);
                    command.Parameters.AddWithValue("@StatusDate", policyPremiumLine.StatusDate);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdatePolicyPremiumLines(Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "Policy_UpdatePremiumLines";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID",PolicyID); 
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdatePolicyPremiumLines(int PolicyPremiumID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "Policy_UpdatePremiumLinesByPolicyPremiumID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyPremiumID ", PolicyPremiumID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateMainPremiumLines(Guid PolicyID, Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "Policy_UpdateMainPremiumLines";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID ", PolicyID);
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeletePolicyPremiumLine(int policyPremiumLineId)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "DELETE FROM PolicyPremiumsLines WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", policyPremiumLineId);

                    command.ExecuteNonQuery();
                }
            }
        }
        private PolicyPremiumLine MapDataReaderToPolicyPremiumLine(SqlDataReader reader)
        {
            return new PolicyPremiumLine
            {
                ID = (int)reader["ID"],
                PolicyPremiumsID = (int)reader["PolicyPremiumsID"],
                PolicyProductID = Guid.Parse(reader["ProductID"].ToString()),
                Premium = (decimal)reader["Premium"],
                StatusID = (int)reader["StatusID"],
                StatusDate = (DateTime)reader["StatusDate"],
            };
        }
        public bool UpdatePolicyPremiumDates(int PolicyPremiumID, PolicyDates policyDates)
        { 
            string updateQuery = @"
            UPDATE PolicyPremiums
            SET   
             ProposedStartDate = @ProposedStartDate, 
             ClientSignedDate = @ClientSignedDate,
             AgentSignedDate = @AgentSignedDate,
             DateApplicationReceived = @DateApplicationReceived,
             DeductionStartDate=@DeductionStartDate,
             PreferredBillingDay=@PreferredBillingDay,
             SystemDate = @SystemDate
             WHERE ID=@PolicyPremiumID;";
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                { 
                    command.Parameters.AddWithValue("@PolicyPremiumID", PolicyPremiumID); 
                    command.Parameters.AddWithValue("@ProposedStartDate", (object)policyDates.ProposedStartDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PreferredBillingDay", policyDates.PreferredBillingDay);
                    command.Parameters.AddWithValue("@ClientSignedDate", (object)policyDates.ClientSignedDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AgentSignedDate", (object)policyDates.AgentSignedDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DateApplicationReceived", (object)policyDates.DateApplicationReceived ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DeductionStartDate", (object)policyDates.DeductionStartDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SystemDate", (object)policyDates.SystemDate ?? DBNull.Value);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
    }
}
