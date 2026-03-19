using LAIMS.Models.Commissions;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Commissions
{
    public class IntermediaryCommissionTypeRepository
    {
        private readonly string connectionString;

        public IntermediaryCommissionTypeRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public void AddIntermediaryCommissionType(IntermediaryCommissionType intermediaryCommissionType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "INSERT INTO IntermediaryCommissionTypes (CommissionTypeID, IntermediaryID) " +
                             "VALUES (@CommissionTypeID, @IntermediaryID)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CommissionTypeID", intermediaryCommissionType.CommissionTypeID);
                    command.Parameters.AddWithValue("@IntermediaryID", intermediaryCommissionType.IntermediaryID);

                    command.ExecuteNonQuery();
                }
            }
        }
        public List<IntermediaryCommissionType> GetAllIntermediaryCommissionTypes()
        {
            List<IntermediaryCommissionType> intermediaryCommissionTypes = new List<IntermediaryCommissionType>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM IntermediaryCommissionTypes";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            IntermediaryCommissionType intermediaryCommissionType = MapDataReaderToIntermediaryCommissionType(reader);
                            intermediaryCommissionTypes.Add(intermediaryCommissionType);
                        }
                    }
                }
            }

            return intermediaryCommissionTypes;
        }
        public void UpdateIntermediaryCommissionType(IntermediaryCommissionType intermediaryCommissionType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "UPDATE IntermediaryCommissionTypes SET CommissionTypeID = @CommissionTypeID, " +
                             "IntermediaryID = @IntermediaryID WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", intermediaryCommissionType.ID);
                    command.Parameters.AddWithValue("@CommissionTypeID", intermediaryCommissionType.CommissionTypeID);
                    command.Parameters.AddWithValue("@IntermediaryID", intermediaryCommissionType.IntermediaryID);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeleteIntermediaryCommissionType(int intermediaryCommissionTypeId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM IntermediaryCommissionTypes WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", intermediaryCommissionTypeId);

                    command.ExecuteNonQuery();
                }
            }
        }
        private IntermediaryCommissionType MapDataReaderToIntermediaryCommissionType(SqlDataReader reader)
        {
            return new IntermediaryCommissionType
            {
                ID = (int)reader["ID"],
                CommissionTypeID = (int)reader["CommissionTypeID"],
                IntermediaryID = (int)reader["IntermediaryID"],
            };
        }
    }
}
