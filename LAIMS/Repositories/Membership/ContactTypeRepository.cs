using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.Data.SqlClient;
namespace LAIMS.Repositories.Membership
{     
        public class ContactTypeRepository : IContactTypeRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        string Database;
        public ContactTypeRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public void InsertContactType(ContactType contactType)
            {
                using (SqlConnection connection = new SqlConnection(Database))
                {
                    string query = "INSERT INTO ContactTypes (Type, AddedBy) VALUES (@Type, @AddedBy)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Type", contactType.Type);
                        command.Parameters.AddWithValue("@AddedBy", contactType.AddedBy);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }

            public List<ContactType> GetAllContactTypes()
            {
                List<ContactType> contactTypes = new List<ContactType>();

                using (SqlConnection connection = new SqlConnection(Database))
                {
                    string query = "SELECT ID, Type, AddedOn, AddedBy FROM ContactTypes Order By [Type] Asc";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ContactType contactType = new ContactType
                                {
                                    ID = (int)reader["ID"],
                                    Type = reader["Type"].ToString(),
                                    AddedOn = reader["AddedOn"] as DateTime?,
                                    AddedBy = reader["AddedBy"].ToString()
                                };
                                contactTypes.Add(contactType);
                            }
                        }
                    }
                }

                return contactTypes;
            }

            public void UpdateContactType(ContactType contactType)
            {
                using (SqlConnection connection = new SqlConnection(Database))
                {
                    string query = "UPDATE ContactTypes SET Type = @Type, AddedBy = @AddedBy WHERE ID = @ID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", contactType.ID);
                        command.Parameters.AddWithValue("@Type", contactType.Type);
                        command.Parameters.AddWithValue("@AddedBy", contactType.AddedBy);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }

            public void DeleteContactType(int contactTypeId)
            {
                using (SqlConnection connection = new SqlConnection(Database))
                {
                    string query = "DELETE FROM ContactTypes WHERE ID = @ID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", contactTypeId);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
}
     
