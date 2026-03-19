using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using Microsoft.Data.SqlClient;
namespace LAIMS.Repositories.Lifeproducts
{ 
    public class PolicyTypesExpenseLineRepository: IPolicyTypesExpenseLineRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public PolicyTypesExpenseLineRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void AddPolicyTypesExpenseLine(PolicyTypesExpenseLine expenseLine)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"INSERT INTO PolicyTypesExpenseLines 
                             VALUES (@ExpenseTypeID, @IsPercentage, @CurrencyID, @Amount, @Current, @AddedOn,
                                     @AddedBy, @Archived, @ArchivedBy, @ArchivedComment, @ArchivedOn,
                                     @Deleted, @DeletedBy, @DeletedComment, @DeletedOn);";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, expenseLine);
                    command.ExecuteNonQuery();
                }
            }
        }

        public PolicyTypesExpenseLine GetPolicyTypesExpenseLineById(int id)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM PolicyTypesExpenseLines WHERE ID = @ID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToExpenseLine(reader);
                        }
                    }
                }
            }
            return null;
        }

        public void UpdatePolicyTypesExpenseLine(PolicyTypesExpenseLine expenseLine)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"UPDATE PolicyTypesExpenseLines 
                             SET ExpenseTypeID = @ExpenseTypeID, IsPercentage = @IsPercentage, 
                                 CurrencyID = @CurrencyID, Amount = @Amount, Current = @Current, 
                                 AddedOn = @AddedOn, AddedBy = @AddedBy, Archived = @Archived, 
                                 ArchivedBy = @ArchivedBy, ArchivedComment = @ArchivedComment, 
                                 ArchivedOn = @ArchivedOn, Deleted = @Deleted, DeletedBy = @DeletedBy, 
                                 DeletedComment = @DeletedComment, DeletedOn = @DeletedOn 
                             WHERE ID = @ID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, expenseLine);
                    command.Parameters.AddWithValue("@ID", expenseLine.ID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeletePolicyTypesExpenseLine(int id)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM PolicyTypesExpenseLines WHERE ID = @ID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SetParameters(SqlCommand command, PolicyTypesExpenseLine expenseLine)
        {
            command.Parameters.AddWithValue("@ExpenseTypeID", (object)expenseLine.ExpenseTypeID ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsPercentage", (object)expenseLine.IsPercentage ?? DBNull.Value);
            command.Parameters.AddWithValue("@CurrencyID", expenseLine.CurrencyID);
            command.Parameters.AddWithValue("@Amount", (object)expenseLine.Amount ?? DBNull.Value);
            command.Parameters.AddWithValue("@Current", (object)expenseLine.Current ?? DBNull.Value);
            command.Parameters.AddWithValue("@AddedOn", (object)expenseLine.AddedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@AddedBy", (object)expenseLine.AddedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@Archived", (object)expenseLine.Archived ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedBy", (object)expenseLine.ArchivedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedComment", (object)expenseLine.ArchivedComment ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedOn", (object)expenseLine.ArchivedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@Deleted", (object)expenseLine.Deleted ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedBy", (object)expenseLine.DeletedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedComment", (object)expenseLine.DeletedComment ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedOn", (object)expenseLine.DeletedOn ?? DBNull.Value);
        }

        private PolicyTypesExpenseLine MapReaderToExpenseLine(SqlDataReader reader)
        {
            return new PolicyTypesExpenseLine
            {
                ID = (int)reader["ID"],
                ExpenseTypeID = reader["ExpenseTypeID"] is DBNull ? (int?)null : (int)reader["ExpenseTypeID"],
                IsPercentage = reader["IsPercentage"] is DBNull ? (bool?)null : (bool)reader["IsPercentage"],
                CurrencyID = (int)reader["CurrencyID"],
                Amount = reader["Amount"] is DBNull ? (decimal?)null : (decimal)reader["Amount"],
                Current = reader["Current"] is DBNull ? (bool?)null : (bool)reader["Current"],
                AddedOn = reader["AddedOn"] is DBNull ? (DateTime?)null : (DateTime)reader["AddedOn"],
                AddedBy = reader["AddedBy"] is DBNull ? null : (string)reader["AddedBy"],
                Archived = reader["Archived"] is DBNull ? (bool?)null : (bool)reader["Archived"],
                ArchivedBy = reader["ArchivedBy"] is DBNull ? null : (string)reader["ArchivedBy"],
                ArchivedComment = reader["ArchivedComment"] is DBNull ? null : (string)reader["ArchivedComment"],
                ArchivedOn = reader["ArchivedOn"] is DBNull ? (DateTime?)null : (DateTime)reader["ArchivedOn"],
                Deleted = reader["Deleted"] is DBNull ? (bool?)null : (bool)reader["Deleted"],
                DeletedBy = reader["DeletedBy"] is DBNull ? null : (string)reader["DeletedBy"],
                DeletedComment = reader["DeletedComment"] is DBNull ? null : (string)reader["DeletedComment"],
                DeletedOn = reader["DeletedOn"] is DBNull ? (DateTime?)null : (DateTime)reader["DeletedOn"],
            };
        }
    }

}
