
using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership; 
using Microsoft.Data.SqlClient;
namespace LAIMS.Repositories.Membership
{  

    public class CountryRepository: ICountryRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public CountryRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public List<Country> GetAllCountries()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<Country> countries = new List<Country>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM Countries", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Country country = MapDataToCountry(reader);
                            countries.Add(country);
                        }
                    }
                }
            }

            return countries;
        }

        public Country GetCountryById(int countryId)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM Countries WHERE CountryID = @CountryID", connection))
                {
                    command.Parameters.AddWithValue("@CountryID", countryId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataToCountry(reader);
                        }
                    }
                }
            }

            return null;
        }
        public Country GetCountryName(string countryName)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM Countries WHERE Country = @Country", connection))
                {
                    command.Parameters.AddWithValue("@Country", countryName);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataToCountry(reader);
                        }
                    }
                }
            }

            return null;
        }
        public void AddCountry(Country country)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(
                    "INSERT INTO Countries (Country, Code, NumericCode, Sequence, Visibility) " +
                    "VALUES (@Country, @Code, @NumericCode, @Sequence, @Visibility); " +
                    "SELECT SCOPE_IDENTITY();", connection))
                {
                    command.Parameters.AddWithValue("@Country", country.CountryName);
                    command.Parameters.AddWithValue("@Code", country.Code);
                    command.Parameters.AddWithValue("@NumericCode", country.NumericCode);
                    command.Parameters.AddWithValue("@Sequence", country.Sequence);
                    command.Parameters.AddWithValue("@Visibility", country.Visibility);

                    // ExecuteScalar is used to retrieve the identity value of the newly inserted record
                    int newCountryId = Convert.ToInt32(command.ExecuteScalar());
                    country.CountryID = newCountryId;
                }
            }
        }
        public int AddCity(int CountryID, string CityName, string AddedBy, DateTime AddedOn)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(
                    "Declare @CityID int=-10; SELECT @CityID=[ID] FROM [dbo].[Cities] WHERE [CountryID]=@CountryID And [City]=@CityName; IF(@CityID=-10) BEGIN Insert into [dbo].[Cities]([CountryID],[City],[AddedOn],[AddedBy]) VALUES (@CountryID,@CityName,@AddedOn,@AddedBy); SELECT @CityID=@@IDENTITY END; SELECT @CityID", connection))
                {
                    command.Parameters.AddWithValue("@CountryID", CountryID);
                    command.Parameters.AddWithValue("@CityName", CityName);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", AddedOn); 
                    // ExecuteScalar is used to retrieve the identity value of the newly inserted record
                    int newCityId = Convert.ToInt32(command.ExecuteScalar());
                    return newCityId; 
                }
            }
        }
        private Country MapDataToCountry(SqlDataReader reader)
        {
            return new Country
            {
                CountryID = Convert.ToInt32(reader["CountryID"]),
                CountryName = Convert.ToString(reader["Country"]),
                Code = Convert.ToString(reader["Code"]),
                NumericCode = Convert.ToString(reader["NumericCode"]),
                Sequence = Convert.ToInt32(reader["Sequence"]),
                Visibility = Convert.ToByte(reader["Visibility"])
            };
        }
    }

}
