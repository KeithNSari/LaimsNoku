using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Membership
{
    public class LIRoleRepository: ILIRoleRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public LIRoleRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
        public List<LIRole> GetAllLIRoles()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<LIRole> lIRoles = new List<LIRole>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM LIRoles";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                           LIRole liRole = MapDataReaderToLIRole(reader);
                            lIRoles.Add(liRole);
                        }
                    }
                }
            }

            return lIRoles;
        }
        private LIRole MapDataReaderToLIRole(SqlDataReader reader)
        {
            return new LIRole
            {
                ID = Convert.ToInt32(reader["ID"]),
                RoleName = reader["Role"].ToString(),
                //AddedOn = reader["AddedOn"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["AddedOn"]),
                //AddedBy = reader["AddedBy"].ToString(),
                //Archived = reader["Archived"] is DBNull ? (bool?)null : Convert.ToBoolean(reader["Archived"]),
                //ArchivedBy = reader["ArchivedBy"].ToString(),
                //ArchivedComment = reader["ArchivedComment"].ToString(),
                //ArchivedOn = reader["ArchivedOn"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["ArchivedOn"]),
                //Deleted = reader["Deleted"] is DBNull ? (bool?)null : Convert.ToBoolean(reader["Deleted"]),
                //DeletedBy = reader["DeletedBy"].ToString(),
                //DeletedComment = reader["DeletedComment"].ToString(),
                //DeletedOn = reader["DeletedOn"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["DeletedOn"])
            };
        }
    }
}
