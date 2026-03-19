using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using Microsoft.Data.SqlClient;
namespace LAIMS.Repositories.Lifeproducts
{ 
    public class ExpenseTypeRepository: IExpenseTypeRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public ExpenseTypeRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

        }
        public List<ExpenseType> GetAllExpenseTypes()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<ExpenseType> expenseTypes = new List<ExpenseType>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM ExpenseTypes Where [Configurable]=1";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ExpenseType expenseType = MapDataReaderToExpenseType(reader);
                            expenseTypes.Add(expenseType);
                        }
                    }
                }
            }

            return expenseTypes;
        }

        public ExpenseType GetExpenseTypeById(int id)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM ExpenseTypes WHERE ID = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataReaderToExpenseType(reader);
                        }
                    }
                }
            }

            return null;
        }

        public void AddExpenseType(ExpenseType expenseType)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "INSERT INTO ExpenseTypes (Type, Description, AddedOn, AddedBy, Archived, ArchivedBy, ArchivedComment, ArchivedOn, Deleted, DeletedBy, DeletedComment, DeletedOn) " +
                               "VALUES (@Type, @Description, @AddedOn, @AddedBy, @Archived, @ArchivedBy, @ArchivedComment, @ArchivedOn, @Deleted, @DeletedBy, @DeletedComment, @DeletedOn)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    MapExpenseTypeToParameters(expenseType, command);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateExpenseType(ExpenseType expenseType)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "UPDATE ExpenseTypes SET Type = @Type, Description = @Description, " +
                               "AddedOn = @AddedOn, AddedBy = @AddedBy, Archived = @Archived, ArchivedBy = @ArchivedBy, ArchivedComment = @ArchivedComment, " +
                               "ArchivedOn = @ArchivedOn, Deleted = @Deleted, DeletedBy = @DeletedBy, DeletedComment = @DeletedComment, DeletedOn = @DeletedOn " +
                               "WHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    MapExpenseTypeToParameters(expenseType, command);
                    command.Parameters.AddWithValue("@Id", expenseType.ID);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteExpenseType(int id)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM ExpenseTypes WHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        private ExpenseType MapDataReaderToExpenseType(SqlDataReader reader)
        {
            return new ExpenseType
            {
                ID = Convert.ToInt32(reader["ID"]),
                Type = reader["Type"].ToString(),
                Description = reader["Description"].ToString(),
                AddedOn = reader["AddedOn"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["AddedOn"]),
                AddedBy = reader["AddedBy"].ToString(),
                Archived = reader["Archived"] is DBNull ? (bool?)null : Convert.ToBoolean(reader["Archived"]),
                ArchivedBy = reader["ArchivedBy"].ToString(),
                ArchivedComment = reader["ArchivedComment"].ToString(),
                ArchivedOn = reader["ArchivedOn"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["ArchivedOn"]),
                Deleted = reader["Deleted"] is DBNull ? (bool?)null : Convert.ToBoolean(reader["Deleted"]),
                DeletedBy = reader["DeletedBy"].ToString(),
                DeletedComment = reader["DeletedComment"].ToString(),
                DeletedOn = reader["DeletedOn"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["DeletedOn"])
            };
        }

        private void MapExpenseTypeToParameters(ExpenseType expenseType, SqlCommand command)
        {
            command.Parameters.AddWithValue("@Type", expenseType.Type);
            command.Parameters.AddWithValue("@Description", expenseType.Description);
            command.Parameters.AddWithValue("@AddedOn", expenseType.AddedOn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@AddedBy", expenseType.AddedBy);
            command.Parameters.AddWithValue("@Archived", expenseType.Archived ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedBy", expenseType.ArchivedBy);
            command.Parameters.AddWithValue("@ArchivedComment", expenseType.ArchivedComment);
            command.Parameters.AddWithValue("@ArchivedOn", expenseType.ArchivedOn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Deleted", expenseType.Deleted ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DeletedBy", expenseType.DeletedBy);
            command.Parameters.AddWithValue("@DeletedComment", expenseType.DeletedComment);
            command.Parameters.AddWithValue("@DeletedOn", expenseType.DeletedOn ?? (object)DBNull.Value);
        }
    }
}
