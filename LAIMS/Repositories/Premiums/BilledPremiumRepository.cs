using LAIMS.Interfaces.Premiums; 
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient; 
using System.Data;

namespace LAIMS.Repositories.Premiums
{
    public class BilledPremiumRepository: IBilledPremiumRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private readonly string Database;
        public BilledPremiumRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public decimal GetBillAmount(int BillID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @TotalAmount decimal(18,2)=0; SELECT @TotalAmount=[TotalAmount] FROM [dbo].[BillingHeader] WHERE [BillID]=@BillID;SELECT @TotalAmount";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@BillID", BillID);
                   return Convert.ToDecimal(command.ExecuteScalar()); 
                }
            }
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
        // Create
        public void AddBilledPremium(BilledPremium billedPremium)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "INSERT INTO BilledPremiums (BatchID, BillID, PCCID, MemberID, PolicyID, PolicyPremiumID, CurrencyID, Amount, PaymentMethodID, PaymentProviderID, Paid,DueDate, AddedOn) " +
                               "VALUES (@BatchID, @BillID, @PCCID, @MemberID, @PolicyID, @PolicyPremiumID, @CurrencyID, @Amount, @PaymentMethodID, @PaymentProviderID, @Paid,@DueDate, @AddedOn)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, billedPremium);
                    command.ExecuteNonQuery();
                }
            }
        }
        public int AddPremiumHeader(PremiumHeader premiumHeader)
        {
            int headerID = -1;
            using (SqlConnection conn = new SqlConnection(Database))
            {
                conn.Open();
                string sql = "DECLARE @CurrentDate date=GETDATE();INSERT INTO [PremiumHeader] ([BillingID],[BilledPremiumID],[TotalAmount],[DatePaymentReceived],[DatePaymentRecorded],[AddedBy],[AddedOn]) VALUES (@BillingID,@BilledPremiumID,@TotalAmount,@DatePaymentReceived,@CurrentDate,@AddedBy,@CurrentDate);SELECT SCOPE_IDENTITY()";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@BillingID", premiumHeader.BillingID);
                    cmd.Parameters.AddWithValue("@BilledPremiumID", premiumHeader.BilledPremiumID);
                    cmd.Parameters.AddWithValue("@TotalAmount", premiumHeader.TotalAmount);
                    cmd.Parameters.AddWithValue("@DatePaymentReceived", premiumHeader.DatePaymentReceived);
                    cmd.Parameters.AddWithValue("@AddedBy", premiumHeader.AddedBy);
                    headerID = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return headerID;
        }
        public void AddPremiumLine(PremiumLine premiumLine)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "INSERT INTO [PremiumLines] ([PremiumHeaderID],[PaymentMethodID],[PaymentProviderID],[CurrencyID],[Amount],[Reference]) VALUES (@PremiumHeaderID,@PaymentMethodID,@PaymentProviderID,@CurrencyID,@Amount,@Reference)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PremiumHeaderID", premiumLine.PremiumHeaderID);
                    command.Parameters.AddWithValue("@PaymentMethodID", premiumLine.PaymentMethodID);
                    command.Parameters.AddWithValue("@PaymentProviderID", premiumLine.PaymentProviderID);
                    command.Parameters.AddWithValue("@CurrencyID", premiumLine.CurrencyID);
                    command.Parameters.AddWithValue("@Amount", premiumLine.Amount);
                    command.Parameters.AddWithValue("@Reference", (object)premiumLine.Reference ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateBilledPremiums(int BillID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [BilledPremiums] SET [Paid]=1 WHERE [BillID]=@BillID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@BillID", BillID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateBillingHeader(int BillID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [BillingHeader] SET [Paid]=1 WHERE [BillID]=@BillID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@BillID", BillID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdatePolicyBalance(Guid PolicyID,decimal Amount)
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
        public void PremiumBreakDown(int PremiumID, decimal PremiumAmount, Guid PolicyId, int CurrencyID)
        {
            decimal processedAmount = 0;
            foreach (DataRow expenseDR in GetExpenseType(PolicyId).Rows)
            {
                int ExpenseTypeID = Convert.ToInt32(expenseDR["ExpenseTypeID"]);
                int MainProductOnly = Convert.ToInt32(expenseDR["MainProductOnly"]);
                if (MainProductOnly == 1)
                {
                    AddMainProductExpenseType(PremiumID, ExpenseTypeID);
                }
                else
                {
                    decimal calculatedAmount;
                    int policyTypesExpensesID = Convert.ToInt32(expenseDR["ID"]);
                    decimal expenseAmount = Convert.ToDecimal(expenseDR["Amount"]);
                    if (Convert.ToInt32(expenseDR["Ispercentage"]) == 1)
                    {
                        calculatedAmount = PremiumAmount * (expenseAmount / 100);
                    }
                    else
                    {
                        calculatedAmount = expenseAmount;
                    }
                    if (Save(calculatedAmount, PremiumID, policyTypesExpensesID, CurrencyID))
                    {
                        processedAmount += calculatedAmount;
                    }
                }
            }
        }
        private bool Save(decimal Amount, int PremiumID, int PolicyTypesExpensesID, int CurrencyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "SET NOCOUNT OFF;INSERT INTO [PremiumsBreakDown] ([PolicyTypesExpensesID],[CurrencyID],[PremiumID],[Amount]) VALUES (@PolicyTypesExpensesID,@CurrencyID,@PremiumID, @Amount)";
                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    using (connection)
                    {
                        cmd.Parameters.AddWithValue("@Amount", Amount);
                        cmd.Parameters.AddWithValue("@PremiumID", PremiumID);
                        cmd.Parameters.AddWithValue("@PolicyTypesExpensesID", PolicyTypesExpensesID);
                        cmd.Parameters.AddWithValue("@CurrencyID", CurrencyID);
                        return Convert.ToBoolean(cmd.ExecuteNonQuery());
                    }
                }
            }
        }
        private DataTable GetExpenseType(Guid PolicyID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            string query = "SELECT [PolicyTypesExpenses].[ID],[ExpenseTypeID],[Ispercentage],[Amount],[MainProductOnly] FROM [dbo].[PolicyTypesExpenses] " +
                "LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[PolicyTypesExpenses].[PolicyTypeID] " +
                "LEFT JOIN [Policy] ON [Policy].[PolicyType]=[PolicyTypes].[ID] WHERE [PolicyTypesExpenses].[Archived]=0" +
                "AND [Policy].[ID]=@PolicyID";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        private bool AddMainProductExpenseType(int PremiumID, int ExpenseTypeID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PremiumBreakdown_SaveMainProductComponent";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (connection)
                    {
                        cmd.Parameters.AddWithValue("@PremiumID", PremiumID);
                        cmd.Parameters.AddWithValue("@ExpenseTypeID", ExpenseTypeID);
                        return Convert.ToBoolean(cmd.ExecuteNonQuery());
                    }
                }
            }
        }
        // Read
        public BilledPremium GetBilledPremiumById(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM BilledPremiums WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapBilledPremiumFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        public List<BilledPremium> GetAllBilledPremiums()
        {
            List<BilledPremium> billedPremiums = new List<BilledPremium>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM BilledPremiums";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BilledPremium billedPremium = MapBilledPremiumFromReader(reader);
                            billedPremiums.Add(billedPremium);
                        }
                    }
                }
            }

            return billedPremiums;
        }
        public List<BillingSummary> GetPolicyBilledPremiums(string PolicyNo)
        {
            List<BillingSummary> billingSummaries = new List<BillingSummary>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BilledPremiums_SearchByPolicyNo";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BillingSummary billingSummary = new()
                            {
                                BatchID = (int)reader["BatchID"],
                                BillID = (int)reader["BillID"],
                                InvoiceNo = (string)reader["InvoiceNo"],
                                PolicyNo = (string)reader["PolicyNo"],
                                Currency = (string)reader["Currency"],
                                BilledAmount = (decimal)reader["TotalAmount"],
                                Paid = (string)reader["Paid"],
                                DateDue = (string)reader["DateDue"]
                            };
                            billingSummaries.Add(billingSummary);
                        }
                    }
                }
            }
            return billingSummaries;
        }
        public DataTable GetLatest()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new();
            SqlConnection connection = new();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "BilledPremiums_GetLatest";
            SqlDataAdapter da = new(command);
            da.Fill(DT);
            DT = FormatTable(DT,0);
            if (DT.Rows.Count > 0)
            {
                int lastRowIndex = DT.Rows.Count - 1;
                DataRow LastRow = DT.Rows[lastRowIndex];
                DataRow newRow = DT.NewRow();
                newRow["BillID"] = LastRow["BillID"];
                newRow["InvoiceNo"] = "Total";
                newRow["CurrencyID"] = LastRow["CurrencyID"];
                newRow["Currency"] = LastRow["Currency"];
                newRow["Amount"] = LastRow["TotalAmount"];
                newRow["Paid"] = LastRow["Paid"];
                newRow["PolicyID"] = LastRow["PolicyID"];
                DT.Rows.InsertAt(newRow, lastRowIndex+1);
            }
            return DT;
        }
        public DataTable GetFirstUnpaid(string PolicyNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new();
            SqlConnection connection = new();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "BilledPremiums_FirstUnpaid";
            command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
            SqlDataAdapter da = new(command);
            da.Fill(DT); 
            return DT;
        }
        public DataTable Search(string PolicyNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new();
            DT.Columns.Add("BillID", typeof(int));
            DT.Columns.Add("InvoiceNo", typeof(string));
            DT.Columns.Add("CurrencyID", typeof(int));
            DT.Columns.Add("Currency", typeof(string));
            DT.Columns.Add("Amount", typeof(decimal));
            DT.Columns.Add("Paid", typeof(string));
            DT.Columns.Add("PolicyID", typeof(Guid));
            SqlConnection connection = new();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "BilledPremiums_SearchByPolicyNo";
            command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
            SqlDataAdapter da = new(command);
            da.Fill(DT);
            DT = FormatTable(DT, 0);
            if (DT.Rows.Count > 0)
            {
                int lastRowIndex = DT.Rows.Count - 1;
                DataRow LastRow = DT.Rows[lastRowIndex];
                DataRow newRow = DT.NewRow();
                newRow["BillID"] = LastRow["BillID"];
                newRow["InvoiceNo"] = "Total";
                newRow["CurrencyID"] = LastRow["CurrencyID"];
                newRow["Currency"] = LastRow["Currency"];
                newRow["Amount"] = LastRow["TotalAmount"];
                newRow["Paid"] = LastRow["Paid"];
                newRow["PolicyID"] = LastRow["PolicyID"];
                DT.Rows.InsertAt(newRow, lastRowIndex+1);
            }
            return DT;
        }
        private DataTable FormatTable(DataTable DT, int StartIndex)
        {            
            for (int i = StartIndex; i < DT.Rows.Count; i++)
            {               
                if (i > 0)
                {
                    DataRow DR = DT.Rows[i];
                    DataRow PreviousDR = DT.Rows[i - 1];
                    if((DR["InvoiceNo"].ToString()!= "Total") && (PreviousDR["InvoiceNo"].ToString() != "Total"))
                    {//if consecutive entries belong to different bills, insert a total row before that row
                        if ((Convert.ToInt32(DR["BillID"]) != Convert.ToInt32(PreviousDR["BillID"])))
                        {
                            DataRow newRow = DT.NewRow();
                            newRow["BillID"] = PreviousDR["BillID"];
                            newRow["InvoiceNo"] = "Total";
                            newRow["CurrencyID"] = PreviousDR["CurrencyID"];
                            newRow["Currency"] = PreviousDR["Currency"];
                            newRow["Amount"] = PreviousDR["TotalAmount"];
                            newRow["Paid"] = PreviousDR["Paid"];
                            newRow["PolicyID"] = PreviousDR["PolicyID"];
                            DT.Rows.InsertAt(newRow, i);
                            StartIndex = i + 1;
                            FormatTable(DT, StartIndex);
                        }
                        else
                        {
                            //if consecutive entries belong to the same bill, e.g. an ivoice with multiple policies, batch and invoice must be left out
                            DR["BatchID"] = DBNull.Value;
                            DR["InvoiceNo"] = string.Empty;
                        }
                    }                       
                } 
            }           
            return DT;
        }
        // Update
        public void UpdateBilledPremium(BilledPremium billedPremium)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE BilledPremiums SET BatchID = @BatchID, BillID = @BillID, PCCID = @PCCID, MemberID = @MemberID, PolicyID = @PolicyID, " +
                               "PolicyPremiumID = @PolicyPremiumID, CurrencyID = @CurrencyID, Amount = @Amount, PaymentMethodID = @PaymentMethodID, " +
                               "PaymentProviderID = @PaymentProviderID, Paid = @Paid, AddedOn = @AddedOn WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, billedPremium);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Delete
        public void DeleteBilledPremium(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM BilledPremiums WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetLatestPayments()
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            string query = "Premiums_GetLatest";
            cmd.CommandText = query; 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetLatestAllocationSuspenseEntries()
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            string query = "AllocationSuspenseEntries_GetLatest";
            cmd.CommandText = query;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetAllocationSuspenseEntries(long BatchID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure; 
            cmd.CommandText = "AllocationSuspenseEntries_GetByBatch";
            cmd.Parameters.AddWithValue("@BatchID", BatchID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetAllocationSuspenseEntries(Guid PolicyID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "AllocationSuspenseEntries_GetByPolicy";
            cmd.Parameters.AddWithValue("@PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetUnPaid(long BatchID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "BilledPremiums_GetUnpaidByBatch";
            cmd.Parameters.AddWithValue("@BatchID", BatchID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetUnPaid(Guid PolicyID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "BilledPremiums_GetUnpaidByPolicyID";
            cmd.Parameters.AddWithValue("@PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable SearchAllocationSuspenseEntries(DateTime PaymentDate)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            string query = "AllocationSuspenseEntries_SearchByDate";
            cmd.CommandText = query;
            SqlParameter paymentDate = new("PaymentDate", DbType.Date);
            paymentDate.Value = PaymentDate;
            cmd.Parameters.Add(paymentDate);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable SearchAllocationSuspenseEntries(string SearchTerm)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            string query = "AllocationSuspenseEntries_Search";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@SearchTerm", SearchTerm);
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
        public DataTable SearchPayments(string SearchTerm)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            string query = "Premiums_Search";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@SearchTerm", SearchTerm);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void ReversePremiums(PremiumReversalParameters parameters)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand("Premiums_Reversal", connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    command.Parameters.AddWithValue("@PremiumHeaderID", parameters.PremiumHeaderID);
                    command.Parameters.AddWithValue("@BillID", parameters.BillID);
                    command.Parameters.AddWithValue("@ReversedBy", parameters.ReversedBy);
                    command.Parameters.AddWithValue("@ReversalReason", parameters.ReversalReason);
                    command.Parameters.AddWithValue("@ReversalComment", parameters.ReversalComment);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        public void RefundPremium(PremiumReversalParameters parameters)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand("Premiums_Refund", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PremiumHeaderID", parameters.PremiumHeaderID);
                    command.Parameters.AddWithValue("@BillID", parameters.BillID);
                    command.Parameters.AddWithValue("@ReversedBy", parameters.ReversedBy);
                    command.Parameters.AddWithValue("@ReversalReason", parameters.ReversalReason);
                    command.Parameters.AddWithValue("@ReversalComment", parameters.ReversalComment);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        // Helper methods
        private void SetParameters(SqlCommand command, BilledPremium billedPremium)
         {
            command.Parameters.AddWithValue("@BatchID", billedPremium.BatchID);
            command.Parameters.AddWithValue("@BillID", (object)billedPremium.BillID ?? DBNull.Value);
            command.Parameters.AddWithValue("@PCCID", billedPremium.PCCID);
            command.Parameters.AddWithValue("@MemberID", billedPremium.MemberID);
            command.Parameters.AddWithValue("@PolicyID", billedPremium.PolicyID);
            command.Parameters.AddWithValue("@PolicyPremiumID", billedPremium.PolicyPremiumID);
            command.Parameters.AddWithValue("@CurrencyID", billedPremium.CurrencyID);
            command.Parameters.AddWithValue("@Amount", billedPremium.Amount);
            command.Parameters.AddWithValue("@PaymentMethodID", billedPremium.PaymentMethodID);
            command.Parameters.AddWithValue("@PaymentProviderID", (object)billedPremium.PaymentProviderID ?? DBNull.Value);
            command.Parameters.AddWithValue("@Paid", (object)billedPremium.Paid ?? DBNull.Value);
            command.Parameters.AddWithValue("@DueDate", (object)billedPremium.DueDate ?? DBNull.Value);            
            command.Parameters.AddWithValue("@AddedOn", (object)billedPremium.AddedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@ID", billedPremium.ID);
        }

        private BilledPremium MapBilledPremiumFromReader(SqlDataReader reader)
        {
            return new BilledPremium
            {
                ID = (int)reader["ID"],
                BatchID = (int)reader["BatchID"],
                BillID = reader["BillID"] != DBNull.Value ? (int?)reader["BillID"] : null,
                PCCID = (int)reader["PCCID"],
                MemberID = (int)reader["MemberID"],
                PolicyID = (Guid)reader["PolicyID"],
                PolicyPremiumID = (int)reader["PolicyPremiumID"],
                CurrencyID = (int)reader["CurrencyID"],
                Amount = (decimal)reader["Amount"],
                PaymentMethodID = (byte)reader["PaymentMethodID"],
                PaymentProviderID = reader["PaymentProviderID"] != DBNull.Value ? (int?)reader["PaymentProviderID"] : null,
                Paid = reader["Paid"] != DBNull.Value ? (byte?)reader["Paid"] : null,
                AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime?)reader["AddedOn"] : null
            };
        }
    }
}
 
