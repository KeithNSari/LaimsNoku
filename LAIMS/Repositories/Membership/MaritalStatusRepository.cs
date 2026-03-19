using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Membership
{
    public class MaritalStatusRepository: IMaritalStatusRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public MaritalStatusRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public List<MaritalStatus> GetAllMaritalStatuses()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<MaritalStatus> maritalStatuses = new List<MaritalStatus>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM MaritalStatii", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MaritalStatus maritalStatus = MapDataToMaritalStatus(reader);
                            maritalStatuses.Add(maritalStatus);
                        }
                    }
                }
            }

            return maritalStatuses;
        }

        public MaritalStatus GetMaritalStatusById(int maritalStatusId)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM MaritalStatii WHERE MaritalStatusID = @MaritalStatusID", connection))
                {
                    command.Parameters.AddWithValue("@MaritalStatusID", maritalStatusId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataToMaritalStatus(reader);
                        }
                    }
                }
            }

            return null;
        }

        public void AddMaritalStatus(MaritalStatus maritalStatus)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(
                    "INSERT INTO MaritalStatii (MaritalStatus) " +
                    "VALUES (@MaritalStatus); " +
                    "SELECT SCOPE_IDENTITY();", connection))
                {
                    command.Parameters.AddWithValue("@MaritalStatus", maritalStatus.MaritalStatusName);

                    int newMaritalStatusId = Convert.ToInt32(command.ExecuteScalar());
                    maritalStatus.MaritalStatusID = newMaritalStatusId;
                }
            }
        }

        public void UpdateMaritalStatus(MaritalStatus maritalStatus)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(
                    "UPDATE MaritalStatii SET MaritalStatus = @MaritalStatus WHERE MaritalStatusID = @MaritalStatusID", connection))
                {
                    command.Parameters.AddWithValue("@MaritalStatusID", maritalStatus.MaritalStatusID);
                    command.Parameters.AddWithValue("@MaritalStatus", maritalStatus.MaritalStatusName);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteMaritalStatus(int maritalStatusId)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("DELETE FROM MaritalStatii WHERE MaritalStatusID = @MaritalStatusID", connection))
                {
                    command.Parameters.AddWithValue("@MaritalStatusID", maritalStatusId);

                    command.ExecuteNonQuery();
                }
            }
        }

        private MaritalStatus MapDataToMaritalStatus(SqlDataReader reader)
        {
            return new MaritalStatus
            {
                MaritalStatusID = Convert.ToInt32(reader["MaritalStatusID"]),
                MaritalStatusName = Convert.ToString(reader["MaritalStatus"])
            };
        }
    }

}
