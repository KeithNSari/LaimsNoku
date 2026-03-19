using LAIMS.Interfaces.Commissions;
using LAIMS.Models.Commissions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Commissions
{
    public class PolicyTypeCommissionRepository: IPolicyTypeCommissionRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PolicyTypeCommissionRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public List<PolicyTypeCommission> GetAllPolicyTypeCommissions()
        {
            List<PolicyTypeCommission> policyTypeCommissions = new List<PolicyTypeCommission>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM PolicyTypeCommissions";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyTypeCommission policyTypeCommission = MapReaderToPolicyTypeCommission(reader);
                            policyTypeCommissions.Add(policyTypeCommission);
                        }
                    }
                }
            }

            return policyTypeCommissions;
        }
        public List<PolicyTypeCommission> GetPolicyTypeCommissions(Guid PolicyTypeID, Guid ProductID, int IntermediaryTypeID)
        {
            List<PolicyTypeCommission> policyTypeCommissions = new List<PolicyTypeCommission>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "PolicyTypeCommissions_GetPPIP";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("PolicyTypeID", PolicyTypeID);
                    command.Parameters.AddWithValue("ProductID", ProductID);
                    command.Parameters.AddWithValue("IntermediaryTypeID", IntermediaryTypeID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyTypeCommission policyTypeCommission = MapReaderToPolicyTypeCommission(reader);
                            policyTypeCommissions.Add(policyTypeCommission);
                        }
                    }
                }
            }

            return policyTypeCommissions;
        }
        public PolicyTypeCommission GetPolicyTypeCommissionById(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM PolicyTypeCommissions WHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToPolicyTypeCommission(reader);
                        }
                    }
                }
            }

            return null;
        }
        public DataTable Get()
        { 
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyTypeCommissions_Get"; 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }

        public void AddPolicyTypeCommission(PolicyTypeCommission policyTypeCommission)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DECLARE @Exists int=0; SELECT @Exists=Count(*) FROM [dbo].[PolicyTypeCommissions] WHERE [IntermediaryTypeID]=@IntermediaryTypeID AND [PolicyTypeID]=@PolicyTypeID AND [ProductID]=@ProductID AND [Calculation]=@Calculation AND [CommissionRate]=@CommissionRate AND [CPPStarts]=@CPPStarts AND [CPPEnds]=@CPPEnds; IF(@Exists=0) BEGIN INSERT INTO PolicyTypeCommissions(IntermediaryTypeID, PolicyTypeID, ProductID, FunctionType,Calculation, CommissionRate, CPPStarts, CPPEnds) VALUES (@IntermediaryTypeID, @PolicyTypeID, @ProductID, @FunctionType,@Calculation, @CommissionRate, @CPPStarts, @CPPEnds) END";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    MapPolicyTypeCommissionToParameters(policyTypeCommission, command);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdatePolicyTypeCommission(PolicyTypeCommission policyTypeCommission)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "UPDATE PolicyTypeCommissions SET IntermediaryTypeID = @IntermediaryTypeID, PolicyTypeID = @PolicyTypeID, ProductID = @ProductID, FunctionType = @FunctionType, FunctionName = @FunctionName, CommissionRate = @CommissionRate, CPPStarts = @CPPStarts, CPPEnds = @CPPEnds WHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", policyTypeCommission.ID);
                    MapPolicyTypeCommissionToParameters(policyTypeCommission, command);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeletePolicyTypeCommission(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM PolicyTypeCommissions WHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        private PolicyTypeCommission MapReaderToPolicyTypeCommission(SqlDataReader reader)
        {
            return new PolicyTypeCommission
            {
                ID = (int)reader["ID"],
                IntermediaryTypeID = (int)reader["IntermediaryTypeID"],
                PolicyTypeID = reader["PolicyTypeID"] == DBNull.Value ? (Guid?)null : (Guid)reader["PolicyTypeID"],
                ProductID = (Guid)reader["ProductID"],
                FunctionType = (byte)reader["FunctionType"],
                Calculation =  (string)reader["Calculation"],
                FunctionName = reader["FunctionName"] == DBNull.Value ? null : reader["FunctionName"].ToString(),
                CommissionRate = (decimal)reader["CommissionRate"],
                CPPStarts = (int)reader["CPPStarts"],
                CPPEnds = (int)reader["CPPEnds"]
            };
        }

        private void MapPolicyTypeCommissionToParameters(PolicyTypeCommission policyTypeCommission, SqlCommand command)
        {
            command.Parameters.AddWithValue("@IntermediaryTypeID", policyTypeCommission.IntermediaryTypeID);
            command.Parameters.AddWithValue("@PolicyTypeID", policyTypeCommission.PolicyTypeID ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ProductID", policyTypeCommission.ProductID);
            command.Parameters.AddWithValue("@FunctionType", policyTypeCommission.FunctionType);
            command.Parameters.AddWithValue("@Calculation", policyTypeCommission.Calculation);
            command.Parameters.AddWithValue("@FunctionName", policyTypeCommission.FunctionName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CommissionRate", policyTypeCommission.CommissionRate);
            command.Parameters.AddWithValue("@CPPStarts", policyTypeCommission.CPPStarts);
            command.Parameters.AddWithValue("@CPPEnds", policyTypeCommission.CPPEnds);
        }
    }
} 
