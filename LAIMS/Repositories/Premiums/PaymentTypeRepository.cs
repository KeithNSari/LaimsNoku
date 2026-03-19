using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Premiums
{
    public class PaymentTypeRepository: IPaymentTypeRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public PaymentTypeRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public List<PaymentType> GetAllPaymentTypes()
        {
            List<PaymentType> paymentTypes = new List<PaymentType>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "SELECT ID, [Type] FROM PaymentTypes ORDER BY [Type] ASC ";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PaymentType paymentType = new PaymentType
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            TypeName = reader["Type"].ToString()
                        };
                        paymentTypes.Add(paymentType);
                    }
                }
            }

            return paymentTypes;
        }

        public PaymentType GetPaymentTypeById(int id)
        {
            PaymentType paymentType = null;

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "SELECT ID, [Type] FROM PaymentTypes WHERE ID = @ID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", id);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        paymentType = new PaymentType
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            TypeName = reader["Type"].ToString()
                        };
                    }
                }
            }

            return paymentType;
        }

        public void AddPaymentType(PaymentType paymentType)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "INSERT INTO PaymentTypes ([Type]) VALUES (@Type)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Type", paymentType.TypeName);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdatePaymentType(PaymentType paymentType)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "UPDATE PaymentTypes SET [Type] = @Type WHERE ID = @ID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Type", paymentType.TypeName);
                command.Parameters.AddWithValue("@ID", paymentType.ID);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeletePaymentType(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "DELETE FROM PaymentTypes WHERE ID = @ID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }       
    }
}
