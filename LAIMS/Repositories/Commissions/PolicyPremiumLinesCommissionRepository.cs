using LAIMS.Interfaces.Commissions;
using LAIMS.Models.Commissions;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Commissions
{
    public class PolicyPremiumLinesCommissionRepository: IPolicyPremiumLinesCommissionRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PolicyPremiumLinesCommissionRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public void AddPolicyPremiumLinesCommission(PolicyPremiumLinesCommission policyPremiumLinesCommission)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "INSERT INTO PolicyPremiumLinesCommission (PolicyPremiumsLinesID, IntermediaryCommissionTypeID, ProductCommissionTypeID, StatusID, StatusDate, Commission) " +
                             "VALUES (@PolicyPremiumsLinesID, @IntermediaryCommissionTypeID, @ProductCommissionTypeID, @StatusID, @StatusDate, @Commission)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyPremiumsLinesID", policyPremiumLinesCommission.PolicyPremiumsLinesID);
                    command.Parameters.AddWithValue("@IntermediaryCommissionTypeID", policyPremiumLinesCommission.IntermediaryCommissionTypeID);
                    command.Parameters.AddWithValue("@ProductCommissionTypeID", policyPremiumLinesCommission.ProductCommissionTypeID);
                    command.Parameters.AddWithValue("@StatusID", policyPremiumLinesCommission.StatusID);
                    command.Parameters.AddWithValue("@StatusDate", policyPremiumLinesCommission.StatusDate);
                    command.Parameters.AddWithValue("@Commission", policyPremiumLinesCommission.Commission);

                    command.ExecuteNonQuery();
                }
            }
        } 
        public List<PolicyPremiumLinesCommission> GetAllPolicyPremiumLinesCommissions()
        {
            List<PolicyPremiumLinesCommission> policyPremiumLinesCommissions = new List<PolicyPremiumLinesCommission>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "SELECT * FROM PolicyPremiumLinesCommission";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyPremiumLinesCommission policyPremiumLinesCommission = MapDataReaderToPolicyPremiumLinesCommission(reader);
                            policyPremiumLinesCommissions.Add(policyPremiumLinesCommission);
                        }
                    }
                }
            }

            return policyPremiumLinesCommissions;
        } 

        public void UpdatePolicyPremiumLinesCommission(PolicyPremiumLinesCommission policyPremiumLinesCommission)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "UPDATE PolicyPremiumLinesCommission SET PolicyPremiumsLinesID = @PolicyPremiumsLinesID, " +
                             "IntermediaryCommissionTypeID = @IntermediaryCommissionTypeID, ProductCommissionTypeID = @ProductCommissionTypeID, " +
                             "StatusID = @StatusID, StatusDate = @StatusDate, Commission = @Commission WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", policyPremiumLinesCommission.ID);
                    command.Parameters.AddWithValue("@PolicyPremiumsLinesID", policyPremiumLinesCommission.PolicyPremiumsLinesID);
                    command.Parameters.AddWithValue("@IntermediaryCommissionTypeID", policyPremiumLinesCommission.IntermediaryCommissionTypeID);
                    command.Parameters.AddWithValue("@ProductCommissionTypeID", policyPremiumLinesCommission.ProductCommissionTypeID);
                    command.Parameters.AddWithValue("@StatusID", policyPremiumLinesCommission.StatusID);
                    command.Parameters.AddWithValue("@StatusDate", policyPremiumLinesCommission.StatusDate);
                    command.Parameters.AddWithValue("@Commission", policyPremiumLinesCommission.Commission);

                    command.ExecuteNonQuery();
                }
            }
        } 
        public void DeletePolicyPremiumLinesCommission(int policyPremiumLinesCommissionId)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "DELETE FROM PolicyPremiumLinesCommission WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", policyPremiumLinesCommissionId);

                    command.ExecuteNonQuery();
                }
            }
        } 
        private PolicyPremiumLinesCommission MapDataReaderToPolicyPremiumLinesCommission(SqlDataReader reader)
        {
            return new PolicyPremiumLinesCommission
            {
                ID = (int)reader["ID"],
                PolicyPremiumsLinesID = (int)reader["PolicyPremiumsLinesID"],
                IntermediaryCommissionTypeID = (int)reader["IntermediaryCommissionTypeID"],
                ProductCommissionTypeID = (int)reader["ProductCommissionTypeID"],
                StatusID = (int)reader["StatusID"],
                StatusDate = (DateTime)reader["StatusDate"],
                Commission = (decimal)reader["Commission"],
            };
        }
    }
}
