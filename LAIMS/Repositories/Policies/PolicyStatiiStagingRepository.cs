using Azure.Core;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.Policies; 
using Microsoft.Data.SqlClient;
using System.Data;
namespace LAIMS.Repositories.Policies
{ 

    public class PolicyStatiiStagingRepository : IPolicyStatiiStagingRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PolicyStatiiStagingRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public PolicyStatiiStaging GetById(Guid requestId)
        {
            using (var connection = new SqlConnection(Database))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT * FROM PolicyStatiiStaging WHERE RequestID = @RequestID", connection))
                {
                    command.Parameters.Add(new SqlParameter("@RequestID", requestId));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToPolicyStatiiStaging(reader);
                        }
                    }
                }
            }
            return null;
        }

        public List<PolicyStatiiStaging> GetAll()
        {
            var policies = new List<PolicyStatiiStaging>();

            using (var connection = new SqlConnection(Database))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT * FROM PolicyStatiiStaging", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            policies.Add(MapToPolicyStatiiStaging(reader));
                        }
                    }
                }
            }
            return policies;
        }

        public void Add(PolicyStatiiStaging policyStatiiStaging)
        {
            using (var connection = new SqlConnection(Database))
            {
                connection.Open();
                using (var command = new SqlCommand("DELETE FROM [dbo].[PolicyStatiiStaging] WHERE [RequestID]=@RequestID; INSERT INTO PolicyStatiiStaging (RequestID, PolicyID, PolicyStatus, PolicyStatusReason, PolicyStatusDate, PolicyStatusComment, AddedBy) VALUES (@RequestID, @PolicyID, @PolicyStatus, @PolicyStatusReason, @PolicyStatusDate, @PolicyStatusComment, @AddedBy)", connection))
                {
                    command.Parameters.Add(new SqlParameter("@RequestID", policyStatiiStaging.RequestID));
                    command.Parameters.Add(new SqlParameter("@PolicyID", policyStatiiStaging.PolicyID)); 
                    command.Parameters.Add(new SqlParameter("@PolicyStatus", (object)policyStatiiStaging.PolicyStatus ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@PolicyStatusReason", (object)policyStatiiStaging.PolicyStatusReason ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@PolicyStatusDate", (object)policyStatiiStaging.PolicyStatusDate ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@PolicyStatusComment", (object)policyStatiiStaging.PolicyStatusComment ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@AddedBy", policyStatiiStaging.AddedBy)); 

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(PolicyStatiiStaging policyStatiiStaging)
        {
            using (var connection = new SqlConnection(Database))
            {
                connection.Open();
                using (var command = new SqlCommand("UPDATE PolicyStatiiStaging SET PolicyID = @PolicyID, PolicyStage = @PolicyStage, PolicyStatusRuleID = @PolicyStatusRuleID, PolicyStatus = @PolicyStatus, PolicyStatusReason = @PolicyStatusReason, PolicyStatusDate = @PolicyStatusDate, PolicyStatusComment = @PolicyStatusComment, AddedBy = @AddedBy, AddedOn = @AddedOn, Approved = @Approved, ApprovedBy = @ApprovedBy, ApprovedOn = @ApprovedOn WHERE RequestID = @RequestID", connection))
                {
                    command.Parameters.Add(new SqlParameter("@RequestID", policyStatiiStaging.RequestID));
                    command.Parameters.Add(new SqlParameter("@PolicyID", policyStatiiStaging.PolicyID));
                    command.Parameters.Add(new SqlParameter("@PolicyStage", policyStatiiStaging.PolicyStage));
                    command.Parameters.Add(new SqlParameter("@PolicyStatusRuleID", (object)policyStatiiStaging.PolicyStatusRuleID ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@PolicyStatus", (object)policyStatiiStaging.PolicyStatus ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@PolicyStatusReason", (object)policyStatiiStaging.PolicyStatusReason ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@PolicyStatusDate", (object)policyStatiiStaging.PolicyStatusDate ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@PolicyStatusComment", (object)policyStatiiStaging.PolicyStatusComment ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@AddedBy", policyStatiiStaging.AddedBy));
                    command.Parameters.Add(new SqlParameter("@AddedOn", (object)policyStatiiStaging.AddedOn ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Approved", (object)policyStatiiStaging.Approved ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@ApprovedBy", (object)policyStatiiStaging.ApprovedBy ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@ApprovedOn", (object)policyStatiiStaging.ApprovedOn ?? DBNull.Value));

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid requestId)
        {
            using (var connection = new SqlConnection(Database))
            {
                connection.Open();
                using (var command = new SqlCommand("DELETE FROM PolicyStatiiStaging WHERE RequestID = @RequestID", connection))
                {
                    command.Parameters.Add(new SqlParameter("@RequestID", requestId));
                    command.ExecuteNonQuery();
                }
            }
        }

        public void ExecuteStoredProcedure(string procedureName)
        {
            using (var connection = new SqlConnection(Database))
            {
                connection.Open();
                using (var command = new SqlCommand(procedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SetApproved(Guid requestId, byte approved, string approvedBy)
        {
            using (var connection = new SqlConnection(Database))
            {
                connection.Open();
                using (var command = new SqlCommand("UPDATE PolicyStatiiStaging SET Approved = @Approved, ApprovedBy = @ApprovedBy, ApprovedOn = @ApprovedOn WHERE RequestID = @RequestID", connection))
                {
                    command.Parameters.Add(new SqlParameter("@RequestID", requestId));
                    command.Parameters.Add(new SqlParameter("@Approved", approved));
                    command.Parameters.Add(new SqlParameter("@ApprovedBy", approvedBy));
                    command.Parameters.Add(new SqlParameter("@ApprovedOn", DateTime.Now));

                    command.ExecuteNonQuery();
                }
            }
        }

        private PolicyStatiiStaging MapToPolicyStatiiStaging(IDataReader reader)
        {
            return new PolicyStatiiStaging
            {
                RequestID = (Guid)reader["RequestID"],
                PolicyID = (Guid)reader["PolicyID"],
                PolicyStage = (int)reader["PolicyStage"],
                PolicyStatusRuleID = reader["PolicyStatusRuleID"] as Guid?,
                PolicyStatus = reader["PolicyStatus"] as int?,
                PolicyStatusReason = reader["PolicyStatusReason"] as int?,
                PolicyStatusDate = reader["PolicyStatusDate"] as DateTime?,
                PolicyStatusComment = reader["PolicyStatusComment"] as string,
                AddedBy = (string)reader["AddedBy"],
                AddedOn = reader["AddedOn"] as DateTime?,
                Approved = reader["Approved"] as byte?,
                ApprovedBy = reader["ApprovedBy"] as string,
                ApprovedOn = reader["ApprovedOn"] as DateTime?
            };
        }
        public DataTable GetStatusHistory(Guid RequestID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PolicyStatiiStaging_GetByRequestID";
            command.Parameters.AddWithValue("@RequestID", RequestID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public bool UpdatePolicyStatus(Guid RequestID,string ApprovedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyStatiiStaging_Approve";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    command.Parameters.AddWithValue("@ApprovedBy", ApprovedBy);
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
    }

}
