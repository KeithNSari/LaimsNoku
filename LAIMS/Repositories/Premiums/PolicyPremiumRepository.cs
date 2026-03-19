using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace LAIMS.Repositories.Premiums
{
    public class PolicyPremiumRepository: IPolicyPremiumRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PolicyPremiumRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public int AddPolicyPremium(PolicyPremium policyPremium)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @RecordID int=0; SELECT @RecordID=[ID] FROM PolicyPremiums WHERE [HeaderID]=@HeaderID; IF(@RecordID=0) " +
                    "BEGIN INSERT INTO PolicyPremiums (HeaderID, PaymentFrequencyID, PaymentMethodID, PaymentProviderID, PremiumPayer, PremiumPayerAccountID, Premium, AuthoriseAutoPayment) " +
                    "VALUES (@HeaderID, @PaymentFrequencyID, @PaymentMethodID, @PaymentProviderID, @PremiumPayer, @PremiumPayerAccountID, @Premium, @AuthoriseAutoPayment); " +
                    "SELECT @RecordID=SCOPE_IDENTITY() END; SELECT @RecordID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@HeaderID",policyPremium.HeaderID);
                    command.Parameters.AddWithValue("@PaymentFrequencyID", (object)policyPremium.PaymentFrequencyID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaymentMethodID", (object)policyPremium.PaymentMethodID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaymentProviderID", (object)policyPremium.PaymentProviderID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PremiumPayer", policyPremium.PremiumPayer);
                    command.Parameters.AddWithValue("@PremiumPayerAccountID", (object)policyPremium.PremiumPayerAccountID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Premium", policyPremium.Premium);
                    command.Parameters.AddWithValue("@AuthoriseAutoPayment", (object)policyPremium.AuthoriseAutoPayment ?? DBNull.Value);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int AddPolicyPremium(PolicyPremium policyPremium, Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @PremiumPayer int=0; SELECT Top(1) @PremiumPayer=[PremiumPayer] FROM [dbo].[PolicyPremiums] WHERE [HeaderID]=@HeaderID ORDER BY [ID] ASC; SELECT @PremiumPayer;" +
                    " DECLARE @RecordID int=0; SELECT @RecordID=[ID] FROM [PolicyPremiums] WHERE [RequestID]=@RequestID;" +
                    " IF(@RecordID=0) " +
                    "BEGIN " +
                    "INSERT INTO PolicyPremiums (RequestID,HeaderID, PaymentFrequencyID, PaymentMethodID, PaymentProviderID, PremiumPayer, PremiumPayerAccountID, Premium, AuthoriseAutoPayment,Approved)" +
                    " VALUES (@RequestID,@HeaderID, @PaymentFrequencyID, @PaymentMethodID, @PaymentProviderID, @PremiumPayer, @PremiumPayerAccountID, @Premium, @AuthoriseAutoPayment,0);" +
                    " SELECT @RecordID=SCOPE_IDENTITY() END; SELECT @RecordID";  
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.Parameters.AddWithValue("@HeaderID", policyPremium.HeaderID);
                    command.Parameters.AddWithValue("@PaymentFrequencyID", (object)policyPremium.PaymentFrequencyID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaymentMethodID", (object)policyPremium.PaymentMethodID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaymentProviderID", (object)policyPremium.PaymentProviderID ?? DBNull.Value);
                    //command.Parameters.AddWithValue("@PremiumPayer", policyPremium.PremiumPayer); //Premium Payer will be the same as for the main Premium
                    command.Parameters.AddWithValue("@PremiumPayerAccountID", (object)policyPremium.PremiumPayerAccountID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Premium", policyPremium.Premium);
                    command.Parameters.AddWithValue("@AuthoriseAutoPayment", (object)policyPremium.AuthoriseAutoPayment ?? DBNull.Value);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int GetPremiumPayer(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @PayerID int=0; SELECT TOP (1) @PayerID=[PremiumPayer] FROM [dbo].[PolicyPremiums] WHERE [HeaderID]=@HeaderID AND [Current]=1 ORDER BY [ID] Desc; SELECT @PayerID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HeaderID", PolicyID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public List<PolicyPremium> GetAllPolicyPremiums()
        {
            List<PolicyPremium> policyPremiums = new List<PolicyPremium>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "SELECT * FROM PolicyPremiums";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyPremium policyPremium = MapDataReaderToPolicyPremium(reader);
                            policyPremiums.Add(policyPremium);
                        }
                    }
                }
            }

            return policyPremiums;
        }
        public PolicyPremium GetMainPolicyPremium(Guid PolicyID)
        {
            PolicyPremium policyPremium = new PolicyPremium();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "SELECT TOP(1) * FROM PolicyPremiums WHERE [HeaderID]=@HeaderID AND [Current]=1 ORDER BY [ID] ASC";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@HeaderID", PolicyID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            policyPremium = MapDataReaderToPolicyPremium(reader); 
                        }
                    }
                }
            }
            return policyPremium;
        }
        public PolicyPremium GetPolicyPremiumByRequest(Guid RequestID)
        {
            PolicyPremium policyPremium = new PolicyPremium();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "SELECT TOP(1) * FROM PolicyPremiums WHERE [RequestID]=@RequestID AND [Current]=1 ORDER BY [ID] DESC";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            policyPremium = MapDataReaderToPolicyPremium(reader);
                        }
                    }
                }
            }
            return policyPremium;
        }
        public List<PolicyPremiumIntermediary> GetPolicyPremiumsIntermediaries(int PolicyPremiumID)
        {
            List<PolicyPremiumIntermediary> policyPremiumIntermediaries = new List<PolicyPremiumIntermediary>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "PolicyPremium_GetIntermediaries";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("PolicyPremiumID", PolicyPremiumID); 
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyPremiumIntermediary policyPremiumIntermediary = new()
                            {
                                ID = (int)reader["ID"],
                                PolicyPremiumID = (int)reader["PolicyPremiumID"],
                                IntermediaryID = (int)reader["IntermediaryID"],
                                IntermediaryTypeID = (int)reader["IntermediaryTypeID"]
                            };
                            policyPremiumIntermediaries.Add(policyPremiumIntermediary); 
                        }
                    }
                }
            }
            return policyPremiumIntermediaries;
        }       
        public void UpdatePolicyPremium(PolicyPremium policyPremium)
            {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "UPDATE PolicyPremiums SET HeaderID = @HeaderID, PaymentFrequencyID = @PaymentFrequencyID, " +
                             "PaymentMethodID = @PaymentMethodID, PaymentProviderID = @PaymentProviderID, PremiumPayer = @PremiumPayer, " +
                             "PremiumPayerAccountID = @PremiumPayerAccountID, Premium = @Premium, AuthoriseAutoPayment = @AuthoriseAutoPayment, " +
                             "Current = @Current WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", policyPremium.ID);
                    command.Parameters.AddWithValue("@HeaderID", (object)policyPremium.HeaderID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaymentFrequencyID", (object)policyPremium.PaymentFrequencyID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaymentMethodID", (object)policyPremium.PaymentMethodID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaymentProviderID", (object)policyPremium.PaymentProviderID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PremiumPayer", policyPremium.PremiumPayer);
                    command.Parameters.AddWithValue("@PremiumPayerAccountID", (object)policyPremium.PremiumPayerAccountID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Premium", policyPremium.Premium);
                    command.Parameters.AddWithValue("@AuthoriseAutoPayment", (object)policyPremium.AuthoriseAutoPayment ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Current", policyPremium.Current);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdatePaymentMethod(PolicyPremium policyPremium, Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @Billingdate int=0; " +
                    "IF(@PaymentMethodID=2) BEGIN DECLARE @CurrencyID int; SELECT @CurrencyID=[CurrencyID] FROM [Policy] WHERE [ID]=@HeaderID; SELECT TOP (1) @Billingdate=[Billingdate] FROM [dbo].[PremiumCollectionConfigHeader] WHERE [Archived]=0 AND [PaymentProviderID]=@PaymentProviderID AND [CurrencyID]=@CurrencyID AND [PaymentMethodID]=@PaymentMethodID ORDER BY [ID] ASC END; " +
                    "UPDATE PolicyPremiums SET [PreferredBillingDay]=@Billingdate,PaymentFrequencyID=@PaymentFrequencyID,PaymentMethodID=@PaymentMethodID,PaymentProviderID=@PaymentProviderID,PremiumPayerAccountID=@PremiumPayerAccountID, AuthoriseAutoPayment=@AuthoriseAutoPayment WHERE ID=@ID AND HeaderID=@HeaderID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", policyPremium.ID);
                    command.Parameters.AddWithValue("@HeaderID", PolicyID); 
                    command.Parameters.AddWithValue("@PaymentFrequencyID", (object)policyPremium.PaymentFrequencyID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaymentMethodID", (object)policyPremium.PaymentMethodID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaymentProviderID", (object)policyPremium.PaymentProviderID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PremiumPayerAccountID", (object)policyPremium.PremiumPayerAccountID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AuthoriseAutoPayment", (object)policyPremium.AuthoriseAutoPayment ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }
		public void UpdatePremiumPayer(PolicyPremium policyPremium)
		{
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string sql = "UPDATE [dbo].[PolicyPremiums] SET [PremiumPayer]=@PremiumPayerID WHERE [HeaderID]=@PolicyID";
				using (SqlCommand command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@PremiumPayerID", policyPremium.PremiumPayer);
					command.Parameters.AddWithValue("@PolicyID", policyPremium.HeaderID);
		      		command.ExecuteNonQuery();
				}
			}
		}
		public void UpdatePremiumAmount(Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @TotalContributions decimal (18,2)=0; SELECT @TotalContributions=SUM([Contribution]) FROM [dbo].[PolicyBeneficiariesLines] LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID] WHERE [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiariesLines].[Current]=1 AND [PolicyBeneficiaries].[HeaderID]=@PolicyID; Update [dbo].[PolicyPremiums] SET [Premium]=@TotalContributions WHERE [HeaderID]=@PolicyID AND [Current]=1";
                using (SqlCommand command = new SqlCommand(sql, connection))
                { 
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.ExecuteNonQuery();
                }
            }
        }
		public void UpdateFullPremiumAmount(Guid PolicyID)
		{
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string sql = "Policy_UpdatePremium";
				using (SqlCommand command = new SqlCommand(sql, connection))
				{
                    command.CommandType = CommandType.StoredProcedure; 
					command.Parameters.AddWithValue("@PolicyID", PolicyID);
					command.ExecuteNonQuery();
				}
			}
		}
		public void UpdatePremiumAmount(Guid PolicyID, int PolicyPremiumID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @TotalContributions decimal (18,2)=0; SELECT @TotalContributions=SUM([Contribution]) FROM [dbo].[PolicyBeneficiariesLines] LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID] WHERE [PolicyBeneficiariesLines].[Archived]=0 AND [PolicyBeneficiariesLines].[Current]=1 AND [PolicyBeneficiaries].[HeaderID]=@PolicyID AND [PolicyPremiumID]=@PolicyPremiumID; Update [dbo].[PolicyPremiums] SET [Premium]=@TotalContributions WHERE [HeaderID]=@PolicyID AND [ID]=@PolicyPremiumID AND [Current]=1";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@PolicyPremiumID", PolicyPremiumID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdatePremiumPayer(int PolicyPremiumID, int MemberID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "Update [dbo].[PolicyPremiums] SET [PremiumPayer]=@MemberID WHERE [ID]=@PolicyPremiumID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyPremiumID", PolicyPremiumID);
                    command.Parameters.AddWithValue("@MemberID", MemberID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void ApprovePremium(int PolicyPremiumID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "Update [dbo].[PolicyPremiums] SET [Approved]=1 WHERE [ID]=@PolicyPremiumID AND [Premium]>0";
                using (SqlCommand command = new SqlCommand(sql, connection))
                { 
                    command.Parameters.AddWithValue("@PolicyPremiumID", PolicyPremiumID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public int UpdatePolicyPremium(Guid PolicyID, string AgentCodes, int PreferredBillingDay)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @PolicyPremiumID int=0; SELECT @PolicyPremiumID=[ID] FROM [dbo].[PolicyPremiums] WHERE [HeaderID]=@PolicyID AND [Current]=1 Update [dbo].[PolicyPremiums] SET [PreferredBillingDay]=@PreferredBillingDay, [AgentCodes]=@AgentCodes WHERE [ID]=@PolicyPremiumID; SELECT @PolicyPremiumID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PolicyID", PolicyID); 
                    command.Parameters.AddWithValue("@PreferredBillingDay", PreferredBillingDay); 
                    command.Parameters.AddWithValue("@AgentCodes", AgentCodes);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int UpdatePolicyPremiumIntermediaries(int PolicyPremiumID, string AgentCodes)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Update [dbo].[PolicyPremiums] SET [AgentCodes]=@AgentCodes WHERE [ID]=@PolicyPremiumID; SELECT @PolicyPremiumID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PolicyPremiumID", PolicyPremiumID); 
                    command.Parameters.AddWithValue("@AgentCodes", AgentCodes);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int GetCurrentBillingDay(Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "BillingDay_Get";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    return Convert.ToInt32(command.ExecuteScalar()); 
                }
            }
        }
        public void UpdateBillingDay(Guid PolicyID,int BillingDay)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [dbo].[PolicyPremiums] SET [PreferredBillingDay]=@BillingDay WHERE [HeaderID]=@PolicyID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@BillingDay", BillingDay);
                    command.ExecuteNonQuery();
                }
            }
        }
		public void UpdateStopOrderBillingDay(Guid PolicyID, int PaymentProviderID)
		{
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string sql = "DECLARE @BillingDay int; SELECT TOP(1) @BillingDay=[BillingDate] FROM [PremiumCollectionConfigHeader] WHERE [PaymentProviderID]=@PaymentProviderID AND [Archived]=0; UPDATE [PolicyPremiums] SET [PreferredBillingDay]=@BillingDay WHERE [HeaderID]=@PolicyID";
				using (SqlCommand command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@PolicyID", PolicyID);
					command.Parameters.AddWithValue("@PaymentProviderID", PaymentProviderID);
					command.ExecuteNonQuery();
				}
			}
		}
		public void AddPolicyPremiumIntermediaries(int PolicyPremiumID,string AgentCode)
        {
            int agentID = 0;
            int IntermediaryTypeID = 0;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT Top(1) [ID],[IntermediaryTypeID] FROM [dbo].[Intermediaries] WHERE [Archived]=0 And [AgentCode]=@AgentCode ORDER BY [ID] DESC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AgentCode", AgentCode); 
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            agentID = (int)reader["ID"];
                            IntermediaryTypeID = (int)reader["IntermediaryTypeID"];
                        }
                    }
                }
            }
            if (agentID > 0)
            {
                int ActingIntermediaryType; //2 you are acting as a tied agent, 1 you are acting as an independent agent
                if(IntermediaryTypeID== 1) //Independent Agent
                {
                    ActingIntermediaryType = 1;
                }
                else // all other types are tied agents -replace this check with an SP, to prevent tying to a particular heirachy
                {
                    ActingIntermediaryType = 2;
                }
                AddPolicyPremiumIntermediary(PolicyPremiumID,agentID, IntermediaryTypeID, ActingIntermediaryType);                
            }
        }
        public void AddSupervisors(int PolicyPremiumID)
        {
            List<int> agents = new List<int>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT [IntermediaryID] FROM [dbo].[PolicyPremiumIntermediaries] WHERE ([IntermediaryActingType]=1 OR [IntermediaryActingType]=2) AND [PolicyPremiumID]=@PolicyPremiumID AND [Archived]=0";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PolicyPremiumID", PolicyPremiumID);
                     using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int agentID = (int)reader["IntermediaryID"];
                            agents.Add(agentID);
                        }
                    }
                }
            }
            foreach (int agentID in agents)
            {
                int currentAgentID = agentID;
                int supervisorID = -1;
                while (supervisorID != 0)
                {
                   Intermediary supervisor = GetSupervisor(currentAgentID);
                    if (supervisor !=null)
                    {
                        supervisorID = supervisor.ID;
                        if (supervisorID != 0)
                        {
                            AddPolicyPremiumIntermediary(PolicyPremiumID, supervisorID, supervisor.IntermediaryTypeID, supervisor.IntermediaryTypeID);
                            currentAgentID = supervisorID; //next iteration gets the supervisor for the current supervisor
                        }
                    }
                    else
                    {
                        supervisorID = 0;
                    }                   
                }
            }           
        }
        private void AddPolicyPremiumIntermediary(int PolicyPremiumID, int IntermediaryID, int IntermediaryTypeID, int IntermediaryActingType)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Exists int=0; SELECT @Exists=Count(*) FROM [dbo].[PolicyPremiumIntermediaries] WHERE [PolicyPremiumID]=@PolicyPremiumID AND [IntermediaryID]=@IntermediaryID; IF(@Exists=0) BEGIN INSERT INTO [dbo].[PolicyPremiumIntermediaries]([PolicyPremiumID],[IntermediaryID],[IntermediaryTypeID],[IntermediaryActingType]) VALUES(@PolicyPremiumID,@IntermediaryID,@IntermediaryTypeID,@IntermediaryActingType) END";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PolicyPremiumID", PolicyPremiumID);
                    command.Parameters.AddWithValue("@IntermediaryID", IntermediaryID);
                    command.Parameters.AddWithValue("@IntermediaryTypeID", IntermediaryTypeID);
                    command.Parameters.AddWithValue("@IntermediaryActingType", IntermediaryActingType);
                    command.ExecuteNonQuery();
                }
            }
        }
        private Intermediary GetSupervisor(int IntermediaryID)
        {
            Intermediary supervisor = new Intermediary();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT I1.[ReportsToIntermediaryID], I2.[IntermediaryTypeID] FROM [Intermediaries] I1 LEFT JOIN [Intermediaries] I2 On I1.ReportsToIntermediaryID=I2.[ID] WHERE I1.[ID]=@IntermediaryID";
                using (SqlCommand command = new SqlCommand(query, connection))
                { 
                    command.Parameters.AddWithValue("@IntermediaryID", IntermediaryID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if ((reader["ReportsToIntermediaryID"] != DBNull.Value)&& (reader["IntermediaryTypeID"] != DBNull.Value))
                            {
                                supervisor.ID = (int)reader["ReportsToIntermediaryID"];
                                supervisor.IntermediaryTypeID = (int)reader["IntermediaryTypeID"];
                            } 
                        }
                    }
                }
            }
            return supervisor;
        }
        public DataTable GetPremiumDetails(Guid PolicyID, int ID)
        { 
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyPremium_GetByID";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            cmd.Parameters.AddWithValue("ID", ID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetInitialPremiumAgents(Guid PolicyID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Policy_GetInitialAgents";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID); 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetAgents(int PolicyPremiumID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyPremium_GetAgents";
            cmd.Parameters.AddWithValue("PolicyPremiumID", PolicyPremiumID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public int GetInitialPolicyPremiumID(Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @ID int=0; SELECT TOP(1) @ID=[ID] FROM PolicyPremiums WHERE [HeaderID]=@PolicyID AND [Current]=1 ORDER BY [ID] DESC; SELECT @ID";
                using (SqlCommand command = new(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public DataTable GetLatestOrders()
        { 
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "BillingDocuments_Latest"; 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void DeletePolicyPremium(int policyPremiumId)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "DELETE FROM PolicyPremiums WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", policyPremiumId);

                    command.ExecuteNonQuery();
                }
            }
        } 

        private PolicyPremium MapDataReaderToPolicyPremium(SqlDataReader reader)
        {
            

            return new PolicyPremium
            {
                ID = (int)reader["ID"],
                HeaderID = reader["HeaderID"] != DBNull.Value ? (Guid?)reader["HeaderID"] : null,
                PaymentFrequencyID = reader["PaymentFrequencyID"] != DBNull.Value ? (int?)reader["PaymentFrequencyID"] : null,
                PaymentMethodID = reader["PaymentMethodID"] != DBNull.Value ? (int?)reader["PaymentMethodID"] : null,
                PaymentProviderID = reader["PaymentProviderID"] != DBNull.Value ? (int?)reader["PaymentProviderID"] : null,
                PremiumPayer = (int)reader["PremiumPayer"],
                PremiumPayerAccountID = reader["PremiumPayerAccountID"] != DBNull.Value ? (int?)reader["PremiumPayerAccountID"] : null,
                Premium = reader["Premium"] != DBNull.Value ? (decimal)reader["Premium"] : 0, 
                AuthoriseAutoPayment = reader["AuthoriseAutoPayment"] != DBNull.Value ? (byte?)reader["AuthoriseAutoPayment"] : null,
                Current = (byte)reader["Current"],
            };
        }
        //deprecated
        public decimal GetPremiumRates(Guid ProductID, int PolicyBeneficiaryID, decimal Cover)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "PremiumRates_Get";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductID", ProductID); 
                    command.Parameters.AddWithValue("@Cover", Cover);
                    command.Parameters.AddWithValue("@PolicyBeneficiaryID", PolicyBeneficiaryID);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        public decimal GetPremiumRates(Guid ProductID, int PolicyBeneficiaryID, decimal Cover, int RiskGroupID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "PremiumRates_Get";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductID", ProductID);
                    command.Parameters.AddWithValue("@Cover", Cover);
                    command.Parameters.AddWithValue("@PolicyBeneficiaryID", PolicyBeneficiaryID);
                    command.Parameters.AddWithValue("@RiskGroupID", RiskGroupID);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        public PolicyDates GetPolicyPremiumDates(int PolicyPremiumID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "PolicyPremium_Dates";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyPremiumID", PolicyPremiumID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            PolicyDates policyDates = new()
                            {
                                ProposedStartDate = reader["ProposedStartDate"] != DBNull.Value ? (DateTime?)reader["ProposedStartDate"] : null,
                                PreferredBillingDay = reader["PreferredBillingDay"] != DBNull.Value ? (int)reader["PreferredBillingDay"]: 0,
                                ClientSignedDate = reader["ClientSignedDate"] != DBNull.Value ? (DateTime?)reader["ClientSignedDate"] : null,
                                AgentSignedDate = reader["AgentSignedDate"] != DBNull.Value ? (DateTime?)reader["AgentSignedDate"] : null,
                                DateApplicationReceived = reader["DateApplicationReceived"] != DBNull.Value ? (DateTime?)reader["DateApplicationReceived"] : null,
                                CommencementDate = reader["CommencementDate"] != DBNull.Value ? (DateTime?)reader["CommencementDate"] : null,
                                DeductionStartDate = reader["DeductionStartDate"] != DBNull.Value ? (DateTime?)reader["DeductionStartDate"] : null,
                                SystemDate = reader["SystemDate"] != DBNull.Value ? (DateTime?)reader["SystemDate"] : null,
                            };
                            return policyDates;
                        }
                        return null;
                    }
                }
            }
        }
        public CoverDetails GetCoverDetails(Guid  PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Cover_GetDetails";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            CoverDetails coverDetails = new()
                            {
                                PolicyID=PolicyID,
                                Cover=Convert.ToDecimal(reader["Cover"]),
                                Premium=Convert.ToDecimal(reader["Contribution"])
                            };
                            return coverDetails;
                        }
                        return null;
                    }
                }
            }
        }
        public void ArchiveBeneficiaryPremiumLines(Guid PolicyID, int MemberID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "PolicyBeneficiariesLines_Archive";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@MemberID", MemberID);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
