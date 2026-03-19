using LAIMS.Interfaces.Investments;
using LAIMS.Models.Investments;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Investments
{
    public class PolicyUnitsRepository: IPolicyUnitsRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PolicyUnitsRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        // Create a new PolicyUnits record
        public void Create(PolicyUnit policyUnits)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"INSERT INTO PolicyUnits (PolicyID, UnitTrustID, TotalUnits, LastUpdated)
                             VALUES (@PolicyID, @UnitTrustID, @TotalUnits, @LastUpdated)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PolicyID", policyUnits.PolicyID);
                    command.Parameters.AddWithValue("@UnitTrustID", policyUnits.UnitTrustID);
                    command.Parameters.AddWithValue("@TotalUnits", policyUnits.TotalUnits);
                    command.Parameters.AddWithValue("@LastUpdated", policyUnits.LastUpdated);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Read a PolicyUnits record by ID
        public PolicyUnit Read(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM PolicyUnits WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapPolicyUnitsFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }
        public List<PolicyUnit> GetByPolicy(Guid PolicyID)
        {
            List<PolicyUnit> policyUnitsList = new List<PolicyUnit>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyUnits_Get";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyUnit policyUnits = MapPolicyUnitsFromReader(reader);
                            policyUnitsList.Add(policyUnits);
                        }
                    }
                }
            }
            return policyUnitsList;
        }
        // Read all PolicyUnits records
        public List<PolicyUnit> ReadAll()
        {
            List<PolicyUnit> policyUnitsList = new List<PolicyUnit>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM PolicyUnits";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyUnit policyUnits = MapPolicyUnitsFromReader(reader);
                            policyUnitsList.Add(policyUnits);
                        }
                    }
                }
            }

            return policyUnitsList;
        }

        // Update a PolicyUnits record
        public void Update(PolicyUnit policyUnits)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"UPDATE PolicyUnits 
                             SET PolicyID = @PolicyID, UnitTrustID = @UnitTrustID, 
                                 TotalUnits = @TotalUnits, LastUpdated = @LastUpdated
                             WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", policyUnits.ID);
                    command.Parameters.AddWithValue("@PolicyID", policyUnits.PolicyID);
                    command.Parameters.AddWithValue("@UnitTrustID", policyUnits.UnitTrustID);
                    command.Parameters.AddWithValue("@TotalUnits", policyUnits.TotalUnits);
                    command.Parameters.AddWithValue("@LastUpdated", policyUnits.LastUpdated);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Delete a PolicyUnits record
        public void Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM PolicyUnits WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }
         
        private PolicyUnit MapPolicyUnitsFromReader(SqlDataReader reader)
        {
            return new PolicyUnit
            {
                ID = (int)reader["ID"],
                PolicyID = (Guid)reader["PolicyID"],
                UnitTrust= reader["UnitTrust"] == DBNull.Value ? null : (string)reader["UnitTrust"],
                UnitTrustID = (Guid)reader["UnitTrustID"],
                TotalUnits = (decimal)reader["TotalUnits"],
                UnitPricesListID = (int)reader["UnitPricesListID"],
                OfferPrice = (decimal)reader["OfferPrice"],
                BidPrice = (decimal)reader["BidPrice"],
                LastUpdated = (DateTime)reader["LastUpdated"]
            };
        }
    }
}
