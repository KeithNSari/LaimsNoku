using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Premiums
{
    public class BillingHeaderRepository: IBillingHeaderRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public BillingHeaderRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public PolicyDetailedBalance GetDetailedBalance(string policyNo)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Policies_GetDetailedBalance";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyNo", policyNo);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new PolicyDetailedBalance
                            {
                                PolicyName = reader["PolicyName"].ToString(),
                                Proposer = reader["Proposer"].ToString(),
                                PremiumPayer = reader["PremiumPayer"].ToString(),
                                Currency = reader["Currency"].ToString(),
                                PolicyBalance = Convert.ToDecimal(reader["PolicyBalance"]),
                                SuspenseBalance = Convert.ToDecimal(reader["SuspenseBalance"])
                            };
                        }
                        else
                        {
                            // Handle case where no records are found for the given policyNo
                            return null;
                        }
                    }
                }
            }
        }
            public int AddAdhocBill(BillingHeader billingHeader)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BilledPremiums_AdhocBilling";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (connection)
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@BatchID", billingHeader.BatchID);
                        command.Parameters.AddWithValue("@PolicyID", billingHeader.PolicyID);
                        command.Parameters.AddWithValue("@DueDate", billingHeader.DateDue);
                        command.Parameters.AddWithValue("@PaymentMethodID", billingHeader.PaymentMethodID);
                        command.Parameters.AddWithValue("@PaymentProviderID", billingHeader.PaymentProviderID);
                        command.Parameters.AddWithValue("@PremiumPayerAccountID", billingHeader.PremiumPayerAccountID);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
        }
        public bool AddAdhocBillHeader(int BilledPremiumID)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BillingHeader_AddAdhoc";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (connection)
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@BilledPremiumID", BilledPremiumID); 
                        return Convert.ToBoolean(command.ExecuteNonQuery());
                    }
                }
            }
        }
        public void CreateBillingHeader(BillingHeader billingHeader)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"
                INSERT INTO [dbo].[BillingHeader] 
                (BatchID, BillID, PCCID, InvoiceNo, MemberID, PaymentProviderID, PaymentMethodID, CurrencyID, 
                TotalAmount, Paid, Printed, DateDue, AddedOn, AddedBy) 
                VALUES 
                (@BatchID, @BillID, @PCCID, @InvoiceNo, @MemberID, @PaymentProviderID, @PaymentMethodID, @CurrencyID, 
                @TotalAmount, @Paid, @Printed, @DateDue, @AddedOn, @AddedBy)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BatchID", billingHeader.BatchID);
                    command.Parameters.AddWithValue("@BillID", billingHeader.BillID);
                    command.Parameters.AddWithValue("@PCCID", billingHeader.PCCID);
                    command.Parameters.AddWithValue("@InvoiceNo", billingHeader.InvoiceNo ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MemberID", billingHeader.MemberID);
                    command.Parameters.AddWithValue("@PaymentProviderID", billingHeader.PaymentProviderID);
                    command.Parameters.AddWithValue("@PaymentMethodID", billingHeader.PaymentMethodID);
                    command.Parameters.AddWithValue("@CurrencyID", billingHeader.CurrencyID);
                    command.Parameters.AddWithValue("@TotalAmount", billingHeader.TotalAmount);
                    command.Parameters.AddWithValue("@Paid", billingHeader.Paid);
                    command.Parameters.AddWithValue("@Printed", billingHeader.Printed);
                    command.Parameters.AddWithValue("@DateDue", billingHeader.DateDue);
                    command.Parameters.AddWithValue("@AddedOn", billingHeader.AddedOn);
                    command.Parameters.AddWithValue("@AddedBy", billingHeader.AddedBy);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdatePolicyBalance(Guid PolicyID, decimal Amount)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [Policy] SET [Balance]=[Balance] + @Amount WHERE [ID]=@PolicyID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@Amount", Amount);
                    command.ExecuteNonQuery();
                }
            }
        }
        public BillingHeader ReadBillingHeader(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM [dbo].[BillingHeader] WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToBillingHeader(reader);
                        }
                    }
                }

                return null;
            }
        }
        public List<int> GetUnpaidBillingHeaders(int PremiumPayerID, int CurrencyID, decimal Amount, int PaymentMethodID, int PaymentProviderID)
        {
            List<int> headers = new List<int>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BillingHeader_FindUnpaidByPremiumPayer";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PremiumPayerID", PremiumPayerID);
                    command.Parameters.AddWithValue("@CurrencyID", CurrencyID);
                    command.Parameters.AddWithValue("@Amount", Amount);
                    command.Parameters.AddWithValue("@PaymentMethodID", PaymentMethodID);
                    command.Parameters.AddWithValue("@PaymentProviderID", PaymentProviderID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int header = Convert.ToInt32(reader["BillID"]);
                            headers.Add(header);
                        }
                    }
                }
            }
            return headers;
        }
        public void UpdateBillingHeader(BillingHeader billingHeader)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"
                UPDATE [dbo].[BillingHeader] 
                SET BatchID = @BatchID, BillID = @BillID, PCCID = @PCCID, 
                    InvoiceNo = @InvoiceNo, MemberID = @MemberID, 
                    PaymentProviderID = @PaymentProviderID, PaymentMethodID = @PaymentMethodID, 
                    CurrencyID = @CurrencyID, TotalAmount = @TotalAmount, 
                    Paid = @Paid, Printed = @Printed, DateDue = @DateDue, 
                    AddedOn = @AddedOn, AddedBy = @AddedBy 
                WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", billingHeader.ID);
                    command.Parameters.AddWithValue("@BatchID", billingHeader.BatchID);
                    command.Parameters.AddWithValue("@BillID", billingHeader.BillID);
                    command.Parameters.AddWithValue("@PCCID", billingHeader.PCCID);
                    command.Parameters.AddWithValue("@InvoiceNo", billingHeader.InvoiceNo ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MemberID", billingHeader.MemberID);
                    command.Parameters.AddWithValue("@PaymentProviderID", billingHeader.PaymentProviderID);
                    command.Parameters.AddWithValue("@PaymentMethodID", billingHeader.PaymentMethodID);
                    command.Parameters.AddWithValue("@CurrencyID", billingHeader.CurrencyID);
                    command.Parameters.AddWithValue("@TotalAmount", billingHeader.TotalAmount);
                    command.Parameters.AddWithValue("@Paid", billingHeader.Paid);
                    command.Parameters.AddWithValue("@Printed", billingHeader.Printed);
                    command.Parameters.AddWithValue("@DateDue", billingHeader.DateDue);
                    command.Parameters.AddWithValue("@AddedOn", billingHeader.AddedOn);
                    command.Parameters.AddWithValue("@AddedBy", billingHeader.AddedBy);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteBillingHeader(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM [dbo].[BillingHeader] WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetBatches(int StatusID)
        { 
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "BillingBatches_GetByStatus";
            cmd.Parameters.AddWithValue("StatusID", StatusID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public BillingBatch GetBillingBatchById(long batchId)
        {
            BillingBatch billingBatch = null;
            string query = "BillingBatches_GetByBatchID";
            using (SqlConnection connection = new SqlConnection(Database))
            {                
                SqlCommand command = new SqlCommand(query, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@BatchID", batchId);

                connection.Open();

                SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read())
                {
                    billingBatch = new BillingBatch
                    {
                        BatchID = reader.GetInt64(reader.GetOrdinal("BatchID")),
                        PCCID = reader.GetInt32(reader.GetOrdinal("PCCID")),
                        Provider = reader.GetString(reader.GetOrdinal("Provider")),
                        PaymentProviderID = reader.GetInt32(reader.GetOrdinal("PaymentProviderID")),
                        PaymentMethodID = reader.GetInt32(reader.GetOrdinal("PaymentMethodID")),
                        PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                        StatusID = reader.GetInt32(reader.GetOrdinal("StatusID")),
                        Entries = reader.GetInt32(reader.GetOrdinal("Entries")),
                        CurrencyID = reader.GetInt32(reader.GetOrdinal("CurrencyID")),
                        Currency = reader.GetString(reader.GetOrdinal("Currency")),
                        AllocationSuspenseAmount = reader.GetDecimal(reader.GetOrdinal("AllocationSuspenseAmount")),
                        PolicySuspenseAmount = reader.GetDecimal(reader.GetOrdinal("PolicySuspenseAmount")),
                        SystemSuspenseAmount = reader.GetDecimal(reader.GetOrdinal("SystemSuspenseAmount")),
                        BatchTotalAmount = reader.GetDecimal(reader.GetOrdinal("BatchTotalAmount")),
                        AddedOn = reader.GetDateTime(reader.GetOrdinal("AddedOn"))
                    };
                }
                reader.Close();
            }
            return billingBatch;
        } 
    private BillingHeader MapToBillingHeader(SqlDataReader reader)
        {
            return new BillingHeader
            {
                ID = (int)reader["ID"],
                BatchID = (int)reader["BatchID"],
                BillID = (int)reader["BillID"],
                PCCID = (int)reader["PCCID"],
                InvoiceNo = reader["InvoiceNo"] == DBNull.Value ? null : (string)reader["InvoiceNo"],
                MemberID = (int)reader["MemberID"],
                PaymentProviderID = (int)reader["PaymentProviderID"],
                PaymentMethodID = (int)reader["PaymentMethodID"],
                CurrencyID = (int)reader["CurrencyID"],
                TotalAmount = (decimal)reader["TotalAmount"],
                Paid = (byte)reader["Paid"],
                Printed = (byte)reader["Printed"],
                DateDue = (DateTime)reader["DateDue"],
                AddedOn = (DateTime)reader["AddedOn"],
                AddedBy = (string)reader["AddedBy"]
            };
        }
    }
}
