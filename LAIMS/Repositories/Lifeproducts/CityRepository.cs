using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

public class CityRepository: ICityRepository
{
    private readonly string Database;
    private IConfiguration _configuration;
    private IWebHostEnvironment _environment;
    public CityRepository(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
        Database = _configuration.GetConnectionString("DefaultConnection");
    }

    public void InsertCity(City city)
    {
        using (SqlConnection connection = new SqlConnection(Database))
        {
            string query = @"
                    INSERT INTO Cities (CountryID, City, AddedBy)
                    VALUES (@CountryID, @City, @AddedBy);
                    SELECT SCOPE_IDENTITY();";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CountryID", city.CountryID);
                command.Parameters.AddWithValue("@City", city.CityName);
                command.Parameters.AddWithValue("@AddedBy", (object)city.AddedBy ?? DBNull.Value);
                connection.Open();
                city.ID = Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }

    public List<City> GetAllCities()
    {
        List<City> cities = new List<City>();
        using (SqlConnection connection = new SqlConnection(Database))
        {
            string query = "SELECT ID, CountryID, City, AddedOn, AddedBy FROM Cities";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        City city = new City
                        {
                            ID = (int)reader["ID"],
                            CountryID = (int)reader["CountryID"],
                            CityName = reader["City"].ToString(),
                            AddedOn = reader["AddedOn"] as DateTime?,
                            AddedBy = reader["AddedBy"] as string
                        };

                        cities.Add(city);
                    }
                }
            }
        }
        return cities;
    }
    public List<City> GetCitiesByCountry(int CountryID)
    {
        List<City> cities = new List<City>();
        using (SqlConnection connection = new SqlConnection(Database))
        {
            string query = "SELECT ID, CountryID, City, AddedOn, AddedBy FROM Cities WHERE CountryID=@CountryID";
          
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CountryID", CountryID);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        City city = new City
                        {
                            ID = (int)reader["ID"],
                            CountryID = (int)reader["CountryID"],
                            CityName = reader["City"].ToString(),
                            AddedOn = reader["AddedOn"] as DateTime?,
                            AddedBy = reader["AddedBy"] as string
                        };

                        cities.Add(city);
                    }
                }
            }
        }
        return cities;
    }
    public void UpdateCity(City city)
    {
        using (SqlConnection connection = new SqlConnection(Database))
        {
            string query = @"
                    UPDATE Cities
                    SET CountryID = @CountryID, City = @City, AddedBy = @AddedBy
                    WHERE ID = @ID";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ID", city.ID);
                command.Parameters.AddWithValue("@CountryID", city.CountryID);
                command.Parameters.AddWithValue("@City", city.CityName);
                command.Parameters.AddWithValue("@AddedBy", (object)city.AddedBy ?? DBNull.Value);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }

    public void DeleteCity(int cityId)
    {
        using (SqlConnection connection = new SqlConnection(Database))
        {
            string query = "DELETE FROM Cities WHERE ID = @ID";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ID", cityId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
} 