using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.Data.SqlClient;
namespace LAIMS.Repositories.Membership
{ 
    public class GenderRepository: IGenderRepository
    {

        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public GenderRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public List<Gender> GetAllGenders()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<Gender> genders = new List<Gender>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM Genders", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Gender gender = MapDataToGender(reader);
                            genders.Add(gender);
                        }
                    }
                }
            }

            return genders;
        }

        public Gender GetGenderById(int id)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT * FROM Genders WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataToGender(reader);
                        }
                    }
                }
            }

            return null;
        }

        public void AddGender(Gender gender)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(
                    "INSERT INTO Genders (Name) " +
                    "VALUES (@Name); " +
                    "SELECT SCOPE_IDENTITY();", connection))
                {
                    command.Parameters.AddWithValue("@Name", gender.GenderName);

                    int newGenderId = Convert.ToInt32(command.ExecuteScalar());
                    gender.Id = newGenderId;
                }
            }
        }

        public void UpdateGender(Gender gender)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(
                    "UPDATE Genders SET Name = @Name WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", gender.Id);
                    command.Parameters.AddWithValue("@Name", gender.GenderName);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteGender(int id)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("DELETE FROM Genders WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        private Gender MapDataToGender(SqlDataReader reader)
        {
            return new Gender
            {
                Id = Convert.ToInt32(reader["Id"]),
                GenderName = Convert.ToString(reader["Name"])
            };
        }
    }


}
