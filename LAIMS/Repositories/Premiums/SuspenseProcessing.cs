using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;
using System.Data; 
namespace LAIMS.Repositories.Premiums
{
    public class SuspenseProcessing: ISuspenseProcessing
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public SuspenseProcessing(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");

        } 

        public SuspenseProcessingMessage PayPremium(decimal premium, int MemberID)
        {
            SuspenseProcessingMessage suspenseProcessingMessage = new SuspenseProcessingMessage();
            List<SuspenseBalance> SuspenseBalances = GetSuspenseBalances(MemberID);
            var usedBalanceAmounts = new Dictionary<int, decimal>();
            decimal totalBalance = SuspenseBalances.Sum(b => b.Amount);
            if (totalBalance < premium)
            {
                suspenseProcessingMessage.Successful = false;
                suspenseProcessingMessage.ProcessingMessage="Insufficient balance to pay the premium."; 
                return suspenseProcessingMessage;
            }
            foreach (var balance in SuspenseBalances)
            {
                if (premium <= 0)
                    break;
                decimal amountUsed = Math.Min(premium, balance.Amount);
                balance.Amount -= amountUsed;
                usedBalanceAmounts[balance.ID] = amountUsed;
                premium -= amountUsed;

            }
            suspenseProcessingMessage.Successful = true;
            suspenseProcessingMessage.ProcessingMessage = "Successfully processed";
            suspenseProcessingMessage.DeductedAmounts = usedBalanceAmounts;
            return suspenseProcessingMessage;
        }
        public List<SuspenseBalance> GetSuspenseBalances(int MemberID)
        {
            List<SuspenseBalance> suspenseBalances = new List<SuspenseBalance>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "SELECT [ID],[Balance] FROM [dbo].[SuspenseHeader] WHERE [MemberID]=@MemberID AND [Balance]>0";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MemberID", MemberID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SuspenseBalance suspenseBalance = new SuspenseBalance()
                            {
                                ID = (int)reader["ID"],
                                Amount = (decimal)reader["Balance"]
                            };
                            suspenseBalances.Add(suspenseBalance);
                        }
                    }
                }
            }
            return suspenseBalances;
        }
        public int AddSuspenseHeader(SuspenseHeader header)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"INSERT INTO SuspenseHeader (BatchID,MemberID, PaymentMethodID, PaymentTypeID,PaymentID, InternalBankAccountID, 
                            SuspenseType, PolicyID,TargetPolicyNo,SourceID, Source, PaymentDate, Reference, StatusID, ProcessedAmount, 
                            CurrencyID, Balance, AddedOn, AddedBy) 
                            VALUES (@BatchID,@MemberID, @PaymentMethodID, @PaymentTypeID,@PaymentID, @InternalBankAccountID, 
                            @SuspenseType, @PolicyID,@TargetPolicyNo,@SourceID, @Source, @PaymentDate, @Reference, @StatusID, 
                            @ProcessedAmount, @CurrencyID, @Balance, @AddedOn, @AddedBy); SELECT @@IDENTITY";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BatchID", header.BatchID);
                command.Parameters.AddWithValue("@MemberID", header.MemberID);
                command.Parameters.AddWithValue("@PaymentMethodID", header.PaymentMethodID);
                command.Parameters.AddWithValue("@PaymentTypeID", header.PaymentTypeID);
                command.Parameters.AddWithValue("@PaymentID", header.PaymentID);
                command.Parameters.AddWithValue("@InternalBankAccountID", header.InternalBankAccountID);
                command.Parameters.AddWithValue("@SuspenseType", (object)header.SuspenseType ?? DBNull.Value);
                command.Parameters.AddWithValue("@PolicyID", (object)header.PolicyID ?? DBNull.Value);
                command.Parameters.AddWithValue("@TargetPolicyNo", (object)header.TargetPolicyNo ?? DBNull.Value);                
                command.Parameters.AddWithValue("@SourceID", (object)header.SourceID ?? DBNull.Value);
                command.Parameters.AddWithValue("@Source", (object)header.Source ?? DBNull.Value);
                command.Parameters.AddWithValue("@PaymentDate", header.PaymentDate);
                command.Parameters.AddWithValue("@Reference", (object)header.Reference ?? DBNull.Value);
                command.Parameters.AddWithValue("@StatusID", header.StatusID);
                command.Parameters.AddWithValue("@ProcessedAmount", header.ProcessedAmount);
                command.Parameters.AddWithValue("@CurrencyID", (object)header.CurrencyID ?? DBNull.Value);
                command.Parameters.AddWithValue("@Balance", header.Balance);
                command.Parameters.AddWithValue("@AddedOn", header.AddedOn);
                command.Parameters.AddWithValue("@AddedBy", header.AddedBy);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
        public void UpdateSuspenseHeaderBalance(int SuspenseHeaderID, decimal Amount )
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "Update [SuspenseHeader] SET [Balance]=[Balance] + @Amount WHERE [ID]=@SuspenseHeaderID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@Amount", Amount);
                    command.Parameters.AddWithValue("@SuspenseHeaderID", SuspenseHeaderID); 
                    command.ExecuteNonQuery();
                }
            }
        }
        public void AddSuspenseLine(SuspenseLine line)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"INSERT INTO SuspenseLines (HeaderID, Credit, Debit, AddedOn, AddedBy) 
                             VALUES (@HeaderID, @Credit, @Debit, @AddedOn, @AddedBy)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@HeaderID", line.HeaderID);
                command.Parameters.AddWithValue("@Credit", line.Credit);
                command.Parameters.AddWithValue("@Debit", line.Debit);
                command.Parameters.AddWithValue("@AddedOn", line.AddedOn);
                command.Parameters.AddWithValue("@AddedBy", line.AddedBy);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public DataTable GetLatestSystemSuspenseData()
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            string query = "SystemSuspenseHeader_GetLatest";
            cmd.CommandText = query; 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetLatestPolicySuspenseData()
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            string query = "SuspenseHeader_GetLatest";
            cmd.CommandText = query;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetPolicySuspenseData(long BatchID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure; 
            cmd.CommandText = "SuspenseHeader_GetByBatch";
            cmd.Parameters.AddWithValue("@BatchID", BatchID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetPolicySuspenseData(Guid PolicyID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SuspenseHeader_GetByPolicy";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetPolicySuspenseBatch(long BatchID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SuspenseHeader_GetPolicySuspenseBatch";
            cmd.Parameters.AddWithValue("@BatchID", BatchID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetSystemSuspenseData(long BatchID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SuspenseHeader_GetSystemSuspenseBatch";
            cmd.Parameters.AddWithValue("@BatchID", BatchID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable SearchLatestPolicySuspenseData(string PolicyNo)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            string query = "SuspenseHeader_Search";
            cmd.Parameters.AddWithValue("PolicyNo", PolicyNo);  
            cmd.CommandText = query;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable SearchPayments(DateTime PaymentDate)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            string query = "Premiums_SearchByDate";
            cmd.CommandText = query;
            SqlParameter paymentDate = new("PaymentDate", DbType.Date);
            paymentDate.Value = PaymentDate;
            cmd.Parameters.Add(paymentDate);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable SearchPolicySuspenseDataByDate(DateTime PaymentDate)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            string query = "SuspenseHeader_SearchByDate";
            cmd.CommandText = query;
            SqlParameter paymentDate = new("PaymentDate", DbType.Date);
            paymentDate.Value = PaymentDate;
            cmd.Parameters.Add(paymentDate);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
		public DataTable SearchSytemSuspenseDataByDate(DateTime PaymentDate)
		{
			DataTable DT = new DataTable();
			SqlConnection connection = new SqlConnection();
			connection.ConnectionString = Database;
			SqlCommand cmd = connection.CreateCommand();
			cmd.CommandType = CommandType.StoredProcedure;
			string query = "SuspenseHeader_SearchSystemSuspenseByDate";
			cmd.CommandText = query;
			SqlParameter paymentDate = new("PaymentDate", DbType.Date);
			paymentDate.Value = PaymentDate;
			cmd.Parameters.Add(paymentDate);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			da.Fill(DT);
			return DT;
		}
		public decimal GetBatchSuspenseBalance(long BatchID, int SuspenseType)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "SELECT ISNULL(SUM([Balance]),0) FROM [dbo].[SuspenseHeader] WHERE [BatchID]=@BatchID AND [Reversed]=0 AND [SuspenseType]=@SuspenseType";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.Parameters.AddWithValue("@SuspenseType", SuspenseType);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        public void ReversePolicySuspense(int SuspenseHeaderID,int ReversalReason,string ReversalComment, string ReversedBy)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "PolicySuspense_Reversal";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@SuspenseHeaderID", SuspenseHeaderID);
                    command.Parameters.AddWithValue("@ReversedBy", ReversedBy);
                    command.Parameters.AddWithValue("@ReversalReason", ReversalReason);
                    command.Parameters.AddWithValue("@ReversalComment", ReversalComment);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void ReverseSuspenseEntry(int SuspenseHeaderID, int ReversalReason, string ReversalComment, string ReversedBy)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "Suspense_Reversal";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@SuspenseHeaderID", SuspenseHeaderID);
                    command.Parameters.AddWithValue("@ReversedBy", ReversedBy);
                    command.Parameters.AddWithValue("@ReversalReason", ReversalReason);
                    command.Parameters.AddWithValue("@ReversalComment", ReversalComment);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void RefundSuspenseEntry(int SuspenseHeaderID, int ReversalReason, decimal Amount, string ReversalComment, string ReversedBy)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "Suspense_Refund";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@SuspenseHeaderID", SuspenseHeaderID);
                    command.Parameters.AddWithValue("@Amount", Amount);
                    command.Parameters.AddWithValue("@ReversedBy", ReversedBy);
                    command.Parameters.AddWithValue("@ReversalReason", ReversalReason);
                    command.Parameters.AddWithValue("@ReversalComment", ReversalComment);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void RefundSystemSuspenseEntry(int SuspenseHeaderID, int ReversalReason, decimal Amount, string ReversalComment, string ReversedBy)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "SystemSuspense_Refund";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@SuspenseHeaderID", SuspenseHeaderID);
                    command.Parameters.AddWithValue("@Amount", Amount);
                    command.Parameters.AddWithValue("@ReversedBy", ReversedBy);
                    command.Parameters.AddWithValue("@ReversalReason", ReversalReason);
                    command.Parameters.AddWithValue("@ReversalComment", ReversalComment);
                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetPaymentDetails(int ID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            string query = "SELECT [Details],[Field1],[Field2],[Field3],[Field4],[Field5],[Field6],[Field7],[Field8],[Field9],[Field10] FROM [dbo].[Payments] WHERE [ID]=@ID";
            cmd.Parameters.AddWithValue("@ID", ID);
            cmd.CommandText = query;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
    }
} 
