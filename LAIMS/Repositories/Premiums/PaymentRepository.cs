using LAIMS.Interfaces.Premiums;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.OleDb;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LAIMS.Repositories.Premiums
{
    public class PaymentRepository: IPaymentRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private ISuspenseProcessing _suspenseProcessing;

        public PaymentRepository(IConfiguration configuration, IWebHostEnvironment environment,
            ISuspenseProcessing suspenseProcessing)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
            _suspenseProcessing = suspenseProcessing;
        }
        public int InsertPayment(Payment payment)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"
                INSERT INTO Payments (BatchID, CurrencyID, Amount, Reference, InternalAccountNoID, PaymentMethod, 
                                      PaymentType, PaymentProvider, PaidBy, PaymentDate, Details, PolicySuspenseAmount, 
                                      PystemSuspenseAmount, AllocationSuspenseAmount,PolicyNo, Field1, Field2, Field3, Field4, 
                                      Field5, Field6, Field7, Field8, Field9, Field10, AddedOn, AddedBy)
                VALUES (@BatchID, @CurrencyID, @Amount, @Reference, @InternalAccountNoID, @PaymentMethod, @PaymentType, 
                        @PaymentProvider, @PaidBy, @PaymentDate, @Details, @PolicySuspenseAmount, @PystemSuspenseAmount, 
                        @AllocationSuspenseAmount,@PolicyNo, @Field1, @Field2, @Field3, @Field4, @Field5, @Field6, @Field7, 
                        @Field8, @Field9, @Field10, @AddedOn, @AddedBy);
                SELECT SCOPE_IDENTITY();";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BatchID", payment.BatchID);
                command.Parameters.AddWithValue("@CurrencyID", payment.CurrencyID);
                command.Parameters.AddWithValue("@Amount", payment.Amount);
                command.Parameters.AddWithValue("@Reference", payment.Reference);
                command.Parameters.AddWithValue("@InternalAccountNoID", payment.InternalAccountNoID);
                command.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
                command.Parameters.AddWithValue("@PaymentType", payment.PaymentType);
                command.Parameters.AddWithValue("@PaymentProvider", payment.PaymentProvider);
                command.Parameters.AddWithValue("@PaidBy", (object)payment.PaidBy ?? DBNull.Value);
                command.Parameters.AddWithValue("@PaymentDate", (object)payment.PaymentDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@Details", (object)payment.Details ?? DBNull.Value);
                command.Parameters.AddWithValue("@PolicySuspenseAmount", (object)payment.PolicySuspenseAmount ?? DBNull.Value);
                command.Parameters.AddWithValue("@PystemSuspenseAmount", (object)payment.PystemSuspenseAmount ?? DBNull.Value);
                command.Parameters.AddWithValue("@AllocationSuspenseAmount", (object)payment.AllocationSuspenseAmount ?? DBNull.Value);
				command.Parameters.AddWithValue("@PolicyNo", (object)payment.PolicyNo ?? DBNull.Value);
				command.Parameters.AddWithValue("@Field1", (object)payment.Field1 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field2", (object)payment.Field2 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field3", (object)payment.Field3 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field4", (object)payment.Field4 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field5", (object)payment.Field5 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field6", (object)payment.Field6 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field7", (object)payment.Field7 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field8", (object)payment.Field8 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field9", (object)payment.Field9 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field10", (object)payment.Field10 ?? DBNull.Value);
                command.Parameters.AddWithValue("@AddedOn", (object)payment.AddedOn ?? DBNull.Value);
                command.Parameters.AddWithValue("@AddedBy", (object)payment.AddedBy ?? DBNull.Value); 
                connection.Open();
                object insertedId = command.ExecuteScalar();

                if (insertedId != DBNull.Value && insertedId != null)
                {
                    return Convert.ToInt32(insertedId);
                }

                throw new InvalidOperationException("Insert operation failed or ID not returned.");
            }
        }
        public void UploadStatement(string ExcelFilePath, long BatchID, string AddedBy)
        {
            string excelConnStr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={ExcelFilePath};Extended Properties='Excel 12.0;HDR=YES;'";
            using (var excelConn = new OleDbConnection(excelConnStr))
            {
                excelConn.Open();
                // Assuming the data is on the first sheet (Sheet1)
                string excelQuery = "SELECT * FROM [Statement$]";
                using (var excelCommand = new OleDbCommand(excelQuery, excelConn))
                {
                    using (var excelReader = excelCommand.ExecuteReader())
                    {
                        using (var sqlConn = new SqlConnection(Database))
                        {
                            sqlConn.Open();
                            // Prepare SQL INSERT statement
                            string sqlInsert = @"
                            INSERT INTO Payments (BatchID, CurrencyID, Amount, Reference, InternalAccountNoID, 
                                                  PaymentMethod, PaymentType, PaymentProvider, PaidBy, PaymentDate, 
                                                  Details,PolicyID,PolicyNo,Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8, Field9, Field10, 
                                                  AddedOn, AddedBy) OUTPUT INSERTED.ID
                            VALUES (@BatchID, @CurrencyID, @Amount, @Reference, @InternalAccountNoID, @PaymentMethod, 
                                    @PaymentType, @PaymentProvider, @PaidBy, @PaymentDate,@Details,@PolicyID,@PolicyNo, @Field1, @Field2, @Field3, @Field4, 
                                    @Field5, @Field6, @Field7, @Field8, @Field9, @Field10, @AddedOn, @AddedBy);";
                            using (var sqlCmd = new SqlCommand(sqlInsert, sqlConn))
                            {
                                while (excelReader.Read())
                                {
                                    // Map Excel columns to SQL parameters
                                    string? policyNo = excelReader["PolicyNo"].ToString();
                                    Guid policyID = Guid.Empty;
									if (!string.IsNullOrEmpty(policyNo))
									{
										policyID= GetPolicyID(policyNo);
									}

									sqlCmd.Parameters.Clear();
                                    sqlCmd.Parameters.AddWithValue("@BatchID", BatchID);
                                    int currencyID = GetCurrencyID(excelReader["Currency"].ToString());
                                    sqlCmd.Parameters.AddWithValue("@CurrencyID", currencyID);
                                    decimal amount = Convert.ToDecimal(excelReader["Amount"]);
                                    sqlCmd.Parameters.AddWithValue("@Amount",amount );
                                    string reference = excelReader["Reference"].ToString();
                                    sqlCmd.Parameters.AddWithValue("@Reference", reference);
                                    int accountID = GetAccountID(excelReader["Account No"].ToString());
                                    sqlCmd.Parameters.AddWithValue("@InternalAccountNoID",accountID);                                   
                                    sqlCmd.Parameters.AddWithValue("@PaymentMethod", 3);//direct payment
                                    sqlCmd.Parameters.AddWithValue("@PaymentType", 6);
                                    sqlCmd.Parameters.AddWithValue("@PaymentProvider", 0); //change so that ZB has ID 0 
                                    sqlCmd.Parameters.AddWithValue("@PaidBy", excelReader["Paid By"].ToString() ?? (object)DBNull.Value);
                                    DateTime paymentDate = Convert.ToDateTime(excelReader["Payment Date"].ToString());
                                    sqlCmd.Parameters.AddWithValue("@PaymentDate", paymentDate);
                                    sqlCmd.Parameters.AddWithValue("@Details", excelReader["Details"].ToString());
									sqlCmd.Parameters.AddWithValue("@PolicyNo", policyNo ?? (object)DBNull.Value);
                                    sqlCmd.Parameters.AddWithValue("@PolicyID", policyID);
								    sqlCmd.Parameters.AddWithValue("@Field1", excelReader["Field1"].ToString() ?? (object)DBNull.Value);
                                    sqlCmd.Parameters.AddWithValue("@Field2", excelReader["Field2"].ToString() ?? (object)DBNull.Value);
                                    sqlCmd.Parameters.AddWithValue("@Field3", excelReader["Field3"].ToString() ?? (object)DBNull.Value);
                                    sqlCmd.Parameters.AddWithValue("@Field4", excelReader["Field4"].ToString() ?? (object)DBNull.Value);
                                    sqlCmd.Parameters.AddWithValue("@Field5", excelReader["Field5"].ToString() ?? (object)DBNull.Value);
                                    sqlCmd.Parameters.AddWithValue("@Field6", excelReader["Field6"].ToString() ?? (object)DBNull.Value);
                                    sqlCmd.Parameters.AddWithValue("@Field7", excelReader["Field7"].ToString() ?? (object)DBNull.Value);
                                    sqlCmd.Parameters.AddWithValue("@Field8", excelReader["Field8"].ToString() ?? (object)DBNull.Value);
                                    sqlCmd.Parameters.AddWithValue("@Field9", excelReader["Field9"].ToString() ?? (object)DBNull.Value);
                                    sqlCmd.Parameters.AddWithValue("@Field10", excelReader["Field10"].ToString() ?? (object)DBNull.Value); 
                                    sqlCmd.Parameters.AddWithValue("@AddedOn", DateTime.Now); // Assuming AddedOn can be null in DB
                                    sqlCmd.Parameters.AddWithValue("@AddedBy", AddedBy);
                                    // Execute SQL INSERT command for each row
                                    int paymentId = (int)sqlCmd.ExecuteScalar();
                                    byte suspenseType=2;
                                    if (policyID == Guid.Empty) suspenseType = 3;
                                    SuspenseHeader suspenseHeader = new()
                                    {
                                        BatchID = BatchID,
                                        MemberID = 0,
                                        PaymentMethodID = 3,
                                        PaymentTypeID = 6,
                                        PaymentID = paymentId,
                                        Source = "",
                                        InternalBankAccountID = 0,
                                        SuspenseType = suspenseType,
                                        PolicyID = policyID,
                                        TargetPolicyNo = string.Empty,
                                        PaymentDate = paymentDate,
                                        Reference = reference,
                                        StatusID = 0,
                                        ProcessedAmount = 0,
                                        CurrencyID = currencyID,
                                        Balance = amount,
                                        AddedOn = DateTime.Now,
                                        AddedBy = AddedBy
                                    };
                                    int id = _suspenseProcessing.AddSuspenseHeader(suspenseHeader);
                                    SuspenseLine suspenseLine = new()
                                    {
                                        HeaderID = id,
                                        Credit = amount,
                                        Debit = 0,
                                        AddedOn = DateTime.Now,
                                        AddedBy = AddedBy
                                    };
                                    _suspenseProcessing.AddSuspenseLine(suspenseLine);
                                }
                            }
                        }
                    }
                }
            }
            AddCashBatchHeader(BatchID, AddedBy);
        }
        public void UploadDebitOrders(string ExcelFilePath, long BatchID, int PaymentProviderID, string AddedBy)
        {
            string excelConnStr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={ExcelFilePath};Extended Properties='Excel 12.0;HDR=YES;'";
            using (var excelConn = new OleDbConnection(excelConnStr))
            {
                excelConn.Open();
                // Assuming the data is on the first sheet (Sheet1)
                string excelQuery = "SELECT * FROM [Entries$]";
                using (var excelCommand = new OleDbCommand(excelQuery, excelConn))
                {
                    using (var excelReader = excelCommand.ExecuteReader())
                    {
                        using (var sqlConn = new SqlConnection(Database))
                        {
                            sqlConn.Open();
                            // Prepare SQL INSERT statement
                            string sqlInsert = @"
                            INSERT INTO Payments (BatchID, CurrencyID, Amount, Reference, InternalAccountNoID, 
                                                  PaymentMethod, PaymentType, PaymentProvider, PaidBy, PaymentDate, 
                                                  Details,PolicyID,PolicyNo,AddedOn, AddedBy) OUTPUT INSERTED.ID
                            VALUES (@BatchID, @CurrencyID, @Amount, @Reference, @InternalAccountNoID, @PaymentMethod, 
                                    @PaymentType, @PaymentProvider, @PaidBy, @PaymentDate,@Details,@PolicyID,@PolicyNo,@AddedOn,@AddedBy);";
                            using (var sqlCmd = new SqlCommand(sqlInsert, sqlConn))
                            {
                                while (excelReader.Read())
                                {
                                    // Map Excel columns to SQL parameters
                                    string? policyNo = excelReader["PolicyNo"].ToString();
                                    Guid policyID = Guid.Empty;
                                    if (!string.IsNullOrEmpty(policyNo))
                                    {
                                        policyID = GetPolicyID(policyNo);
                                    }

                                    sqlCmd.Parameters.Clear();
                                    sqlCmd.Parameters.AddWithValue("@BatchID", BatchID);
                                    int currencyID = GetCurrencyID(excelReader["Currency"].ToString());
                                    sqlCmd.Parameters.AddWithValue("@CurrencyID", currencyID);
                                    decimal amount = Convert.ToDecimal(excelReader["Amount"]);
                                    sqlCmd.Parameters.AddWithValue("@Amount", amount);
                                    string reference = excelReader["Reference"].ToString();
                                    sqlCmd.Parameters.AddWithValue("@Reference", reference);
                                    //int accountID = GetAccountID(excelReader["Account No"].ToString());
                                    sqlCmd.Parameters.AddWithValue("@InternalAccountNoID", -1);// accountID);
                                    sqlCmd.Parameters.AddWithValue("@PaymentMethod", 1);//direct payment
                                    sqlCmd.Parameters.AddWithValue("@PaymentType", 6);
                                    sqlCmd.Parameters.AddWithValue("@PaymentProvider", 0); //change so that ZB has ID 0 
                                    sqlCmd.Parameters.AddWithValue("@PaidBy",DBNull.Value);
                                    DateTime paymentDate = Convert.ToDateTime(excelReader["Payment Date"].ToString());
                                    sqlCmd.Parameters.AddWithValue("@PaymentDate", paymentDate);
                                    sqlCmd.Parameters.AddWithValue("@Details", excelReader["Details"].ToString());
                                    sqlCmd.Parameters.AddWithValue("@PolicyNo", policyNo ?? (object)DBNull.Value);
                                    sqlCmd.Parameters.AddWithValue("@PolicyID", policyID); 
                                    sqlCmd.Parameters.AddWithValue("@AddedOn", DateTime.Now); // Assuming AddedOn can be null in DB
                                    sqlCmd.Parameters.AddWithValue("@AddedBy", AddedBy);
                                    // Execute SQL INSERT command for each row
                                    int paymentId = (int)sqlCmd.ExecuteScalar();
                                    byte suspenseType = 2;
                                    if (policyID == Guid.Empty) suspenseType = 3;
                                    SuspenseHeader suspenseHeader = new()
                                    {
                                        BatchID = BatchID,
                                        MemberID = 0,
                                        PaymentMethodID = 1,
                                        PaymentTypeID = 6,
                                        PaymentID = paymentId,
                                        Source = "",
                                        InternalBankAccountID = 0,
                                        SuspenseType = suspenseType,
                                        PolicyID = policyID,
                                        TargetPolicyNo = string.Empty,
                                        PaymentDate = paymentDate,
                                        Reference = reference,
                                        StatusID = 0,
                                        ProcessedAmount = 0,
                                        CurrencyID = currencyID,
                                        Balance = amount,
                                        AddedOn = DateTime.Now,
                                        AddedBy = AddedBy
                                    };
                                    int id = _suspenseProcessing.AddSuspenseHeader(suspenseHeader);
                                    SuspenseLine suspenseLine = new()
                                    {
                                        HeaderID = id,
                                        Credit = amount,
                                        Debit = 0,
                                        AddedOn = DateTime.Now,
                                        AddedBy = AddedBy
                                    };
                                    _suspenseProcessing.AddSuspenseLine(suspenseLine);
                                }
                            }
                        }
                    }
                }
            }
            AddCashBatchHeader(BatchID, AddedBy);
        }
        private Guid GetPolicyID(string PolicyNo)
		{
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string sql = "DECLARE @ID uniqueidentifier='00000000-0000-0000-0000-000000000000'; SELECT @ID=[ID] FROM [Policy] WHERE ([PolicyNo]=@PolicyNo OR [ApplicationNo]=@PolicyNo); SELECT @ID";
				using (SqlCommand command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
					return Guid.Parse(command.ExecuteScalar().ToString());
				}
			}
		}
		private int GetCurrencyID(string CurrencyName)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "DECLARE @ID int=0; SELECT @ID=[Id] FROM [dbo].[Currencies] WHERE ([Name]=@CurrencyName OR [ShortCode]=@CurrencyName) AND [Archived]=0; SELECT @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@CurrencyName", CurrencyName);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        private int GetAccountID(string AccountNo)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "DECLARE @ID int=0; SELECT @ID=[ID] FROM  [dbo].[MemberBankAccounts] WHERE ([BankAccountNo]=@AccountNo) AND [Internal]=1 AND [MemberID]=0; SELECT @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@AccountNo", AccountNo);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        private int AddCashBatchHeader(long BatchID,string AddedBy)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "INSERT INTO [dbo].[CashFileBatches]([BatchID],[AddedBy]) VALUES (@BatchID,@AddedBy)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    return command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetLatestCashBatchHeaders()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT TOP(100) [BatchID],[CashFileBatches].[AddedOn],[UserName] AS [AddedBy] FROM [dbo].[CashFileBatches] LEFT JOIN [AspNetUsers] ON [CashFileBatches].[AddedBy]=[AspNetUsers].[Id] ORDER BY [BatchID] DESC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public Payment GetPaymentById(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"SELECT * FROM Payments WHERE ID = @Id;";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return MapPaymentFromReader(reader);
                }

                return null;
            }
        }
        public List<Payment> GetAllPayments()
        {
            List<Payment> payments = new List<Payment>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"SELECT * FROM Payments;";

                SqlCommand command = new SqlCommand(query, connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Payment payment = MapPaymentFromReader(reader);
                    payments.Add(payment);
                }
            }

            return payments;
        }
        public void UpdatePayment(Payment payment)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"
                UPDATE Payments 
                SET BatchID = @BatchID, 
                    CurrencyID = @CurrencyID, 
                    Amount = @Amount, 
                    Reference = @Reference, 
                    InternalAccountNoID = @InternalAccountNoID,
                    PaymentMethod = @PaymentMethod, 
                    PaymentType = @PaymentType, 
                    PaymentProvider = @PaymentProvider, 
                    PaidBy = @PaidBy, 
                    PaymentDate = @PaymentDate, 
                    Details = @Details, 
                    PolicySuspenseAmount = @PolicySuspenseAmount, 
                    PystemSuspenseAmount = @PystemSuspenseAmount, 
                    AllocationSuspenseAmount = @AllocationSuspenseAmount,
                    Field1 = @Field1, 
                    Field2 = @Field2, 
                    Field3 = @Field3, 
                    Field4 = @Field4, 
                    Field5 = @Field5, 
                    Field6 = @Field6, 
                    Field7 = @Field7, 
                    Field8 = @Field8, 
                    Field9 = @Field9, 
                    Field10 = @Field10,
                    AddedOn = @AddedOn,
                    AddedBy = @AddedBy,
                    Reversed = @Reversed,
                    ReversedOn = @ReversedOn,
                    ReversedBy = @ReversedBy
                WHERE ID = @ID;"; 

                SqlCommand command = new SqlCommand(query, connection); 
                command.Parameters.AddWithValue("@BatchID", payment.BatchID);
                command.Parameters.AddWithValue("@CurrencyID", payment.CurrencyID);
                command.Parameters.AddWithValue("@Amount", payment.Amount);
                command.Parameters.AddWithValue("@Reference", payment.Reference);
                command.Parameters.AddWithValue("@InternalAccountNoID", payment.InternalAccountNoID);
                command.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
                command.Parameters.AddWithValue("@PaymentType", payment.PaymentType);
                command.Parameters.AddWithValue("@PaymentProvider", payment.PaymentProvider);
                command.Parameters.AddWithValue("@PaidBy", (object)payment.PaidBy ?? DBNull.Value);
                command.Parameters.AddWithValue("@PaymentDate", (object)payment.PaymentDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@Details", (object)payment.Details ?? DBNull.Value);
                command.Parameters.AddWithValue("@PolicySuspenseAmount", (object)payment.PolicySuspenseAmount ?? DBNull.Value);
                command.Parameters.AddWithValue("@PystemSuspenseAmount", (object)payment.PystemSuspenseAmount ?? DBNull.Value);
                command.Parameters.AddWithValue("@AllocationSuspenseAmount", (object)payment.AllocationSuspenseAmount ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field1", (object)payment.Field1 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field2", (object)payment.Field2 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field3", (object)payment.Field3 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field4", (object)payment.Field4 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field5", (object)payment.Field5 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field6", (object)payment.Field6 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field7", (object)payment.Field7 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field8", (object)payment.Field8 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field9", (object)payment.Field9 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Field10", (object)payment.Field10 ?? DBNull.Value);
                command.Parameters.AddWithValue("@AddedOn", (object)payment.AddedOn ?? DBNull.Value);
                command.Parameters.AddWithValue("@AddedBy", (object)payment.AddedBy ?? DBNull.Value);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public void DeletePayment(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"DELETE FROM Payments
                             WHERE ID = @Id;";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        private Payment MapPaymentFromReader(SqlDataReader reader)
        {
            return new Payment
            {
                ID = (int)reader["ID"],
                BatchID = (long)reader["BatchID"],
                CurrencyID = (int)reader["CurrencyID"],
                Amount = (decimal)reader["Amount"],
                Reference = reader["Reference"].ToString(),
                InternalAccountNoID = (int)reader["InternalAccountNoID"],
                PaymentMethod = (int)reader["PaymentMethod"],
                PaymentType = (int)reader["PaymentType"],
                PaymentProvider = (int)reader["PaymentProvider"],
                PaidBy = reader["PaidBy"] != DBNull.Value ? (int)reader["PaidBy"] : (int?)null,
                PaymentDate = reader["PaymentDate"] != DBNull.Value ? (DateTime)reader["PaymentDate"] : (DateTime?)null,
                Details = reader["Details"] != DBNull.Value ? reader["Details"].ToString() : null,
                PolicySuspenseAmount = reader["PolicySuspenseAmount"] != DBNull.Value ? (decimal)reader["PolicySuspenseAmount"] : (decimal?)null,
                PystemSuspenseAmount = reader["PystemSuspenseAmount"] != DBNull.Value ? (decimal)reader["PystemSuspenseAmount"] : (decimal?)null,
                AllocationSuspenseAmount = reader["AllocationSuspenseAmount"] != DBNull.Value ? (decimal)reader["AllocationSuspenseAmount"] : (decimal?)null,
                Field1 = reader["Field1"].ToString(),
                Field2 = reader["Field2"].ToString(),
                Field3 = reader["Field3"].ToString(),
                Field4 = reader["Field4"].ToString(),
                Field5 = reader["Field5"].ToString(),
                Field6 = reader["Field6"].ToString(),
                Field7 = reader["Field7"].ToString(),
                Field8 = reader["Field8"].ToString(),
                Field9 = reader["Field9"].ToString(),
                Field10 = reader["Field10"].ToString(),
                AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime)reader["AddedOn"] : (DateTime?)null,
                AddedBy = reader["AddedBy"].ToString() 
            };
        }

        //Region Upload Premium Rates
        public void UploadPremiumRates(DataTable PremiumRatesDT, Guid MediaUploadID, DateTime EffectiveDate, string AddedBy)
        {
            long batchID = Convert.ToInt64(DateTime.Now.ToString("yyMMddmmss"));
            List<int> RiskParameters = new List<int>();
            Guid productID = Guid.Empty;
            int currencyID = 0;
            decimal sumAssured = 0;
            int riskGroupID = -1;
            string delimiter = ""; 
            int premiumsHeaderRow = 0;           
            for (int currentRowNo = 0; currentRowNo < PremiumRatesDT.Rows.Count; currentRowNo++)
            {
                DataRow DR = PremiumRatesDT.Rows[currentRowNo];
                string currentValue = Convert.ToString(DR[0]).Trim();
                if (currentRowNo == 0)
                {
                    string product = Convert.ToString(currentValue).Trim();
                    productID = GetProductID(product);
                }
                //Use Switch to get other delimeters
                switch (currentValue)
                {
                    case "Start Parameters":
                        delimiter = "Start Parameters";
                        break;
                    case "End Paramaters":
                        delimiter = "End Paramaters";
                        break;
                    case "Sum Assured":
                       if(!decimal.TryParse(Convert.ToString(PremiumRatesDT.Rows[currentRowNo + 1][0].ToString()), out sumAssured))
                        {
                            sumAssured = 0;
                        }
                        break; 
                    case "Currency Code":
                        string currency = Convert.ToString(PremiumRatesDT.Rows[currentRowNo + 1][0].ToString());
                        currencyID = GetCurrencyID(currency.Trim());                             
                        break;
                    case "Start Premium Rates":
                        delimiter = "Start Premium Rates";
                        premiumsHeaderRow = currentRowNo + 1;
                        break;
                    case "End Premium Rates":
                        delimiter = "End Premium Rates";
                        break;
                    default:
                        break;
                }

                //Use the If statements to get the data for the associated delimeter
                if ((delimiter.Equals("Start Parameters")) && (!currentValue.Equals("End Paramaters")))
                {
                    string riskParameter = currentValue.Trim(); ;
                    if(!riskParameter.Equals("Start Parameters") && (!string.IsNullOrEmpty(riskParameter)))
                    {
                        RiskParameters.Add(GetRiskParameterID(currentValue.Trim()));
                    }                     
                }
                else if (delimiter.Equals("End Paramaters"))
                {
                  if(RiskParameters.Count>0) riskGroupID = GetRiskGroup(RiskParameters);
                }  
                else if (delimiter.Equals("Start Premium Rates"))
                {
                    //INSERT Premium Rates Header
                   
                    int relationshipID = -1;
                    if (currentRowNo > premiumsHeaderRow)
                    {
                        if (int.TryParse(Convert.ToString(PremiumRatesDT.Rows[currentRowNo][4]), out int age))
                        {
                            string relationship = Convert.ToString(PremiumRatesDT.Rows[currentRowNo][0] ?? "");
                            if (!string.IsNullOrEmpty(relationship)) relationshipID = GetRelationshipID(relationship.Trim());
                            decimal minCover = 0;
                            decimal maxCover = 0; 
                            if (PremiumRatesDT.Rows[currentRowNo][1] != null)
                            {
                                string colValue = PremiumRatesDT.Rows[currentRowNo][1].ToString();
                                if ((!string.IsNullOrEmpty(colValue)) && (!decimal.TryParse(colValue, out minCover)))
                                {
                                    throw new Exception("Invalid value for minimum cover found! Value should be a valid decimal.");
                                }
                            }
                            if (PremiumRatesDT.Rows[currentRowNo][2] != null)
                            {
                                string colValue = PremiumRatesDT.Rows[currentRowNo][2].ToString();
                                if ((!string.IsNullOrEmpty(colValue))&&  (!decimal.TryParse(colValue, out maxCover)))
                                {
                                    throw new Exception("Invalid value for maximum cover found! Value should be a valid decimal.");
                                }
                            }                            
                            string frequency = Convert.ToString(PremiumRatesDT.Rows[currentRowNo][3] ?? "");
                            if (string.IsNullOrEmpty(frequency)) throw new Exception("Frequency cannot be empty!");
                            int frequencyID = GetFrequencyID(frequency);
                            //loop through policy term indexes 
                            object[] items = PremiumRatesDT.Rows[premiumsHeaderRow].ItemArray;
                            for (int i = 0; i < items.Length; i++)
                            {
                                if (i > 4)
                                {
                                    if ((items[i] != null) && (int.TryParse(items[i].ToString(), out int term)))
                                    {
                                        if ((PremiumRatesDT.Rows[currentRowNo][i] != null) && (decimal.TryParse(PremiumRatesDT.Rows[currentRowNo][i].ToString(), out decimal premium)))
                                        {
                                            SavePremiumRates(batchID, riskGroupID, productID, age, term, premium, sumAssured, frequencyID, currencyID, minCover, maxCover, relationshipID);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            InsertPremiumRatesHeader(batchID, MediaUploadID, productID, currencyID, riskGroupID, EffectiveDate, AddedBy);
        }
        //See Allocation Rates Template Structure
        public void UploadRates(DataTable RatesDT, Guid MediaUploadID, DateTime EffectiveDate, string Destination, string AddedBy)
        {
            long batchID = Convert.ToInt64(DateTime.Now.ToString("yyMMddmmss")); 
            Guid productID = Guid.Empty;
            int currencyID = 0;
            decimal sumAssured = 0; 
            string delimiter = "";
            int coverHeaderRow = 0;
            object[] ages = new object[0];
            for (int currentRowNo = 0; currentRowNo < RatesDT.Rows.Count; currentRowNo++)
            {
                DataRow DR = RatesDT.Rows[currentRowNo];
                string currentValue = Convert.ToString(DR[0]).Trim();
                if (currentRowNo == 0)
                {
                    string product = Convert.ToString(currentValue).Trim();
                    productID = GetProductID(product);
                }
                //Use Switch to get other delimeters
                switch (currentValue)
                {                    
                    case "Sum Assured":
                        if (!decimal.TryParse(Convert.ToString(RatesDT.Rows[currentRowNo + 1][0].ToString()), out sumAssured))
                        {
                            sumAssured = 0;
                        }
                        break;
                    case "Currency Code":
                        string currency = Convert.ToString(RatesDT.Rows[currentRowNo + 1][0].ToString());
                        currencyID = GetCurrencyID(currency.Trim());
                        break;
                    case "Start Rates":
                        delimiter = currentValue;
                        coverHeaderRow = currentRowNo + 1;
                        ages = RatesDT.Rows[coverHeaderRow].ItemArray;
                        break;
                    case "End Rates":
                        delimiter = "End Rates";
                        break;
                    default:
                        break;
                }

                if (delimiter.Equals("Start Rates"))
                {                   
                    if (currentRowNo > coverHeaderRow)
                    {
                        if (int.TryParse(Convert.ToString(RatesDT.Rows[currentRowNo][0]), out int policyTerm))
                        {                                
                            for (int i = 0; i < ages.Length; i++)
                            {
                                if (i > 0)
                                {
                                    if ((ages[i] != null) && (int.TryParse(ages[i].ToString(), out int age)))
                                    {
                                        if ((RatesDT.Rows[currentRowNo][i] != null) && (decimal.TryParse(RatesDT.Rows[currentRowNo][i].ToString(), out decimal rate)))
                                        {
                                            switch(Destination)
                                            {
                                                case "Cover Rates":
                                                      SaveCoverRates(batchID, age, policyTerm, rate);
                                                    break;
                                                case "Allocation Rates":
                                                       SaveAllocationRates(batchID,age,policyTerm,rate);
                                                    break;
                                                default:
                                                    break;
                                            } 
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            switch (Destination)
            {
                case "Cover Rates":
                    SaveCoverRatesHeader(batchID, MediaUploadID, currencyID, productID, EffectiveDate, sumAssured, AddedBy);
                    break;
                case "Allocation Rates": 
                    SaveAllocationRatesHeader(batchID, MediaUploadID, currencyID, productID,EffectiveDate, sumAssured, AddedBy);
                    break;
                default:
                    break;
            }
        }

        private void InsertPremiumRatesHeader(long BatchID, Guid MediaUploadID, Guid ProductID, int CurrencyID,int RiskGroupID,DateTime EffectiveDate, string AddedBy)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "INSERT INTO [PremiumRatesHeader] ([BatchID],[MediaUploadID],[ProductID],[CurrencyID],[RiskGroupID],[EffectiveDate],[AddedBy],[AddedOn]) VALUES(@BatchID,@MediaUploadID,@ProductID,@CurrencyID,@RiskGroupID,@EffectiveDate,@AddedBy,GETDATE())";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.Parameters.AddWithValue("@MediaUploadID", MediaUploadID);
                    command.Parameters.AddWithValue("@ProductID", ProductID);
                    command.Parameters.AddWithValue("@CurrencyID", CurrencyID);
                    command.Parameters.AddWithValue("@RiskGroupID", RiskGroupID);
                    command.Parameters.AddWithValue("@EffectiveDate", EffectiveDate);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        private int GetRiskParameterID(string Parameter)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @ParameterID int = 0; SELECT TOP(1) @ParameterID=[ID] FROM [RiskParameters] " +
                    "WHERE [Parameter]=@Parameter; SELECT @ParameterID;";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Parameter", Parameter);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        private int GetRiskGroup(List<int> Parameters)
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.AppendLine("DECLARE @RiskGroupID int =-1; SELECT @RiskGroupID=[GroupID] FROM [RiskGroupParameters] WHERE [RiskGroupParameters].[ParameterID] IN ({0}) GROUP BY [GroupID] HAVING COUNT(DISTINCT [RiskGroupParameters].[ParameterID]) = {1}; SELECT @RiskGroupID;");
            string parameterValues = string.Join(",", Parameters);
            int NoOfParameters = Parameters.Count();
            string sql = string.Format(queryBuilder.ToString(), parameterValues, NoOfParameters);
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        private int GetRelationshipID(string Relationship)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @RelationshipID int = 0; SELECT TOP(1) @RelationshipID=[ID] FROM [Relationships] " +
                    "WHERE [Relationship]=@Relationship; SELECT @RelationshipID;";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Relationship", Relationship);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        private int GetFrequencyID(string Frequency)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @FrequencyID int = 0; SELECT TOP(1) @FrequencyID=[ID] FROM [PaymentFrequencies] " +
                    "WHERE [PaymentFrequency]=@Frequency; SELECT @FrequencyID;";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Frequency", Frequency);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        private Guid GetProductID(string Product)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @ProductID uniqueidentifier = '00000000-0000-0000-0000-000000000000'; SELECT @ProductID=[ID] FROM [Products] WHERE [Product]=@Product; SELECT @ProductID;";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Product", Product);
                    string result = command.ExecuteScalar().ToString();
                    return Guid.Parse(result);
                }
            }
        }
        private void SavePremiumRatesHeader(long BatchID, Guid ProductID, int CurrencyID,Guid MediaUploadID, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "INSERT INTO [dbo].[PremiumRatesHeader]([BatchID],[ProductID],[CurrencyID],[MediaUploadID],[AddedBy]) " +
                    "VALUES(@BatchID,@ProductID,@CurrencyID,@MediaUploadID,@AddedBy)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                { 
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.Parameters.AddWithValue("@ProductID", ProductID);
                    command.Parameters.AddWithValue("@CurrencyID", CurrencyID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.Parameters.AddWithValue("@MediaUploadID", MediaUploadID);
                    command.ExecuteNonQuery();
                }
            }
        }
        private void SavePremiumRates(long BatchID, int RiskGroupID, Guid ProductID, int Age, int Term, decimal Premium, decimal SumAssured, int FrequencyID, int CurrencyID, decimal MinimumCover, decimal MaximumCover, int RelationshipID)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "INSERT INTO [PremiumRates]([BatchID],[RiskGroupID],[ProductID],[Age],[Term],[Premium],[SumAssured],[FrequencyID]," +
                    "[CurrencyID],[MinimumCover],[MaximumCover],[RelationshipID])" +
                    "VALUES (@BatchID,@RiskGroupID,@ProductID,@Age,@Term,@Premium,@SumAssured,@FrequencyID,@CurrencyID,@MinimumCover," +
                    "@MaximumCover,@RelationshipID)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@RiskGroupID", RiskGroupID);
                    command.Parameters.AddWithValue("@ProductID", ProductID);
                    command.Parameters.AddWithValue("@Age", Age);
                    command.Parameters.AddWithValue("@Term", Term);
                    command.Parameters.AddWithValue("@Premium", Premium);
                    command.Parameters.AddWithValue("@SumAssured", SumAssured);
                    command.Parameters.AddWithValue("@FrequencyID", FrequencyID);
                    command.Parameters.AddWithValue("@CurrencyID", CurrencyID);
                    command.Parameters.AddWithValue("@MinimumCover", MinimumCover);
                    command.Parameters.AddWithValue("@MaximumCover", MaximumCover);
                    command.Parameters.AddWithValue("@RelationshipID", RelationshipID);
                    command.Parameters.AddWithValue("@BatchID", BatchID); 
                    command.ExecuteNonQuery();
                }
            }
        }
        private void SaveAllocationRatesHeader(long BatchID,Guid MediaUploadID ,int CurrencyID, Guid ProductID,DateTime EffectiveDate, decimal SumAssured, string AddedBy)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "INSERT INTO [AllocationRatesHeader] ([SumAssured],[BatchID],[MediaUploadID],[CurrencyID],[ProductID],[EffectiveDate],[AddedBy],[AddedOn]) " +
                    "VALUES (@SumAssured,@BatchID,@MediaUploadID,@CurrencyID,@ProductID,@EffectiveDate,@AddedBy,GetDate())";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SumAssured", SumAssured);
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.Parameters.AddWithValue("@MediaUploadID", MediaUploadID);
                    command.Parameters.AddWithValue("@CurrencyID", CurrencyID);
                    command.Parameters.AddWithValue("@ProductID", ProductID);
                    command.Parameters.AddWithValue("@EffectiveDate", EffectiveDate);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        private void SaveAllocationRates(long BatchID, int PolicyAge, int PolicyTerm, decimal Rate)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "INSERT INTO [AllocationRates] ([BatchID],[PolicyTerm],[PolicyAge],[Rate]) " +
                    "VALUES (@BatchID, @PolicyTerm,@PolicyAge,@Rate)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyAge", PolicyAge);
                    command.Parameters.AddWithValue("@PolicyTerm", PolicyTerm);
                    command.Parameters.AddWithValue("@Rate", Rate);
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.ExecuteNonQuery();
                }
            }
        }
        private void SaveCoverRatesHeader(long BatchID, Guid MediaUploadID, int CurrencyID, Guid ProductID, DateTime EffectiveDate, decimal SumAssured, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "INSERT INTO [CoverRatesHeader] ([SumAssured],[BatchID],[MediaUploadID],[CurrencyID],[ProductID],[EffectiveDate],[AddedBy],[AddedOn]) " +
                    "VALUES (@SumAssured,@BatchID,@MediaUploadID,@CurrencyID,@ProductID,@EffectiveDate,@AddedBy,GetDate())";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SumAssured", SumAssured);
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.Parameters.AddWithValue("@MediaUploadID", MediaUploadID);
                    command.Parameters.AddWithValue("@CurrencyID", CurrencyID);
                    command.Parameters.AddWithValue("@ProductID", ProductID);
                    command.Parameters.AddWithValue("@EffectiveDate", EffectiveDate);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        private void SaveCoverRates(long BatchID, int PolicyAge, int PolicyTerm, decimal Cover)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "INSERT INTO [CoverRates] ([BatchID],[PolicyTerm],[PolicyAge],[Cover]) " +
                    "VALUES (@BatchID, @PolicyTerm,@PolicyAge,@Cover)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyAge", PolicyAge);
                    command.Parameters.AddWithValue("@PolicyTerm", PolicyTerm);
                    command.Parameters.AddWithValue("@Cover", Cover);
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public DataTable GetLatestPremiumRatesHeaders()
        { 
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PremiumRateFiles_GetLatest";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetLatestCoverRates()
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CoverRateFiles_GetLatest";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetLatestAllocationRates()
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "AllocationRateFiles_GetLatest";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
    }
}