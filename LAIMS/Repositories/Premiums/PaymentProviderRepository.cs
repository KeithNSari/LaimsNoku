using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Banking;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Premiums
{
    public class PaymentProviderRepository : IPaymentProviderRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PaymentProviderRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        } 
        public int AddPaymentProvider(PaymentProvider paymentProvider)
        {
            int ID;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @ID int=0; SELECT @ID=[ID] FROM [PaymentProviders] WHERE [MemberID]=@MemberID AND [PaymentMethodID]=@PaymentMethodID AND [Archived]=0; IF(@ID=0) BEGIN INSERT INTO PaymentProviders (MemberID, PaymentMethodID,AddedBy) VALUES (@MemberID,@PaymentMethodID,@AddedBy); SELECT @ID=@@IDENTITY; END SELECT @ID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MemberID", paymentProvider.MemberID);
                    command.Parameters.AddWithValue("@PaymentMethodID", paymentProvider.PaymentMethodID);
                    command.Parameters.AddWithValue("@AddedBy", paymentProvider.AddedBy);
                    ID = Convert.ToInt32(command.ExecuteScalar()); 
                }
            }
            return ID;
        }
        public DataTable GetDebitOrderProviders()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PaymentProviders_DebitOrders";
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        // Read
        public List<PaymentProvider> GetAllPaymentProviders()
        {
            List<PaymentProvider> paymentProviders = new List<PaymentProvider>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "PaymentProviders_Get";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType =CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PaymentProvider paymentProvider = new PaymentProvider
                            {
                                ID = (int)reader["ID"],
                                MemberID = (int)reader["MemberID"],
                                ProviderUID= Guid.Parse(reader["ProviderUID"].ToString()),
                                ProviderName = (string)reader["ProviderName"],
                                PaymentMethodID = (int)reader["PaymentMethodID"],
                                AddedBy = reader["AddedBy"].ToString(),
                                AddedOn = (DateTime)reader["AddedOn"],
                                Archived = (byte)reader["Archived"],
                                ArchivedBy = reader["ArchivedBy"].ToString(),
                                ArchivedOn = reader.IsDBNull(reader.GetOrdinal("ArchivedOn")) ? null : (DateTime?)reader["ArchivedOn"]
                            };
                            paymentProviders.Add(paymentProvider);
                        }
                    }
                }
            }
            return paymentProviders;
        }
        public List<PaymentProvider> GetPaymentProviders(int PaymentMethodID)
        {
            List<PaymentProvider> paymentProviders = new List<PaymentProvider>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "PaymentProviders_GetByPaymentMethod";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("PaymentMethod", PaymentMethodID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PaymentProvider paymentProvider = new PaymentProvider
                            {
                                ID = (int)reader["ID"],
                                MemberID = (int)reader["MemberID"],
                                ProviderUID = Guid.Parse(reader["ProviderUID"].ToString()),
                                ProviderName = (string)reader["ProviderName"],
                                PaymentMethodID = (int)reader["PaymentMethodID"],
                                AddedBy = reader["AddedBy"].ToString(),
                                AddedOn = (DateTime)reader["AddedOn"],
                                Archived = (byte)reader["Archived"],
                                ArchivedBy = reader["ArchivedBy"].ToString(),
                                ArchivedOn = reader.IsDBNull(reader.GetOrdinal("ArchivedOn")) ? null : (DateTime?)reader["ArchivedOn"]
                            };

                            paymentProviders.Add(paymentProvider);
                        }
                    }
                }
            }
            return paymentProviders;
        }
		public List<PaymentProvider> GetDebitOrderProviderList()
		{
			List<PaymentProvider> paymentProviders = new List<PaymentProvider>();

			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();

				string sql = "PaymentProviders_DebitOrders";

				using (SqlCommand command = new SqlCommand(sql, connection))
				{
					command.CommandType = CommandType.StoredProcedure; 
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							PaymentProvider paymentProvider = new PaymentProvider
							{
								ID = (int)reader["ID"],
                                PCCID = (int)reader["PCCID"],
                                MemberID = (int)reader["MemberID"],
								ProviderUID = Guid.Parse(reader["ProviderUID"].ToString()),
								ProviderName = (string)reader["ProviderName"],
								PaymentMethodID = (int)reader["PaymentMethodID"],
								AddedBy = reader["AddedBy"].ToString(),
								AddedOn = (DateTime)reader["AddedOn"],
								Archived = (byte)reader["Archived"],
								ArchivedBy = reader["ArchivedBy"].ToString(),
								ArchivedOn = reader.IsDBNull(reader.GetOrdinal("ArchivedOn")) ? null : (DateTime?)reader["ArchivedOn"]
							};

							paymentProviders.Add(paymentProvider);
						}
					}
				}
			}
			return paymentProviders;
		}
		public List<StopOrder> GetStopOrdersByProvider(int ProviderID)
        {
            List<StopOrder> stopOrders = new List<StopOrder>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "SELECT [PremiumCollectionConfigHeader].[StopOrderName]+'('+ [Currencies].[ShortCode]+')' AS [StopOrderName], [PremiumCollectionConfigHeader].[StopOrderCode],[PremiumCollectionConfigHeader].[ID] AS [PCCID] FROM [PremiumCollectionConfigHeader] LEFT JOIN [Currencies] ON [Currencies].[ID]=[CurrencyID] WHERE [PaymentProviderID]=@PaymentProviderID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@PaymentProviderID", ProviderID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StopOrder stopOrder = new()
                            {
                                StopOrderName = (string)reader["StopOrderName"],
                                PCCID = (int)reader["PCCID"]
                            };
                            stopOrders.Add(stopOrder);
                        }
                    }
                }
            }
            return stopOrders;
        }
        public BankAccountFormat GetBankAccountFormat(int PaymentProviderID)
        {
            BankAccountFormat bankAccountFormat = new();
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DECLARE @BankID int=0; SELECT @BankID=[MemberID] FROM [dbo].[PaymentProviders] WHERE [ID]=@PaymentProviderID; SELECT [BankAccountNoFormat],[BankAccountNoFormatDesc] FROM [dbo].[Banks] WHERE [MemberID]=@BankID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PaymentProviderID", PaymentProviderID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bankAccountFormat.BankAccountNoFormat = reader["BankAccountNoFormat"].ToString();
                            bankAccountFormat.FormatDescription = reader["BankAccountNoFormatDesc"].ToString();
                        }
                    }
                }
            }
            return bankAccountFormat;
        }

        // Update
        public void UpdatePaymentProvider(PaymentProvider paymentProvider)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "UPDATE PaymentProviders SET MemberID = @MemberID, PaymentMethodID = @PaymentMethodID, " +
                             "AddedBy = @AddedBy, AddedOn = @AddedOn, Archived = @Archived, ArchivedBy = @ArchivedBy, " +
                             "ArchivedOn = @ArchivedOn WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MemberID", paymentProvider.MemberID);
                    command.Parameters.AddWithValue("@PaymentMethodID", paymentProvider.PaymentMethodID);
                    command.Parameters.AddWithValue("@AddedBy", paymentProvider.AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", paymentProvider.AddedOn);
                    command.Parameters.AddWithValue("@Archived", paymentProvider.Archived);
                    command.Parameters.AddWithValue("@ArchivedBy", paymentProvider.ArchivedBy);
                    command.Parameters.AddWithValue("@ArchivedOn", (object)paymentProvider.ArchivedOn ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ID", paymentProvider.ID);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Delete
        public void DeletePaymentProvider(int ID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "DELETE FROM PaymentProviders WHERE ID=@ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);

                    command.ExecuteNonQuery();
                }
            }
        }

        //Billing
        public bool AddBatch(int PCCID, long BatchID,int PaymentMethodID, DateTime AddedOn)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BillingBatches_Add";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (connection)
                    { 
                        command.Parameters.AddWithValue("@BatchID", BatchID);
                        command.Parameters.AddWithValue("@PCCID", PCCID);
                        command.Parameters.AddWithValue("@PaymentMethodID", PaymentMethodID);
                        command.Parameters.AddWithValue("@AddedOn", AddedOn);
                        return Convert.ToBoolean(command.ExecuteNonQuery());
                    }
                }
            }
        }
        public void AddBilledPolicies(long BatchID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "INSERT INTO [BilledPolicies] ([BatchID],[BillID],[PCCID],[MemberID],[PremiumPayerID],[PolicyID],[CurrencyID],[Amount],[PaymentMethodID],[PaymentProviderID],[PremiumPayerAccountID],[Paid],[PaymentID],[AdHoc],[DueDate],[AddedOn]) " +
                "SELECT [BatchID],[BillID],[PCCID],[MemberID],[PremiumPayerID],[PolicyID],[CurrencyID],SUM([Amount]),[PaymentMethodID],[PaymentProviderID],[PremiumPayerAccountID],[Paid],[PaymentID],[AdHoc],[DueDate],GETDATE() FROM [dbo].[BilledPremiums] " +
                "WHERE [Reversed]=0 AND [BatchID]=@BatchID " +
                "GROUP BY [BatchID],[BillID],[PCCID],[MemberID],[PremiumPayerID],[PolicyID],[CurrencyID],[PaymentMethodID],[PaymentProviderID],[PremiumPayerAccountID],[Paid],[PaymentID],[AdHoc],[DueDate]";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public bool BillStopOrderPremiums(long BatchID, int PaymentProviderID, int PCCID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BilledPremiums_AddStopOrders";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (connection)
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@BatchID", BatchID);
                        command.Parameters.AddWithValue("@PaymentProviderID", PaymentProviderID);
                        command.Parameters.AddWithValue("@PCCID", PCCID);
                        return Convert.ToBoolean(command.ExecuteNonQuery());
                    }
                }
            }
        }
		public bool BillStopOrderPremiums(long BatchID, int PaymentProviderID, int PCCID, DateTime CollectionDate)
		{
			var Database = _configuration.GetConnectionString("DefaultConnection");
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "BilledPremiums_AddStopOrdersByCollectionDate";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					using (connection)
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@BatchID", BatchID);
						command.Parameters.AddWithValue("@PaymentProviderID", PaymentProviderID);
						command.Parameters.AddWithValue("@PCCID", PCCID);
						command.Parameters.AddWithValue("@CollectionDate", CollectionDate);
						return Convert.ToBoolean(command.ExecuteNonQuery());
					}
				}
			}
		}
		public bool BillDebitOrderPremiums(long BatchID, int PaymentProviderID, int BillingDay, int PCCID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BilledPremiums_AddProviderDebitOrders";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (connection)
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@BatchID", BatchID);
                        command.Parameters.AddWithValue("@PaymentProviderID", PaymentProviderID);
                        command.Parameters.AddWithValue("@BillingDay", BillingDay);
                        command.Parameters.AddWithValue("@PCCID", PCCID);
                        return Convert.ToBoolean(command.ExecuteNonQuery());
                    }
                }
            }
        }
        public bool BillDebitOrderPremiums(long BatchID, int BillingDay, int PCCID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BilledPremiums_AddProviderDebitOrdersByPCCID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (connection)
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@BatchID", BatchID); 
                        command.Parameters.AddWithValue("@BillingDay", BillingDay);
                        command.Parameters.AddWithValue("@PCCID", PCCID);
                        return Convert.ToBoolean(command.ExecuteNonQuery());
                    }
                }
            }
        }
        public bool BillDirectPaymentPremiums(long BatchID, int BillingDay, int CurrencyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BilledPremiums_AddDirectPayments";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (connection)
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@BatchID", BatchID);
                        command.Parameters.AddWithValue("@BillingDay", BillingDay);
                        command.Parameters.AddWithValue("@CurrencyID", CurrencyID);
                        return Convert.ToBoolean(command.ExecuteNonQuery());
                    }
                }
            }
        }
        public void AddBillID(long BatchID, int BillingDay, DateTime LastBilled)
        {
            //make more efficient using while data reader
            int billID = 0;
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [ID],[MemberID],[PaymentProviderID],[CurrencyID],[PaymentMethodID]  FROM [dbo].[BilledPremiums] WHERE [BatchID]=@BatchID  ORDER BY [MemberID] ASC, [CurrencyID] ASC, [PaymentMethodID] ASC,[PaymentProviderID] ASC";
            cmd.Parameters.AddWithValue("@BatchID", BatchID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT); 
            if (DT.Rows.Count > 0)
            {
                //update bill id for first record in batch
                billID = NextBillID();
                int recordID = Convert.ToInt32(DT.Rows[0]["ID"]);
                UpdateBillID(recordID, billID);
            }

            //update bill id for the rest of the records
            for (int i = 1; i < DT.Rows.Count; i++)
            {
                DataRow PreviousDR = DT.Rows[i - 1];
                int PreviousMemberID = Convert.ToInt32(PreviousDR["MemberID"]);
                int PreviousPaymentProviderID = Convert.ToInt32(PreviousDR["PaymentProviderID"]);
                int PreviousCurrencyID = Convert.ToInt32(PreviousDR["CurrencyID"]);
                int PreviousPaymentMethodID = Convert.ToInt32(PreviousDR["PaymentMethodID"]);

                DataRow DR = DT.Rows[i];
                int RecordID = Convert.ToInt32(DR["ID"]);
                int MemberID = Convert.ToInt32(DR["MemberID"]);
                int PaymentProviderID = Convert.ToInt32(DR["PaymentProviderID"]);
                int CurrencyID = Convert.ToInt32(DR["CurrencyID"]);
                int PaymentMethodID = Convert.ToInt32(DR["PaymentMethodID"]);

                if (!((PreviousMemberID == MemberID) && (PreviousPaymentProviderID == PaymentProviderID) && (PreviousCurrencyID == CurrencyID) && (PreviousPaymentMethodID == PaymentMethodID)))
                {
                    billID=NextBillID();// only move to the next bill, if the current record doesn't match the previous
                }
                UpdateBillID(RecordID, billID);
            }
        }
        public bool AddBillHeaders(long BatchID, int BillingDay, DateTime LastBilled)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BillingHeader_Add";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (connection)
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@BatchID", BatchID);
                        // command.Parameters.AddWithValue("@BillingDay", BillingDay);
                        return Convert.ToBoolean(command.ExecuteNonQuery());
                    }
                }
            }
        }
        public int NextBillID()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @NextID int ; SET @NextID = NEXT VALUE FOR dbo.Sequence_BillID; SELECT @NextID ";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (connection)
                    {
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
        }
        public bool UpdateBillID(int RecordID, int BillID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[BilledPremiums] SET [BillID]=@BillID WHERE [ID]=@ID AND [BillID]=0";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (connection)
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@BillID", BillID);
                        command.Parameters.AddWithValue("@ID", RecordID);
                        return Convert.ToBoolean(command.ExecuteNonQuery());
                    }
                }
            }
        }
        public DataTable GetLatestStopOrders()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT TOP(100) [BillingBatches].[Entries],[BillingBatches].[PaidTotalAmount],[BillingBatches].[Paid],[BillingBatches].[BatchID],[BillingBatches].[PCCID],[PremiumCollectionConfigHeader].[PaymentProviderID],[PremiumCollectionConfigHeader].[StoporderName] + '(' + [ShortCode] + ')' AS [Stop Order],[PremiumCollectionConfigHeader].[StopOrderCode],[BillingBatches].[DueDate],[BillingBatches].[AddedOn] AS [DateAdded] FROM [BillingBatches] LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID] = [BillingBatches].[PCCID] LEFT JOIN [Currencies] ON [Currencies].[ID] = [PremiumCollectionConfigHeader].[CurrencyID] WHERE  [PremiumCollectionConfigHeader].[PaymentMethodID]=2 ORDER BY [BillingBatches].[AddedOn] DESC";
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetLatestDebitOrders()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT TOP(100) [BillingBatches].[Entries],[BillingBatches].[PaidTotalAmount],[BillingBatches].[Paid],[BillingBatches].[BatchID],[BillingBatches].[PCCID],[PremiumCollectionConfigHeader].[PaymentProviderID],[Members].[Name1] + '- ' + [Currencies].[Name] AS [Debit Order],[PremiumCollectionConfigHeader].[StopOrderCode],[BillingBatches].[AddedOn] AS [DateAdded] FROM [BillingBatches] LEFT JOIN [PremiumCollectionConfigHeader] ON [PremiumCollectionConfigHeader].[ID] = [BillingBatches].[PCCID] LEFT JOIN [Currencies] ON [Currencies].[ID] = [PremiumCollectionConfigHeader].[CurrencyID] LEFT JOIN [PaymentProviders] ON [PaymentProviders].[ID]=[PremiumCollectionConfigHeader].[PaymentProviderID] LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID]  WHERE  [PremiumCollectionConfigHeader].[PaymentMethodID]=1 ORDER BY [BillingBatches].[AddedOn] DESC";
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetLatestBillingBatches(int PaymentMethodID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "BillingBatches_GetLatest";
            command.Parameters.AddWithValue("PaymentMethodID", PaymentMethodID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetBillingBatchData(long BatchID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "BillingBatches_GetData";
            command.Parameters.AddWithValue("@BatchID", BatchID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public string GetSPName(int PCCID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT [StoredProcedureName] FROM [dbo].[PremiumCollectionConfigHeader] WHERE [ID]=@PCCID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PCCID", PCCID);
                    using (connection)
                    {
                        return command.ExecuteScalar() as string;
                    }
                }
            }
        }
        public string GetDebitOrderName(int PCCID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT [Members].[Name1] + '_' + [Currencies].[Name] + '_'+ Convert(varchar,GetDate(),112) + '_Debit_Order'  AS [Debit Order] FROM [PremiumCollectionConfigHeader] LEFT JOIN [Currencies] ON [Currencies].[ID] = [PremiumCollectionConfigHeader].[CurrencyID] LEFT JOIN [PaymentProviders] ON [PaymentProviders].[ID]=[PremiumCollectionConfigHeader].[PaymentProviderID] LEFT JOIN [Members] ON [Members].[ID]=[PaymentProviders].[MemberID] WHERE  [PremiumCollectionConfigHeader].[ID]=@PCCID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PCCID", PCCID);
                    using (connection)
                    {
                        return command.ExecuteScalar() as string;
                    }
                }
            }
        }
        public int GetFirstPCCID(int PaymentProviderID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @ID int=0; SELECT @ID=[ID] FROM [dbo].[PremiumCollectionConfigHeader] WHERE [PaymentProviderID]=@PaymentProviderID; SELECT @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (connection)
                    { 
                        command.Parameters.AddWithValue("@PaymentProviderID", PaymentProviderID); 
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
        }
        public string GetStopOrderCode(int PCCID)
		{
			var Database = _configuration.GetConnectionString("DefaultConnection");
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "SELECT [StopOrderCode] FROM [dbo].[PremiumCollectionConfigHeader] WHERE [ID]=@PCCID";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@PCCID", PCCID);
					using (connection)
					{
						return command.ExecuteScalar() as string;
					}
				}
			}
		}
        public int GetFileFormat(int PCCID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT [FormatID] FROM [dbo].[PremiumCollectionConfigHeader] WHERE [ID]=@PCCID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PCCID", PCCID);
                    using (connection)
                    {
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
        }
        public DataTable GetStopOrderData(string ProcedureName)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = ProcedureName;
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
		public DataTable GetDebitOrderData(string ProcedureName, long BatchID)
		{
			var Database = _configuration.GetConnectionString("DefaultConnection");
			DataTable DT = new DataTable();
			SqlConnection connection = new SqlConnection();
			connection.ConnectionString = Database;
			SqlCommand command = connection.CreateCommand();            
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = ProcedureName;
			command.Parameters.AddWithValue("@BatchID", BatchID);
			SqlDataAdapter da = new SqlDataAdapter(command);
			da.Fill(DT);
			return DT;
		}
		public DataTable GeStopOrderData(string ProcedureName, int PCCID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = ProcedureName;
            command.Parameters.AddWithValue("PCCID", PCCID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
    }
}
