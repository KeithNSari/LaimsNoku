using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Premiums
{
    public class PaymentMethodRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PaymentMethodRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public void Insert(PaymentMethod paymentMethod)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "INSERT INTO PaymentMethods (Method, Automate, AddedBy, AddedOn, Archived, ArchivedBy, ArchivedOn) " +
                               "VALUES (@Method, @Automate, @AddedBy, @AddedOn, @Archived, @ArchivedBy, @ArchivedOn)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Method", paymentMethod.Method);
                    command.Parameters.AddWithValue("@Automate", paymentMethod.Automate);
                    command.Parameters.AddWithValue("@AddedBy", paymentMethod.AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", paymentMethod.AddedOn);
                    command.Parameters.AddWithValue("@Archived", paymentMethod.Archived);
                    command.Parameters.AddWithValue("@ArchivedBy", paymentMethod.ArchivedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ArchivedOn", paymentMethod.ArchivedOn ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        public PaymentMethod GetById(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM PaymentMethods WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

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

        public List<PaymentMethod> GetAll()
        {
            List<PaymentMethod> result = new List<PaymentMethod>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM PaymentMethods";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PaymentMethod paymentMethod = MapFromReader(reader);
                            result.Add(paymentMethod);
                        }
                    }
                }
            }
            return result;
        }

        public void Update(PaymentMethod paymentMethod)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "UPDATE PaymentMethods SET Method = @Method, Automate = @Automate, " +
                               "AddedBy = @AddedBy, AddedOn = @AddedOn, Archived = @Archived, " +
                               "ArchivedBy = @ArchivedBy, ArchivedOn = @ArchivedOn WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", paymentMethod.ID);
                    command.Parameters.AddWithValue("@Method", paymentMethod.Method);
                    command.Parameters.AddWithValue("@Automate", paymentMethod.Automate);
                    command.Parameters.AddWithValue("@AddedBy", paymentMethod.AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", paymentMethod.AddedOn);
                    command.Parameters.AddWithValue("@Archived", paymentMethod.Archived);
                    command.Parameters.AddWithValue("@ArchivedBy", paymentMethod.ArchivedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ArchivedOn", paymentMethod.ArchivedOn ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM PaymentMethods WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        private PaymentMethod MapFromReader(SqlDataReader reader)
        {
            return new PaymentMethod
            {
                ID = (int)reader["ID"],
                Method = (string)reader["Method"],
                Automate = (byte)reader["Automate"],
                AddedBy = (string)reader["AddedBy"],
                AddedOn = (DateTime)reader["AddedOn"],
                Archived = (byte)reader["Archived"],
                ArchivedBy = reader["ArchivedBy"] is DBNull ? null : (string)reader["ArchivedBy"],
                ArchivedOn = reader["ArchivedOn"] is DBNull ? null : (DateTime?)reader["ArchivedOn"]
            };
        }
    }
}
