using LAIMS.Interfaces.Investments;
using LAIMS.Models.Investments;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Investments
{
    public class PolicyUnitsLinesRepository: IPolicyUnitsLinesRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PolicyUnitsLinesRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        // Create a new PolicyUnitsLines record
        public void Create(PolicyUnitsLines policyUnitsLines)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"INSERT INTO PolicyUnitsLines (PolicyUnitsID, UnitPricesListID, Units, TransactionTypeID, AddedBy, AddedOn)
                             VALUES (@PolicyUnitsID, @UnitPricesListID, @Units, @TransactionTypeID, @AddedBy, @AddedOn)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PolicyUnitsID", policyUnitsLines.PolicyUnitsID);
                    command.Parameters.AddWithValue("@UnitPricesListID", policyUnitsLines.UnitPricesListID);
                    command.Parameters.AddWithValue("@Units", policyUnitsLines.Units);
                    command.Parameters.AddWithValue("@TransactionTypeID", policyUnitsLines.TransactionTypeID);
                    command.Parameters.AddWithValue("@AddedBy", policyUnitsLines.AddedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", policyUnitsLines.AddedOn ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Read a PolicyUnitsLines record by ID
        public PolicyUnitsLines Read(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM PolicyUnitsLines WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapPolicyUnitsLinesFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        // Read all PolicyUnitsLines records
        public List<PolicyUnitsLines> ReadAll()
        {
            List<PolicyUnitsLines> policyUnitsLinesList = new List<PolicyUnitsLines>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM PolicyUnitsLines";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyUnitsLines policyUnitsLines = MapPolicyUnitsLinesFromReader(reader);
                            policyUnitsLinesList.Add(policyUnitsLines);
                        }
                    }
                }
            }
            return policyUnitsLinesList;
        }

        // Update a PolicyUnitsLines record
        public void Update(PolicyUnitsLines policyUnitsLines)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"UPDATE PolicyUnitsLines 
                             SET PolicyUnitsID = @PolicyUnitsID, UnitPricesListID = @UnitPricesListID, 
                                 Units = @Units, TransactionTypeID = @TransactionTypeID, 
                                 AddedBy = @AddedBy, AddedOn = @AddedOn
                             WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", policyUnitsLines.ID);
                    command.Parameters.AddWithValue("@PolicyUnitsID", policyUnitsLines.PolicyUnitsID);
                    command.Parameters.AddWithValue("@UnitPricesListID", policyUnitsLines.UnitPricesListID);
                    command.Parameters.AddWithValue("@Units", policyUnitsLines.Units);
                    command.Parameters.AddWithValue("@TransactionTypeID", policyUnitsLines.TransactionTypeID);
                    command.Parameters.AddWithValue("@AddedBy", policyUnitsLines.AddedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", policyUnitsLines.AddedOn ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void DebitPolicyUnits(Guid policyId, Guid UnitTrustID, int UnitPricesListID, decimal TransactionUnits, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyUnits_Debit";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    command.Parameters.AddWithValue("@PolicyID", policyId);
                    command.Parameters.AddWithValue("@UnitTrustID", UnitTrustID);
                    command.Parameters.AddWithValue("@UnitPricesListID", UnitPricesListID);
                    command.Parameters.AddWithValue("@TransactionUnits", TransactionUnits);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        // Delete a PolicyUnitsLines record
        public void Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM PolicyUnitsLines WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Helper method to map PolicyUnitsLines data from SqlDataReader to PolicyUnitsLines object
        private PolicyUnitsLines MapPolicyUnitsLinesFromReader(SqlDataReader reader)
        {
            return new PolicyUnitsLines
            {
                ID = (int)reader["ID"],
                PolicyUnitsID = (int)reader["PolicyUnitsID"],
                UnitPricesListID = (int)reader["UnitPricesListID"],
                Units = (decimal)reader["Units"],
                TransactionTypeID = (int)reader["TransactionTypeID"],
                AddedBy = reader["AddedBy"] is DBNull ? null : (string)reader["AddedBy"],
                AddedOn = reader["AddedOn"] is DBNull ? (DateTime?)null : (DateTime)reader["AddedOn"]
            };
        }
    }
}
