using LAIMS.Interfaces.Premiums;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Premiums
{
    public class PolicyTypesExpenseRepository: IPolicyTypesExpenseRepository
    { 
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private readonly string Database;
        public PolicyTypesExpenseRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public List<PolicyTypesExpense> GetAll()
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM PolicyTypesExpenses";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<PolicyTypesExpense> policyTypesExpenses = new List<PolicyTypesExpense>();
                        while (reader.Read())
                        {
                            policyTypesExpenses.Add(MapDataReaderToPolicyTypesExpense(reader));
                        }
                        return policyTypesExpenses;
                    }
                }
            }
        }
        public DataTable Get(Guid PolicyTypeID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyTypesExpenses_List";
            cmd.Parameters.AddWithValue("PolicyTypeID", PolicyTypeID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public PolicyTypesExpense GetById(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM PolicyTypesExpenses WHERE ID = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataReaderToPolicyTypesExpense(reader);
                        }
                        return null;
                    }
                }
            }
        }

        public void Add(PolicyTypesExpense policyTypesExpense)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"
                INSERT INTO PolicyTypesExpenses 
                    (PolicyTypeID, PaymentFrequencyID, ExpenseTypeID,IntermediaryTypeID, Ispercentage, CurrencyID, Amount, 
                     StartMonth, EndMonth,AppliesTo,StageID,ApplicationTypeID, [Current], AddedOn, AddedBy)
                VALUES 
                    (@PolicyTypeID, @PaymentFrequencyID, @ExpenseTypeID,@IntermediaryTypeID, @Ispercentage, @CurrencyID, @Amount, 
                     @StartMonth, @EndMonth,@AppliesTo,@StageID,@ApplicationTypeID, @Current, @AddedOn, @AddedBy)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    MapPolicyTypesExpenseToSqlParameter(policyTypesExpense, command);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(PolicyTypesExpense policyTypesExpense)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"
                UPDATE PolicyTypesExpenses 
                SET 
                    PolicyTypeID = @PolicyTypeID, 
                    PaymentFrequencyID = @PaymentFrequencyID, 
                    ExpenseTypeID = @ExpenseTypeID, 
                    Ispercentage = @Ispercentage, 
                    CurrencyID = @CurrencyID, 
                    Amount = @Amount, 
                    StartMonth = @StartMonth, 
                    EndMonth = @EndMonth, 
                    Current = @Current, 
                    AddedOn = @AddedOn, 
                    AddedBy = @AddedBy, 
                    Deleted = @Deleted, 
                    DeletedBy = @DeletedBy, 
                    DeletedComment = @DeletedComment, 
                    DeletedOn = @DeletedOn 
                WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    MapPolicyTypesExpenseToSqlParameter(policyTypesExpense, command);
                    command.Parameters.AddWithValue("@ID", policyTypesExpense.ID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Archive(int id, Guid PolicyTypeID, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Update PolicyTypesExpenses Set [Archived]=1, [ArchivedBy]=@AddedBy, [ArchivedOn]=@AddedOn WHERE ID = @Id And PolicyTypeID=@PolicyTypeID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@PolicyTypeID", PolicyTypeID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private PolicyTypesExpense MapDataReaderToPolicyTypesExpense(SqlDataReader reader)
        {
            return new PolicyTypesExpense
            {
                ID = Convert.ToInt32(reader["ID"]),
                PolicyTypeID = Guid.Parse(reader["PolicyTypeID"].ToString()),
                PaymentFrequencyID = reader["PaymentFrequencyID"] as int?,
                ExpenseTypeID = Convert.ToInt32(reader["ExpenseTypeID"]),
                Ispercentage = Convert.ToByte(reader["Ispercentage"]),
                CurrencyID = Convert.ToInt32(reader["CurrencyID"]),
                Amount = Convert.ToDecimal(reader["Amount"]),
                StartMonth = Convert.ToInt32(reader["StartMonth"]),
                EndMonth = Convert.ToInt32(reader["EndMonth"]),
                Current = reader["Current"] as byte?,
                AddedOn = reader["AddedOn"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["AddedOn"],
                AddedBy = reader["AddedBy"] == DBNull.Value ? null : (string)reader["AddedBy"],
                //Deleted = reader["Deleted"] as byte?,
                //DeletedBy = reader["DeletedBy"] as string,
                //DeletedComment = reader["DeletedComment"] as string,
                //DeletedOn = reader["DeletedOn"] as DateTime?,
            };
        }

        private void MapPolicyTypesExpenseToSqlParameter(PolicyTypesExpense policyTypesExpense, SqlCommand command)
        {
            command.Parameters.AddWithValue("@PolicyTypeID", policyTypesExpense.PolicyTypeID);
            command.Parameters.AddWithValue("@PaymentFrequencyID", policyTypesExpense.PaymentFrequencyID ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ExpenseTypeID", policyTypesExpense.ExpenseTypeID);
            command.Parameters.AddWithValue("@IntermediaryTypeID", policyTypesExpense.IntermediaryTypeID);
            command.Parameters.AddWithValue("@Ispercentage", policyTypesExpense.Ispercentage);
            command.Parameters.AddWithValue("@CurrencyID", policyTypesExpense.CurrencyID);
            command.Parameters.AddWithValue("@Amount", Convert.ToDecimal(policyTypesExpense.Amount));
            command.Parameters.AddWithValue("@StartMonth", Convert.ToInt32(policyTypesExpense.StartMonth));
            command.Parameters.AddWithValue("@EndMonth", Convert.ToInt32(policyTypesExpense.EndMonth));
            command.Parameters.AddWithValue("@AppliesTo", policyTypesExpense.AppliesTo);
            command.Parameters.AddWithValue("@StageID", policyTypesExpense.StageID);
            command.Parameters.AddWithValue("@ApplicationTypeID", policyTypesExpense.ApplicationTypeID);  
            command.Parameters.AddWithValue("@Current", policyTypesExpense.Current ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@AddedOn", policyTypesExpense.AddedOn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@AddedBy", policyTypesExpense.AddedBy ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Deleted", policyTypesExpense.Deleted ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DeletedBy", policyTypesExpense.DeletedBy ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DeletedComment", policyTypesExpense.DeletedComment ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DeletedOn", policyTypesExpense.DeletedOn ?? (object)DBNull.Value);
        }
    }
}
