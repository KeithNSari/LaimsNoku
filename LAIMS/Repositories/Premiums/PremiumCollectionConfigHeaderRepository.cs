using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Membership;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Premiums
{
    public class PremiumCollectionConfigHeaderRepository: IPremiumCollectionConfigHeaderRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PremiumCollectionConfigHeaderRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public bool CheckPremiumCollectionConfigHeaderExistence(int PaymentProviderID,string StopOrderName,string StopOrderCode,int CurrencyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=COUNT(*) FROM [dbo].[PremiumCollectionConfigHeader] WHERE [PaymentProviderID]=@PaymentProviderID AND [Archived]=0 AND ([StopOrderName]=@StopOrderName OR [StopOrderCode]=@StopOrderCode) AND ([CurrencyID]=@CurrencyID); SELECT @Count";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PaymentProviderID", PaymentProviderID);
                    command.Parameters.AddWithValue("@StopOrderName", StopOrderName);
                    command.Parameters.AddWithValue("@StopOrderCode", StopOrderCode);
                    command.Parameters.AddWithValue("@CurrencyID", CurrencyID);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public void Create(PremiumCollectionConfigHeader configHeader)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "INSERT INTO PremiumCollectionConfigHeader (PaymentMethodID, PaymentProviderID, InternalBankAccountID,[StopOrderName],[StopOrderCode],[SalaryDisbursementdate],[Billingdate],[CollectionCommissionRate],[Net],CurrencyID, StoredProcedureName, Aggregated,AddedBy) " +
                               "VALUES (@PaymentMethodID, @PaymentProviderID, @InternalBankAccountID,@StopOrderName,@StopOrderCode,@SalaryDisbursementdate,@Billingdate,@CollectionCommissionRate,@Net,@CurrencyID,@StoredProcedureName, @Aggregated,@AddedBy)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, configHeader);
                    command.ExecuteNonQuery();
                }
            }
        }

        public int AddLines(int MemberID, int PaymentMethodID, DateTime CollectionDay,string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PremiumCollectionConfigLines_Add";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@MemberID", MemberID);
                    command.Parameters.AddWithValue("@PaymentMethodID", PaymentMethodID);
                    command.Parameters.AddWithValue("@CollectionDay", CollectionDay);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy); 
                   return command.ExecuteNonQuery();
                }
            }
        }
        public PremiumCollectionConfigHeader Read(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM PremiumCollectionConfigHeader WHERE ID = @ID";

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
        public DataTable Get()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PremiumCollectionConfigHeader_Get";
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetByPaymentMethod(int PaymentMethodID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PremiumCollectionConfigHeader_GetByPaymentMethod";
            command.Parameters.AddWithValue("@PaymentMethodID", PaymentMethodID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetLatestByPaymentMethod(int PaymentMethodID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PremiumCollectionConfigHeader_GetLatestByPaymentMethod";
            command.Parameters.AddWithValue("@PaymentMethodID", PaymentMethodID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable SearchByPaymentMethod(int PaymentMethodID, string SearchTerm)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PremiumCollectionConfigHeader_SearchByPaymentMethod";
            command.Parameters.AddWithValue("@PaymentMethodID", PaymentMethodID);
            command.Parameters.AddWithValue("@SearchTerm", SearchTerm); 
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetLines()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PremiumCollectionConfigLines_Get";
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public void Update(PremiumCollectionConfigHeader configHeader)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE PremiumCollectionConfigHeader " +
                               "SET PaymentMethodID = @PaymentMethodID, PaymentProviderID = @PaymentProviderID, InternalBankAccountID = @InternalBankAccountID, " +
                               "StoredProcedureName = @StoredProcedureName, Aggregated = @Aggregated, AddedOn = @AddedOn, AddedBy = @AddedBy, " +
                               "Archived = @Archived, ArchivedBy = @ArchivedBy, ArchivedComment = @ArchivedComment, ArchivedOn = @ArchivedOn " +
                               "WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, configHeader);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM PremiumCollectionConfigHeader WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void ArchivePremiumCollectionConfigHeader(int ID, string AddedBy, DateTime AddedOn)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[PremiumCollectionConfigHeader] Set [Archived]=1,[ArchivedBy]=@AddedBy,[ArchivedOn]=@AddedOn WHERE [ID]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", AddedOn);
                    command.ExecuteNonQuery();
                }
            }
        }
        private void SetParameters(SqlCommand command, PremiumCollectionConfigHeader configHeader)
        {
            command.Parameters.AddWithValue("@PaymentMethodID", configHeader.PaymentMethodID);
            command.Parameters.AddWithValue("@PaymentProviderID", configHeader.PaymentProviderID);
            command.Parameters.AddWithValue("@InternalBankAccountID", (object)configHeader.InternalBankAccountID ?? DBNull.Value);
            command.Parameters.AddWithValue("@StopOrderName", (object)configHeader.StopOrderName ?? DBNull.Value);
            command.Parameters.AddWithValue("@StopOrderCode", (object)configHeader.StopOrderCode ?? DBNull.Value);
            command.Parameters.AddWithValue("@SalaryDisbursementdate", (object)configHeader.SalaryDisbursementdate ?? DBNull.Value);
            command.Parameters.AddWithValue("@Billingdate", (object)configHeader.Billingdate ?? DBNull.Value);
            command.Parameters.AddWithValue("@CollectionCommissionRate", (object)configHeader.CollectionCommissionRate ?? DBNull.Value);
            command.Parameters.AddWithValue("@Net", (object)configHeader.Net ?? DBNull.Value);
            command.Parameters.AddWithValue("@CurrencyID", configHeader.CurrencyID);
            command.Parameters.AddWithValue("@StoredProcedureName", configHeader.StoredProcedureName);
            command.Parameters.AddWithValue("@Aggregated", configHeader.Aggregated); 
            command.Parameters.AddWithValue("@AddedBy", (object)configHeader.AddedBy ?? DBNull.Value);  
        }

        private PremiumCollectionConfigHeader MapFromReader(SqlDataReader reader)
        {
            return new PremiumCollectionConfigHeader
            {
                ID = (int)reader["ID"],
                PaymentMethodID = (int)reader["PaymentMethodID"],
                PaymentProviderID = (int)reader["PaymentProviderID"],
                InternalBankAccountID = reader["InternalBankAccountID"] as int?,
                StoredProcedureName = reader["StoredProcedureName"].ToString(),
                Aggregated = (byte)reader["Aggregated"],
                AddedOn = reader["AddedOn"] as DateTime?,
                AddedBy = reader["AddedBy"].ToString(),
                Archived = reader["Archived"] as byte?,
                ArchivedBy = reader["ArchivedBy"].ToString(),
                ArchivedComment = reader["ArchivedComment"].ToString(),
                ArchivedOn = reader["ArchivedOn"] as DateTime?
            };
        }
    }
} 
