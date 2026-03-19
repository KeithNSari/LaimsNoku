using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Premiums
{
    public class IntermediaryTypeRepository: IIntermediaryTypeRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public IntermediaryTypeRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        // CREATE
        public void AddIntermediaryType(IntermediaryType intermediaryType)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "INSERT INTO IntermediaryTypes (Type) VALUES (@Type)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Type", intermediaryType.Type);

                    command.ExecuteNonQuery();
                }
            }
        }

        // READ
        public List<IntermediaryType> GetAllIntermediaryTypes()
        {
            List<IntermediaryType> intermediaryTypes = new List<IntermediaryType>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "SELECT * FROM IntermediaryTypes";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            IntermediaryType intermediaryType = MapDataReaderToIntermediaryType(reader);
                            intermediaryTypes.Add(intermediaryType);
                        }
                    }
                }
            }

            return intermediaryTypes;
        }

        public int GetTypeID(string TypeName)
        {
            int ID = 0;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @ID int =0; SELECT @ID=[ID] FROM [dbo].[IntermediaryTypes] WHERE [Type]=@Type; SELECT @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Type", TypeName);
                    ID=Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return ID;
        }

        // UPDATE
        public void UpdateIntermediaryType(IntermediaryType intermediaryType)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "UPDATE IntermediaryTypes SET Type = @Type WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", intermediaryType.ID);
                    command.Parameters.AddWithValue("@Type", intermediaryType.Type);

                    command.ExecuteNonQuery();
                }
            }
        }

        // DELETE
        public void DeleteIntermediaryType(int intermediaryTypeId)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "DELETE FROM IntermediaryTypes WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", intermediaryTypeId);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Helper method to map SqlDataReader to IntermediaryType object
        private IntermediaryType MapDataReaderToIntermediaryType(SqlDataReader reader)
        {
            return new IntermediaryType
            {
                ID = (int)reader["ID"],
                Type = (string)reader["Type"],
            };
        }
    }
}
