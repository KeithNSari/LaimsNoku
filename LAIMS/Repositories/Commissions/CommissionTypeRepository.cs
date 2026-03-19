using LAIMS.Models.Commissions;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Commissions
{
    public class CommissionTypeRepository
    {
        private readonly string connectionString;

        public CommissionTypeRepository(string connectionString)
        {
            this.connectionString = connectionString;
        } 
        public void AddCommissionType(CommissionType commissionType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "INSERT INTO CommissionTypes (Type) VALUES (@Type)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Type", commissionType.Type);

                    command.ExecuteNonQuery();
                }
            }
        } 
        public List<CommissionType> GetAllCommissionTypes()
        {
            List<CommissionType> commissionTypes = new List<CommissionType>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM CommissionTypes";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CommissionType commissionType = MapDataReaderToCommissionType(reader);
                            commissionTypes.Add(commissionType);
                        }
                    }
                }
            }

            return commissionTypes;
        } 
        public void UpdateCommissionType(CommissionType commissionType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "UPDATE CommissionTypes SET Type = @Type WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", commissionType.ID);
                    command.Parameters.AddWithValue("@Type", commissionType.Type);

                    command.ExecuteNonQuery();
                }
            }
        } 
        public void DeleteCommissionType(int commissionTypeId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM CommissionTypes WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", commissionTypeId);

                    command.ExecuteNonQuery();
                }
            }
        } 
        private CommissionType MapDataReaderToCommissionType(SqlDataReader reader)
        {
            return new CommissionType
            {
                ID = (int)reader["ID"],
                Type = (string)reader["Type"],
            };
        }
    }
}
