using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.Data.SqlClient;
namespace LAIMS.Repositories.Membership
{    
public class TitleRepository: ITitleRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public TitleRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public List<Title> GetAllTitles()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<Title> titles = new List<Title>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM Titles", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Title title = MapDataToTitle(reader);
                            titles.Add(title);
                        }
                    }
                }
            }

            return titles;
        }

        public Title GetTitleById(int titleId)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM Titles WHERE TitleID = @TitleID", connection))
                {
                    command.Parameters.AddWithValue("@TitleID", titleId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataToTitle(reader);
                        }
                    }
                }
            }

            return null;
        }

        public void AddTitle(Title title)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(
                    "INSERT INTO Titles (Title) " +
                    "VALUES (@Title); " +
                    "SELECT SCOPE_IDENTITY();", connection))
                {
                    command.Parameters.AddWithValue("@Title", title.TitleName);

                    int newTitleId = Convert.ToInt32(command.ExecuteScalar());
                    title.TitleID = newTitleId;
                }
            }
        }

        public void UpdateTitle(Title title)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(
                    "UPDATE Titles SET Title = @Title WHERE TitleID = @TitleID", connection))
                {
                    command.Parameters.AddWithValue("@TitleID", title.TitleID);
                    command.Parameters.AddWithValue("@Title", title.TitleName);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteTitle(int titleId)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("DELETE FROM Titles WHERE TitleID = @TitleID", connection))
                {
                    command.Parameters.AddWithValue("@TitleID", titleId);

                    command.ExecuteNonQuery();
                }
            }
        }

        private Title MapDataToTitle(SqlDataReader reader)
        {
            return new Title
            {
                TitleID = Convert.ToInt32(reader["TitleID"]),
                TitleName = Convert.ToString(reader["Title"])
            };
        }
    }


}
