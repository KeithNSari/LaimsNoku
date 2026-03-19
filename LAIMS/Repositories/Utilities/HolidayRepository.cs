using LAIMS.Models.Utilities;
using Microsoft.Data.SqlClient;
using LAIMS.Interfaces.Utilities;

namespace LAIMS.Repositories.Utilities
{
    public class HolidayRepository: IHolidayRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public HolidayRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public void Create(Holiday holiday)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "INSERT INTO Holidays (HolidayName, HolidayDate, Year,NextBillingDate) " +
                               "VALUES (@HolidayName, @HolidayDate, @Year,@NextBillingDate)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, holiday);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Holiday Read(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM Holidays WHERE HolidayID = @HolidayID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HolidayID", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapFromReader(reader);
                        }
                        return null;
                    }
                }
            }
        }
        public List<Holiday> GetAllHolidays(int year)
        {
            List<Holiday> holidays = new List<Holiday>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM Holidays WHERE [Year]=@Year ORDER BY [HolidayDate] ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Year", year);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Holiday holiday = MapFromReader(reader);
                            holidays.Add(holiday);
                        } 
                    }
                }
            }
            return holidays;
        }
        public void Update(Holiday holiday)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE Holidays " +
                               "SET HolidayName = @HolidayName, HolidayDate = @HolidayDate, Year = @Year " +
                               "WHERE HolidayID = @HolidayID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, holiday);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM Holidays WHERE HolidayID = @HolidayID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HolidayID", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SetParameters(SqlCommand command, Holiday holiday)
        {
            command.Parameters.AddWithValue("@HolidayName", holiday.HolidayName);
            command.Parameters.AddWithValue("@HolidayDate", holiday.HolidayDate);
            command.Parameters.AddWithValue("@Year", holiday.Year);
            command.Parameters.AddWithValue("@HolidayID", holiday.HolidayID);
            command.Parameters.AddWithValue("@NextBillingDate", holiday.NextBillingDate);
        }

        private Holiday MapFromReader(SqlDataReader reader)
        {
            return new Holiday
            {
                HolidayID = (int)reader["HolidayID"],
                HolidayName = reader["HolidayName"].ToString(),
                HolidayDate = (DateTime)reader["HolidayDate"],
                Year = (int)reader["Year"],
                NextBillingDate = (DateTime)reader["NextBillingDate"]
            };
        }
    }
}
