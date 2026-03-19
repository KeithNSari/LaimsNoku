using LAIMS.Interfaces.Policies;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LAIMS.Repositories.Policies
{
    public class PolicyRepository: IPolicyRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PolicyRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public void InsertPolicy(Policy policy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"DECLARE @CurrencyID int=0; SELECT @CurrencyID=[CurrencyID] FROM [dbo].[PolicyTypes] WHERE [ID]=@PolicyType; DECLARE @MemberID int=0; SELECT @MemberID=[ID] FROM [Members] WHERE [UID]=@MemberUID; IF(@MemberID>0) BEGIN INSERT INTO Policy (ID, MemberID, PolicyNo, PolicyType,PolicyStatus,PolicyStatusDate,CurrencyID,AddedOn, AddedBy) 
                                 VALUES (@ID, @MemberID, @PolicyNo, @PolicyType, @PolicyStatus,GetUTCDate(),@CurrencyID,@AddedOn, @AddedBy) END";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", policy.ID);
                    command.Parameters.AddWithValue("@MemberUID", policy.MemberUID);
                    command.Parameters.AddWithValue("@PolicyNo", (object)policy.PolicyNo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PolicyType", policy.PolicyType);
                    command.Parameters.AddWithValue("@PolicyStatus", policy.PolicyStatus);
                    command.Parameters.AddWithValue("@AddedOn", (object)policy.AddedOn ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", (object)policy.AddedBy ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }
        public int GetPolicyStatus(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Status int=0; SELECT @Status=[PolicyStatus] FROM [dbo].[Policy] WHERE [ID]=@ID; SELECT @Status";
                using (SqlCommand command = new SqlCommand(query, connection))
                { 
                    command.Parameters.AddWithValue("@ID", ID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public bool UpdatePolicyStatus(Guid ID, int StatusID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Policy] SET [PolicyStatus]=@StatusID,[PolicyStatusDate]=GetUTCDate() WHERE [ID]=@ID AND ([PolicyStatus]<@StatusID)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@StatusID", StatusID);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public bool UpdatePolicyStage(Guid ID, int Stage)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Policy] SET [PolicyStage]=@Stage WHERE [ID]=@ID AND ([PolicyStage]<@Stage)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@Stage", Stage);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public int  GetPolicyStage(Guid ID)
        { 
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Stage int=0; SELECT @Stage=[PolicyStage] FROM [dbo].[Policy] WHERE [ID]=@ID; SELECT @Stage";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID); 
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public DataTable GetBy(string PolicyNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policies_GetByPolicyNo";
            command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public bool UpdatePolicyStatus(Guid ID, int Stage,Guid PolicyStatusRuleID, int StatusID, int? StatusReason, string StatusMessage, string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Policy] SET [PolicyStage]=@Stage,[PolicyStatusRuleID]=@PolicyStatusRuleID,[PolicyStatus]=@StatusID,[PolicyStatusReason]=@PolicyStatusReason,[PolicyStatusComment]=@PolicyStatusComment,[PolicyStatusDate]=GetUTCDate(),[PolicyStatusAddedBy]=@PolicyStatusAddedBy WHERE [ID]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@Stage", Stage);
                    command.Parameters.AddWithValue("@PolicyStatusRuleID", PolicyStatusRuleID);
                    command.Parameters.AddWithValue("@StatusID", StatusID);
                    command.Parameters.AddWithValue("@PolicyStatusComment", StatusMessage);
                    if (StatusReason == null)
                    {
                        command.Parameters.AddWithValue("@PolicyStatusReason", DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@PolicyStatusReason", StatusReason);
                    }
                    command.Parameters.AddWithValue("@PolicyStatusAddedBy", AddedBy);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public bool UpdatePolicyStatus(Guid ID, int StatusID, int? StatusReason, string StatusMessage, string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Policy] SET [PolicyStatus]=@StatusID,[PolicyStatusReason]=@PolicyStatusReason,[PolicyStatusComment]=@PolicyStatusComment,[PolicyStatusDate]=GetUTCDate(),[PolicyStatusAddedBy]=@PolicyStatusAddedBy WHERE [ID]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@StatusID", StatusID);
                    command.Parameters.AddWithValue("@PolicyStatusComment", StatusMessage);
                    if (StatusReason == null)
                    {
                        command.Parameters.AddWithValue("@PolicyStatusReason", DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@PolicyStatusReason", StatusReason);
                    }
                    command.Parameters.AddWithValue("@PolicyStatusAddedBy", AddedBy);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public bool CheckPolicyStatusOwner(Guid ID, int StatusID, string UserID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM [dbo].[Policy] WHERE [PolicyStatus]=@StatusID AND [ID]=@ID AND [PolicyStatusAddedBy]=@PolicyStatusAddedBy; SELECT @Count";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@StatusID", StatusID);
                    command.Parameters.AddWithValue("@PolicyStatusAddedBy", UserID);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public bool UpdatePolicyStatus(Guid ID, int StatusID, string Comment, string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Policy] SET [PolicyStatus]=@StatusID,[PolicyStatusDate]=GetUTCDate(),[PolicyStatusComment]=@PolicyStatusComment,[PolicyStatusAddedBy]=@PolicyStatusAddedBy WHERE [ID]=@ID AND ([PolicyStatus]<@StatusID)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@StatusID", StatusID);
                    command.Parameters.AddWithValue("@PolicyStatusComment", (object)Comment ?? DBNull.Value );
                    command.Parameters.AddWithValue("@PolicyStatusAddedBy", AddedBy); 
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public bool UpdatePolicyStatus(bool OverrideSequence, Guid ID, int Stage, int StatusID, string Comment, string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query;
                if (OverrideSequence)
                {
                    query = "UPDATE [dbo].[Policy] SET [PolicyStage]=@Stage,[PolicyStatus]=@StatusID,[PolicyStatusDate]=GetUTCDate(),[PolicyStatusComment]=@PolicyStatusComment,[PolicyStatusAddedBy]=@PolicyStatusAddedBy WHERE [ID]=@ID AND [PolicyStatus]!=@StatusID";
                }
                else
                {
                    query = "UPDATE [dbo].[Policy] SET [PolicyStage]=@Stage, [PolicyStatus]=@StatusID,[PolicyStatusDate]=GetUTCDate(),[PolicyStatusComment]=@PolicyStatusComment,[PolicyStatusAddedBy]=@PolicyStatusAddedBy WHERE [ID]=@ID AND ([PolicyStatus]<@StatusID)";
                }
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@Stage", Stage);
                    command.Parameters.AddWithValue("@StatusID", StatusID);
                    command.Parameters.AddWithValue("@PolicyStatusComment", (object)Comment ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PolicyStatusAddedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
            if(StatusID==10)
            {
                int randomNo = GenerateRandomNumber(0, 999999);
                char randomLetter = GetRandomLetter();
                char prefix = GetRandomLetter();
                string PolicyMS = GeneratePolicyMS(randomNo, randomLetter);
                GeneratePolicy(DateTime.Now.Year, ID, PolicyMS, randomNo,randomLetter,prefix);
            }
            return true;
        }
        public bool SubmitPolicyApplication(Guid ID, int StatusID,DateTime ApplicationDate, DateTime ProposedStartDate)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Policy] SET [PolicyStatus]=@StatusID,[PolicyStatusDate]=GetUTCDate(),[ApplicationDate]=@ApplicationDate,[ProposedStartDate]=@ProposedStartDate WHERE [ID]=@ID AND ([PolicyStatus]!=@StatusID);";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@StatusID", StatusID); 
                    command.Parameters.AddWithValue("@ApplicationDate", ApplicationDate);
                    command.Parameters.AddWithValue("@ProposedStartDate", ProposedStartDate);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }       
        public bool UpdatePolicyDates(Guid policyId, PolicyDates policyDates)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            string updateQuery = "Policy_UpdateDates";

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    command.Parameters.AddWithValue("@ID", policyId);
                    command.Parameters.AddWithValue("@EffectiveDate", (object)policyDates.EffectiveDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ExpirationDate", (object)policyDates.ExpirationDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ProposedStartDate", (object)policyDates.ProposedStartDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PreferredBillingDay", policyDates.PreferredBillingDay);
                    command.Parameters.AddWithValue("@ClientSignedDate", (object)policyDates.ClientSignedDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AgentSignedDate", (object)policyDates.AgentSignedDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DateApplicationReceived", (object)policyDates.DateApplicationReceived ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DeductionStartDate", (object)policyDates.DeductionStartDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SystemDate", (object)policyDates.SystemDate ?? DBNull.Value);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }

        public ApplicationStats  GetApplicationStats(string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            ApplicationStats applicationStats = new ApplicationStats();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Policy_ApplicationStats";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType=CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            applicationStats.IncompleteApplications= (int)reader["IncompleteApplications"];
                            applicationStats.Completed = (int)reader["Completed"];
                            applicationStats.AddDetails = (int)reader["AddDetails"];
                            applicationStats.AddDocuments = (int)reader["AddDocuments"];
                            applicationStats.Questionnaires = (int)reader["Questionnaires"];
                            applicationStats.PaymentDetails = (int)reader["PaymentDetails"];
                            applicationStats.Submitted = (int)reader["Submitted"];
                        }
                    }
                    return applicationStats;
                }
            }
        }
        public Policy GetPolicyById(Guid policyId)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Policies_GetByID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("PolicyID", policyId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToPolicy(reader);
                        }
                        return null;
                    }
                }
            }
        }
        public PolicyDates GetPolicyDates(Guid policyId)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "Policy_Dates";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    command.Parameters.AddWithValue("@PolicyID", policyId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            PolicyDates policyDates = new()
                            {
                                ApplicationDate = reader["ApplicationDate"] != DBNull.Value ? (DateTime?)reader["ApplicationDate"] : null,
                                EffectiveDate = reader["EffectiveDate"] != DBNull.Value ? (DateTime?)reader["EffectiveDate"] : null,
                                ExpirationDate = reader["ExpirationDate"] != DBNull.Value ? (DateTime?)reader["ExpirationDate"] : null,
                                ProposedStartDate = reader["ProposedStartDate"] != DBNull.Value ? (DateTime?)reader["ProposedStartDate"] : null,
                                PreferredBillingDay = (int)reader["PreferredBillingDay"],
                                ClientSignedDate = reader["ClientSignedDate"] != DBNull.Value ? (DateTime?)reader["ClientSignedDate"] : null,
                                AgentSignedDate = reader["AgentSignedDate"] != DBNull.Value ? (DateTime?)reader["AgentSignedDate"] : null,
                                DateApplicationReceived = reader["DateApplicationReceived"] != DBNull.Value ? (DateTime?)reader["DateApplicationReceived"] : null,
                                CommencementDate = reader["CommencementDate"] != DBNull.Value ? (DateTime?)reader["CommencementDate"] : null,
                                DeductionStartDate = reader["DeductionStartDate"] != DBNull.Value ? (DateTime?)reader["DeductionStartDate"] : null,
                                SystemDate = reader["SystemDate"] != DBNull.Value ? (DateTime?)reader["SystemDate"] : null,
                                AnniversaryDate = reader["AnniversaryDate"] != DBNull.Value ? (DateTime?)reader["AnniversaryDate"] : null,
                                MaturityDate = reader["MaturityDate"] != DBNull.Value ? (DateTime?)reader["MaturityDate"] : null
                            };
                            return policyDates;
                        }
                        return null;
                    }
                }
            }
        }
        public List<Policy> GetAllPolicies()
        {
            List<Policy> policies = new List<Policy>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM Policy";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            policies.Add(MapToPolicy(reader));
                        }
                    }
                }
            }

            return policies;
        }
        public DataTable GetMemberPolicies(Guid MemberUID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policy_GetMemberHistory";
            command.Parameters.AddWithValue("MemberUID", MemberUID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetMemberAssociatedPolicies(Guid MemberUID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policies_GetByMember";
            command.Parameters.AddWithValue("MemberUID", MemberUID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }

        public DataTable GetMemberPolicies()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policies_Get"; 
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetPoliciesByStatusID(int StatusID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policies_GetByStatus";
            command.Parameters.AddWithValue("@StatusID", StatusID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetPoliciesAwaitingApproval()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policies_GetAwaitingApproval"; 
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetPoliciesAwaitingBeneficiaryUpdatesApproval()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policies_GetBeneficiaryUpdatesApproval";
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public int CountAllMyPoliciesAwaitingApproval(string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Policies_AwaitingApprovalCountByUser";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.CommandType = CommandType.StoredProcedure;   
                    return (int)command.ExecuteScalar();
                }
            }
        }
        public int CountAllPoliciesAwaitingApproval()
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Policies_AwaitingApprovalCount";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    return (int)command.ExecuteScalar();
                }
            }
        }
        public DataTable GetPolicyStatusHistory(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policy_StatusHistory";
            command.Parameters.AddWithValue("@PolicyID", PolicyID); 
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable PoliciesSearch(string SearchTerm, int StatusID)
        {
            //if status=0, all entries are searched
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            if (StatusID == 0)
            {
                command.CommandText = "Policies_Search";
            }
            else
            {
                command.CommandText = "Policies_SearchByStatusID";
                command.Parameters.AddWithValue("@StatusID", StatusID);
            }            
            command.Parameters.AddWithValue("@SearchTerm", SearchTerm);  
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable PoliciesSearchByUser(string SearchTerm, string AddedBy)
        { 
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure; 
            command.CommandText = "Policies_SearchByUserID"; 
            command.Parameters.AddWithValue("@SearchTerm", SearchTerm);
            command.Parameters.AddWithValue("@AddedBy", AddedBy);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
		public DataTable Search(string SearchTerm)
		{
			var Database = _configuration.GetConnectionString("DefaultConnection");
			DataTable DT = new DataTable();
			SqlConnection connection = new SqlConnection();
			connection.ConnectionString = Database;
			SqlCommand command = connection.CreateCommand();
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = "Policies_Search";
			command.Parameters.AddWithValue("@SearchTerm", SearchTerm); 
			SqlDataAdapter da = new SqlDataAdapter(command);
			da.Fill(DT);
			return DT;
		}
		public DataTable PoliciesSearchByStatusUpdateUser(string SearchTerm, string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policies_SearchByStatusUpdateUserID";
            command.Parameters.AddWithValue("@SearchTerm", SearchTerm);
            command.Parameters.AddWithValue("@AddedBy", AddedBy);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable PoliciesSearchPostApproved(string SearchTerm)
        { 
            // where status is approved or greater or by surname
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;  
            command.CommandText = "Policies_SearchPostApproved";  
            command.Parameters.AddWithValue("@SearchTerm", SearchTerm);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
		public DataTable PoliciesSearchRiskPolicies(string SearchTerm)
		{
			// where status is approved or greater or by surname
			var Database = _configuration.GetConnectionString("DefaultConnection");
			DataTable DT = new DataTable();
			SqlConnection connection = new SqlConnection();
			connection.ConnectionString = Database;
			SqlCommand command = connection.CreateCommand();
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = "Policies_SearchRiskPolicy";
			command.Parameters.AddWithValue("@SearchTerm", SearchTerm);
			SqlDataAdapter da = new SqlDataAdapter(command);
			da.Fill(DT);
			return DT;
		}
		public DataTable PolicyInvestmentSummary(string PolicyNo)
        {
            // where status is approved or greater or by surname
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PolicyUnits_SearchByPolicy";
            command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public decimal GetInvestmentContentBalance(string PolicyNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection"); 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "SELECT [InvestmentContentBalance] FROM [dbo].[Policy] WHERE [PolicyNo]=@PolicyNo";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            } 
        }
        public DataTable GetLatestApplications(string AddedBy)
		{
			var Database = _configuration.GetConnectionString("DefaultConnection");
			DataTable DT = new DataTable();
			SqlConnection connection = new SqlConnection();
			connection.ConnectionString = Database;
			SqlCommand command = connection.CreateCommand();
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = "Policy_LatestApplications";
			command.Parameters.AddWithValue("@AddedBY", AddedBy);
			SqlDataAdapter da = new SqlDataAdapter(command);
			da.Fill(DT);
			return DT;
		}
        public DataTable GetMySubmissions(string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policies_GetMySubmissions";
            command.Parameters.AddWithValue("@UserID", AddedBy);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetMyReviews(string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policies_GetMyReviews";
            command.Parameters.AddWithValue("@UserID", AddedBy);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetMyWorkQueue(string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policies_GetMyWorkQueue";
            command.Parameters.AddWithValue("@UserID", AddedBy);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable SearchMyWorkQueue(string SearchTerm,string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policies_SearchWorkQueue";
            command.Parameters.AddWithValue("@AddedBy", AddedBy);
            command.Parameters.AddWithValue("@SearchTerm", SearchTerm);
            command.Parameters.AddWithValue("@NormalisedNationalID", SearchTerm.ToUpper().Replace("-", "").Replace(" ", ""));
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetByLatestStatii ()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Policies_GetByLatestStatii"; 
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetRequiredQuestionnaires(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PolicyQuestionnaires_InputList";
            command.Parameters.AddWithValue("@PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetRequiredQuestionnaires(Guid PolicyID, Guid MemberUID, decimal TotalCover)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PolicyQuestionnaires_GetByCover";
            command.Parameters.AddWithValue("@PolicyID", PolicyID);
            command.Parameters.AddWithValue("@MemberUID", MemberUID);
            command.Parameters.AddWithValue("@TotalCover", TotalCover);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public Guid GetProposerUID(string PolicyNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            Guid ID;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @ID int=0; SELECT @ID=[MemberID] FROM [Policy] WHERE [PolicyNo]=@PolicyNo; DECLARE @UID uniqueidentifier; SELECT @UID=[UID] FROM [Members] WHERE [ID]=@ID; SELECT @UID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
                    ID = Guid.Parse(command.ExecuteScalar().ToString());
                }
            }
            return ID;
        }
        public int GetProposerID(string PolicyNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            int ID;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @ID int=0; SELECT @ID=[MemberID] FROM [Policy] WHERE [PolicyNo]=@PolicyNo; SELECT @ID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
                    ID = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return ID;
        }
        public int GetProposerID(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            int ID;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @ID int=0; SELECT @ID=[MemberID] FROM [Policy] WHERE [ID]=@PolicyID; SELECT @ID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    ID = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return ID;
        }
        public Guid GetPolicyID(string PolicyNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection"); 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @ID uniqueidentifier='00000000-0000-0000-0000-000000000000'; SELECT @ID=[ID] FROM [Policy] WHERE [PolicyNo]=@PolicyNo; SELECT @ID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
                   return Guid.Parse(command.ExecuteScalar().ToString());
                }
            } 
        }
        public Guid GetPolicyTypeID(string PolicyNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @ID uniqueidentifier; SELECT @ID=[PolicyType] FROM [Policy] WHERE [PolicyNo]=@PolicyNo; SELECT @ID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
                    return Guid.Parse(command.ExecuteScalar().ToString());
                }
            }
        }
        public bool CheckExistence(string PolicyNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection"); 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM [Policy] WHERE [PolicyNo]=@PolicyNo; SELECT @Count";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            } 
        }
        public void UpdatePolicy(Policy policy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"UPDATE Policy 
                                 SET MemberID = @MemberID, 
                                     PolicyNo = @PolicyNo, 
                                     PolicyType = @PolicyType, 
                                     EffectiveDate = @EffectiveDate, 
                                     AddedOn = @AddedOn, 
                                     AddedBy = @AddedBy 
                                 WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MemberUID", policy.MemberUID);
                    command.Parameters.AddWithValue("@PolicyNo", (object)policy.PolicyNo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PolicyType", policy.PolicyType);
                    command.Parameters.AddWithValue("@EffectiveDate", (object)policy.EffectiveDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", (object)policy.AddedOn ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", (object)policy.AddedBy ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ID", policy.ID);

                    command.ExecuteNonQuery();
                }
            }
        }
        int GenerateRandomNumber(int minValue, int maxValue)
        {
            Random random = new Random();
            int randomNumber = random.Next(minValue, maxValue + 1);
            return randomNumber;
        }
        char GetRandomLetter()
        {
            Random random = new Random();
            int randomNumber = random.Next(26); // Generates a random number between 0 and 25
            char randomLetter = (char)('A' + randomNumber);
            if ((randomLetter == 'I') || (randomLetter == 'O')) 
            {
                return 'T'; //Exclude I or O as these can be confusing when mixed with numbers
            }
            return randomLetter;
        }
        private void GeneratePolicy(int CurrentYear, Guid PolicyID, string PolicyMS, int PolicyNoSeed, char CheckLetter, char PolicyNoPrefix)
        {
            bool success = false;
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM [dbo].[Policy] WHERE [PolicyNoCheckLetter]=@PolicyNoCheckLetter AND [PolicyNoSeed]=@PolicyNoSeed AND [PolicyNoPrefix]=@PolicyNoPrefix AND [Year]=@CurrentYear; IF (@Count=0) BEGIN UPDATE [dbo].[Policy] SET [PolicyMS]=@PolicyMS,[PolicyNoSeed]=@PolicyNoSeed,[PolicyNoCheckLetter]=@PolicyNoCheckLetter,[PolicyNoPrefix]=@PolicyNoPrefix WHERE [ID]=@PolicyID; SET @Count=1; END; SELECT @Count";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@PolicyMS", PolicyMS);
                    command.Parameters.AddWithValue("@PolicyNoSeed", PolicyNoSeed);
                    command.Parameters.AddWithValue("@PolicyNoCheckLetter", CheckLetter);
                    command.Parameters.AddWithValue("@PolicyNoPrefix", PolicyNoPrefix);
                    command.Parameters.AddWithValue("@CurrentYear", CurrentYear);
                    success =Convert.ToBoolean(command.ExecuteScalar());
                }
            }
            if (!success)
            {
                int randomNo = GenerateRandomNumber(0, 999999);
                char randomLetter = GetRandomLetter();
                char prefix = GetRandomLetter();
                string policyMS = GeneratePolicyMS(randomNo, randomLetter);           
                GeneratePolicy(CurrentYear,PolicyID,policyMS,randomNo,randomLetter,prefix);
            }
        }
        string GeneratePolicyMS(int number, char CheckLetter)
        { 
            string formattedString = $"{number:D6}";
            string formattedNumber = $"{formattedString.Substring(0, 3)}-" + CheckLetter + $"-{formattedString.Substring(3, 3)}";
            return formattedNumber;
        }
        public decimal GetInvestmentContentBalance(Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT [InvestmentContentBalance] FROM [dbo].[Policy] WHERE [ID]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ID", PolicyID);
                    return (decimal)command.ExecuteScalar();
                }
            }
        }

        public void DebitInvestmentBalance(Guid policyId,decimal TransactionTotal)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Policy] SET [InvestmentContentTotalDebit]=[InvestmentContentTotalDebit] + @TransactionTotal,[InvestmentContentBalance]=[InvestmentContentBalance]-@TransactionTotal WHERE [ID]=@PolicyID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PolicyID", policyId);
                    command.Parameters.AddWithValue("@TransactionTotal", TransactionTotal);
                    command.ExecuteNonQuery();
                }
            }
        }
        public decimal GetTotalPremiums(Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Premiums_GetTotal";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    return (decimal)command.ExecuteScalar();
                }
            }
        }
        public decimal GetTotalCover(Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @SUM decimal(18,2)=0; SELECT @SUM=ISNULL(SUM([Cover]),0) FROM [dbo].[PolicyBeneficiariesLines] LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[ID]=[PolicyBeneficiariesLines].[HeaderID] WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID AND [PolicyBeneficiariesLines].[Archived]=0; SELECT @SUM";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    return (decimal)command.ExecuteScalar();
                }
            }
        }
        public decimal GetTotalCover(int CurrencyID, Guid MemberUID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "Members_GetTotalCover";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CurrencyID", CurrencyID);
                    command.Parameters.AddWithValue("@MemberUID", MemberUID);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        public void UpdatePolicyTerm(Guid ID, int Term)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Policy] SET [Term]=@Term WHERE [ID]=@ID AND [Term]!=@Term;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@Term", Term);
                    command.ExecuteNonQuery();
                }
            }
        }
        public int GetPolicyTerm(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Term int=0; SELECT @Term=[Term] FROM [dbo].[Policy] WHERE [ID]=@ID; SELECT @Term";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int GetPolicyCurrency(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            int ID;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "SELECT [CurrencyID] FROM [Policy] WHERE [ID]=@PolicyID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    ID = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return ID;
        }
        public int GetPolicyCurrency(string PolicyNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            int ID;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "SELECT [CurrencyID] FROM [Policy] WHERE [PolicyNo]=@PolicyNo";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
                    ID = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return ID;
        }
        public void DeletePolicy(Guid policyId)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM Policy WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", policyId);
                    command.ExecuteNonQuery();
                }
            }
        }
        private Policy MapToPolicy(SqlDataReader reader)
        {
            return new Policy
            {
                EntryNo = Convert.ToInt32(reader["EntryNo"]),
                ID = Guid.Parse(reader["ID"].ToString()),
                //MemberUID = Guid.Parse(reader["MemberUID"].ToString()),
                PolicyNo = reader["PolicyNo"] != DBNull.Value ? reader["PolicyNo"].ToString() : null,
                PolicyType = Guid.Parse(reader["PolicyType"].ToString()),
                PolicyName= reader["PolicyName"] != DBNull.Value ? reader["PolicyName"].ToString() : null,
                CurrencyID = Convert.ToInt32(reader["EntryNo"]),
                CurrencyName= reader["CurrencyName"] != DBNull.Value ? reader["CurrencyName"].ToString() : null,
                InvestmentContentBalance = Convert.ToDecimal(reader["InvestmentContentBalance"]),
                InvestmentContentTotalCredit = Convert.ToDecimal(reader["InvestmentContentTotalCredit"]),
                InvestmentContentTotalDebit = Convert.ToDecimal(reader["InvestmentContentTotalDebit"]),
                EffectiveDate = reader["EffectiveDate"] != DBNull.Value ? (DateTime?)reader["EffectiveDate"] : null,
                AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime?)reader["AddedOn"] : null,
                AddedBy = reader["AddedBy"] != DBNull.Value ? reader["AddedBy"].ToString() : null
            };
        }

        //Policy Servicing
        public void PolicyServicingMessagesAdd(Guid PolicyID, Guid MemberUID, int ChangeTypeID, string Message, string AddedBy)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "INSERT INTO [PolicyServicingMessages]([PolicyID],[MemberUID],[ChangeTypeID],[Message],[AddedBy],[AddedOn]) VALUES (@PolicyID,@MemberUID,@ChangeTypeID,@Message,@AddedBy,GETDATE())";
                using (SqlCommand command = new SqlCommand(sql, connection))
                { 
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@MemberUID", MemberUID);
                    command.Parameters.AddWithValue("@ChangeTypeID", ChangeTypeID);
                    command.Parameters.AddWithValue("@Message", Message);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable PolicyServicingMessagesGet()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            string query = "SELECT TOP(100) [PolicyServicingMessages].[ID],[PolicyServicingChangeTypes].[ChangeType],[Message], [Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName],[PolicyNo],[PolicyServicingMessages].[AddedOn] FROM [dbo].[PolicyServicingMessages] LEFT JOIN [PolicyServicingChangeTypes] ON [PolicyServicingChangeTypes].[ID]=[PolicyServicingMessages].[ChangeTypeID] LEFT JOIN [Members] ON [Members].[UID]=[PolicyServicingMessages].[MemberUID] LEFT JOIN [Policy] ON [Policy].[ID]=[PolicyServicingMessages].[PolicyID] ORDER BY [PolicyServicingMessages].[AddedOn] DESC";
            command.CommandText = query; 
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetPolicyServicingRequests()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            SqlConnection connection = new SqlConnection(Database);
            DataTable DT = new DataTable();
            connection.Open();
            string sql = "PolicyServicingRequests_Select";
            SqlCommand command = new SqlCommand(sql, connection);
            command.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
		public DataTable SearchPolicyServicingRequests(string SearchTerm)
		{
			var Database = _configuration.GetConnectionString("DefaultConnection");
			SqlConnection connection = new SqlConnection(Database);
			DataTable DT = new DataTable();
			connection.Open();
			string sql = "PolicyServicingRequests_Search";
			SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@SearchTerm", SearchTerm);
			command.Parameters.AddWithValue("@NormalisedNationalID", SearchTerm.ToUpper().Replace("-", "").Replace(" ", ""));
			command.CommandType = CommandType.StoredProcedure;
			SqlDataAdapter da = new SqlDataAdapter(command);
			da.Fill(DT);
			return DT;
		} 
		public void InsertPolicyServicingRequests(Guid PolicyID, Guid RequestID, int StatusID, int ChangeTypeID, string AddedBy, DateTime AddedOn, int Archived)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "PolicyServicingRequests_Insert";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.Parameters.AddWithValue("@StatusID", StatusID);
                    command.Parameters.AddWithValue("@ChangeTypeID", ChangeTypeID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", AddedOn);
                    command.Parameters.AddWithValue("@Archived", Archived);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdatePolicyServicingRequests(Guid RequestID, int StatusID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [PolicyServicingRequests] SET [StatusID]=@StatusID WHERE [RequestID]=@RequestID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.Parameters.AddWithValue("@StatusID", StatusID); 
                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetEmploymentRecord(Guid PolicyID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Policy_GetEmploymentRecord";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public string GetEmploymentNo(Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Policy_GetEmploymentRecordNo";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    return command.ExecuteScalar().ToString();
                }
            }
        }

        public int GetPaymentMethod(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @PaymentMethodID int=0; SELECT TOP (1) @PaymentMethodID=[PaymentMethodID] FROM [dbo].[PolicyPremiums] WHERE [Archived]=0 AND [HeaderID]=@PolicyID; SELECT @PaymentMethodID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PolicyID", PolicyID); 
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
    }
}
