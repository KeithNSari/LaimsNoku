using LAIMS.Interfaces.BusinessRules;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Questionnaires;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.BusinessRules
{
    public class BusinessRuleRepository : IBusinessRuleRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public BusinessRuleRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public int CheckExistence(BusinessRule businessRule)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM  [dbo].[Rules] WHERE [RuleName]=@RuleName AND [StoredProcedure]=@StoredProcedure AND [Archived]=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RuleName", businessRule.RuleName);
                    command.Parameters.AddWithValue("@StoredProcedure", businessRule.StoredProcedure);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public List<string> GetStoredProcedures()
        {
            List<string> procedures = new List<string>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT ROUTINE_NAME AS 'ProcedureName' FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE' AND ROUTINE_NAME Like 'Rule%'  ORDER BY ROUTINE_NAME ASC;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            procedures.Add(reader["ProcedureName"].ToString());
                        }
                    }
                }
            }

            return procedures;
        }
        public void CreateBusinessRule(BusinessRule businessRule)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "INSERT INTO Rules (RuleName, StoredProcedure, ID, AddedOn, AddedBy) " +
                               "VALUES (@RuleName, @StoredProcedure, @ID, @AddedOn, @AddedBy)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RuleName", businessRule.RuleName);
                    command.Parameters.AddWithValue("@StoredProcedure", businessRule.StoredProcedure);
                    command.Parameters.AddWithValue("@ID", businessRule.ID);
                    command.Parameters.AddWithValue("@AddedOn", businessRule.AddedOn);
                    command.Parameters.AddWithValue("@AddedBy", businessRule.AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        public BusinessRule GetBusinessRuleById(int entryNo)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM Rules WHERE EntryNo = @EntryNo";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EntryNo", entryNo);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToBusinessRule(reader);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public List<BusinessRule> GetAllBusinessRules()
        {
            List<BusinessRule> businessRules = new List<BusinessRule>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM [dbo].[Rules] WHERE [Archived]=0 AND [RuleType]=1 ORDER BY [RuleName] ASC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BusinessRule businessRule = MapToBusinessRule(reader);
                            businessRules.Add(businessRule);
                        }
                    }
                }
            }
            return businessRules;
        }
        public DataTable Get()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT ROW_NUMBER () OVER ( ORDER BY RuleName Asc, StoredProcedure Asc  ) RowNo, [RuleName],[StoredProcedure],[ID] FROM [dbo].[Rules] WHERE [Archived]=0 ORDER By [RuleName] ASC,[StoredProcedure] ASC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void UpdateBusinessRule(BusinessRule businessRule)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "UPDATE Rules SET RuleName = @RuleName, StoredProcedure = @StoredProcedure, " +
                               "ID = @ID, AddedOn = @AddedOn, AddedBy = @AddedBy WHERE EntryNo = @EntryNo";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EntryNo", businessRule.EntryNo);
                    command.Parameters.AddWithValue("@RuleName", businessRule.RuleName);
                    command.Parameters.AddWithValue("@StoredProcedure", businessRule.StoredProcedure);
                    command.Parameters.AddWithValue("@ID", businessRule.ID);
                    command.Parameters.AddWithValue("@AddedOn", businessRule.AddedOn);
                    command.Parameters.AddWithValue("@AddedBy", businessRule.AddedBy);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void ArchiveBusinessRule(Guid ID, string AddedBy, DateTime AddedOn)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Rules] Set [Archived]=1,[ArchivedBy]=@AddedBy,[ArchivedOn]=@AddedOn WHERE [ID]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", AddedOn);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeleteBusinessRule(int entryNo)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM Rules WHERE EntryNo = @EntryNo";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EntryNo", entryNo);

                    command.ExecuteNonQuery();
                }
            }
        }

        private BusinessRule MapToBusinessRule(SqlDataReader reader)
        {
            return new BusinessRule
            {
                EntryNo = Convert.ToInt32(reader["EntryNo"]),
                RuleName = reader["RuleName"].ToString(),
                StoredProcedure = reader["StoredProcedure"].ToString(),
                ID = Guid.Parse(reader["ID"].ToString()),
                AddedOn = Convert.ToDateTime(reader["AddedOn"]),
                AddedBy = reader["AddedBy"].ToString()
            };
        }
        public List<StatusReport> GetAllChecks(Guid ObjectID, string ValidationGroup, BusinessRulesParameters newBusinessParameters)
        {
            List<StatusReport> statusReports = new List<StatusReport>();
            List<string> procedures = new List<string>();
            List<string> rulesList = new List<string>();
            List<string> ruleIDList = new List<string>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT [Rules].[StoredProcedure],[Rules].[RuleName],[Rules].[ID] AS [RuleID] FROM [dbo].[ObjectRules] LEFT JOIN [Rules] ON [ObjectRules].[RuleID]=[Rules].[ID] WHERE [ObjectRules].[ObjectID]=@ObjectID AND [ObjectRules].[Filter]=@Filter";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ObjectID", ObjectID);
                    command.Parameters.AddWithValue("@Filter", ValidationGroup);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            procedures.Add(reader["StoredProcedure"].ToString());
                            rulesList.Add(reader["RuleName"].ToString());
                            ruleIDList.Add(reader["RuleID"].ToString());
                        }
                    }
                }
            }
            for (int i = 0; i < procedures.Count; i++)
            {
                StatusReport statusReport = ExecuteProcedure(procedures[i], newBusinessParameters);
                statusReport.RuleName = rulesList[i];
                statusReport.RuleID = Guid.Parse(ruleIDList[i]);
                statusReports.Add(statusReport);
            }
            return statusReports;
        }

        public StatusReport CheckRules(Guid ObjectID, string ValidationGroup, BusinessRulesParameters newBusinessParameters)
        {
            StatusReport statusReport = new StatusReport();
            List<string> procedures = new List<string>();
            List<string> rulesList = new List<string>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT [Rules].[StoredProcedure],[Rules].[RuleName]  FROM [dbo].[ObjectRules] LEFT JOIN [Rules] ON [ObjectRules].[RuleID]=[Rules].[ID] WHERE [ObjectRules].[ObjectID]=@ObjectID AND [ObjectRules].[Filter]=@Filter";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ObjectID", ObjectID);
                    command.Parameters.AddWithValue("@Filter", ValidationGroup);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            procedures.Add(reader["StoredProcedure"].ToString());
                            rulesList.Add(reader["RuleName"].ToString());
                        }
                    }
                }
            }
            statusReport.SuccessStatus = -1;// where rules have not been set for an object, statusID will be -1
            for (int i = 0; i < procedures.Count; i++)
            {
                statusReport = ExecuteProcedure(procedures[i], newBusinessParameters);
                if (statusReport.SuccessStatus == 0) //we return the first violated rule
                {
                    statusReport.RuleName = rulesList[i]; //fetch the violated rule
                    return statusReport;
                }
            }
            //if all rules have been matched, we will return 1
            return statusReport;
        }
        private StatusReport ExecuteProcedure(string procedureName, BusinessRulesParameters newBusinessParameters)
        {
            StatusReport statusReport = new StatusReport();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(procedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    //Set Input Parameters
                    SetNewBusinessInputParameters(command, newBusinessParameters);

                    // Output parameters
                    SqlParameter paramSuccessStatus = new SqlParameter("@Status", SqlDbType.TinyInt);
                    paramSuccessStatus.Direction = ParameterDirection.Output;
                    command.Parameters.Add(paramSuccessStatus);

                    SqlParameter paramStatusID = new SqlParameter("@StatusID", SqlDbType.Int);
                    paramStatusID.Direction = ParameterDirection.Output;
                    command.Parameters.Add(paramStatusID);

                    SqlParameter paramStatusReasonID = new SqlParameter("@StatusReasonID", SqlDbType.Int);
                    paramStatusReasonID.Direction = ParameterDirection.Output;
                    command.Parameters.Add(paramStatusReasonID);


                    SqlParameter paramStatusCode = new SqlParameter("@StatusCode", SqlDbType.VarChar, 50);
                    paramStatusCode.Direction = ParameterDirection.Output;
                    command.Parameters.Add(paramStatusCode);

                    SqlParameter paramStatusMessage = new SqlParameter("@StatusMessage", SqlDbType.NVarChar, 500);
                    paramStatusMessage.Direction = ParameterDirection.Output;
                    command.Parameters.Add(paramStatusMessage);

                    // Execute the stored procedure
                    command.ExecuteNonQuery();

                    // Retrieve output parameter values
                    statusReport.SuccessStatus = (byte)paramSuccessStatus.Value;
                    statusReport.StatusID = (int)paramStatusID.Value;
                    statusReport.StatusReasonID = (int)paramStatusReasonID.Value;
                    statusReport.StatusCode = paramStatusCode.Value.ToString();
                    statusReport.StatusMessage = paramStatusMessage.Value.ToString();
                }
            }
            return statusReport;
        }
        private void SetNewBusinessInputParameters(SqlCommand command, BusinessRulesParameters newBusinessParameters)
        {
            SqlCommandBuilder.DeriveParameters(command);
            List<string> parameterList = new List<string>();
            foreach (SqlParameter parameter in command.Parameters)
            {
                parameterList.Add(parameter.ParameterName);
            }
            command.Parameters.Clear();
            foreach (string parameter in parameterList)
            {
                switch (parameter)
                {
                    case "@LifeAssuredMinAge":
                        command.Parameters.AddWithValue("@LifeAssuredMinAge", (object)newBusinessParameters.LifeAssuredMinAge ?? DBNull.Value);
                        break;
                    case "@LifeAssuredMaxAge":
                        command.Parameters.AddWithValue("@LifeAssuredMaxAge", (object)newBusinessParameters.LifeAssuredMaxAge ?? DBNull.Value);
                        break;
                    case "@ProposerMinAge":
                        command.Parameters.AddWithValue("@ProposerMinAge", (object)newBusinessParameters.ProposerMinAge ?? DBNull.Value);
                        break;
                    case "@ProposerMaxAge":
                        command.Parameters.AddWithValue("@ProposerMaxAge", (object)newBusinessParameters.ProposerMaxAge ?? DBNull.Value);
                        break;
                    case "@PremiumPayerMinAge":
                        command.Parameters.AddWithValue("@PremiumPayerMinAge", (object)newBusinessParameters.PremiumPayerMinAge ?? DBNull.Value);
                        break;
                    case "@PremiumPayerMaxAge":
                        command.Parameters.AddWithValue("@PremiumPayerMaxAge", (object)newBusinessParameters.PremiumPayerMaxAge ?? DBNull.Value);
                        break;
                    case "@MinimumTerm":
                        command.Parameters.AddWithValue("@MinimumTerm", (object)newBusinessParameters.MinimumTerm ?? DBNull.Value);
                        break;
                    case "@MaximumTerm":
                        command.Parameters.AddWithValue("@MaximumTerm", (object)newBusinessParameters.MaximumTerm ?? DBNull.Value);
                        break;
                    case "@Gender":
                        command.Parameters.AddWithValue("@Gender", (object)newBusinessParameters.Gender ?? DBNull.Value);
                        break;
                    case "@PolicyID":
                        command.Parameters.AddWithValue("@PolicyID", (object)newBusinessParameters.PolicyID ?? DBNull.Value);
                        break;
                    case "@PolicyTypeID":
                        command.Parameters.AddWithValue("@PolicyTypeID", (object)newBusinessParameters.PolicyTypeID ?? DBNull.Value);
                        break;
                    case "@ProductID":
                        command.Parameters.AddWithValue("@ProductID", (object)newBusinessParameters.ProductID ?? DBNull.Value);
                        break;
                    case "@ProposerUID":
                        command.Parameters.AddWithValue("@ProposerUID", (object)newBusinessParameters.ProposerUID ?? DBNull.Value);
                        break;
                    case "@ProposerDateOfBirth":
                        command.Parameters.AddWithValue("@ProposerDateOfBirth", (object)newBusinessParameters.ProposerDateOfBirth ?? DBNull.Value);
                        break;
                    case "@ProposerAgeNextBirthday":
                        command.Parameters.AddWithValue("@ProposerAgeNextBirthday", (object)newBusinessParameters.ProposerAgeNextBirthday ?? DBNull.Value);
                        break;
                    case "@ProposerCurrentAge":
                        command.Parameters.AddWithValue("@ProposerCurrentAge", (object)newBusinessParameters.ProposerCurrentAge ?? DBNull.Value);
                        break;
                    case "@MemberUID":
                        command.Parameters.AddWithValue("@MemberUID", (object)newBusinessParameters.MemberUID ?? DBNull.Value);
                        break;
                    case "@PremiumPayerUID":
                        command.Parameters.AddWithValue("@PremiumPayerUID", (object)newBusinessParameters.PremiumPayerUID ?? DBNull.Value);
                        break;
                    case "@PremiumPayerID":
                        command.Parameters.AddWithValue("@PremiumPayerID", (object)newBusinessParameters.PremiumPayerID ?? DBNull.Value);
                        break;
                    case "@PremiumPayerDateOfBirth":
                        command.Parameters.AddWithValue("@PremiumPayerDateOfBirth", (object)newBusinessParameters.PremiumPayerDateOfBirth ?? DBNull.Value);
                        break;
                    case "@PremiumPayerAgeNextBirthday":
                        command.Parameters.AddWithValue("@PremiumPayerAgeNextBirthday", (object)newBusinessParameters.PremiumPayerAgeNextBirthday ?? DBNull.Value);
                        break;
                    case "@PremiumPayerCurrentAge":
                        command.Parameters.AddWithValue("@PremiumPayerCurrentAge", (object)newBusinessParameters.PremiumPayerCurrentAge ?? DBNull.Value);
                        break;
                    case "@PaymentMethodID":
                        command.Parameters.AddWithValue("@PaymentMethodID", (object)newBusinessParameters.PaymentMethodID ?? DBNull.Value);
                        break;
                    case "@PrincipalMemberID":
                        command.Parameters.AddWithValue("@PrincipalMemberID", (object)newBusinessParameters.PrincipalMemberID ?? DBNull.Value);
                        break;
                    case "@PolicyBeneficiary":
                        command.Parameters.AddWithValue("@PolicyBeneficiary", (object)newBusinessParameters.PolicyBeneficiary ?? DBNull.Value);
                        break;
                    case "@BeneficiaryUID":
                        command.Parameters.AddWithValue("@BeneficiaryUID", (object)newBusinessParameters.BeneficiaryUID ?? DBNull.Value);
                        break;
                    case "@BeneficiaryDateOfBirth":
                        command.Parameters.AddWithValue("@BeneficiaryDateOfBirth", (object)newBusinessParameters.BeneficiaryDateOfBirth ?? DBNull.Value);
                        break;
                    case "@BeneficiaryAgeNextBirthday":
                        command.Parameters.AddWithValue("@BeneficiaryAgeNextBirthday", (object)newBusinessParameters.BeneficiaryAgeNextBirthday ?? DBNull.Value);
                        break;
                    case "@BeneficiaryCurrentAge":
                        command.Parameters.AddWithValue("@BeneficiaryCurrentAge", (object)newBusinessParameters.BeneficiaryCurrentAge ?? DBNull.Value);
                        break;
                    case "@Relationship":
                        command.Parameters.AddWithValue("@Relationship", (object)newBusinessParameters.Relationship ?? DBNull.Value);
                        break;
                    case "@ILRoleID":
                        command.Parameters.AddWithValue("@ILRoleID", (object)newBusinessParameters.ILRoleID ?? DBNull.Value);
                        break;
                    case "@Premium":
                        command.Parameters.AddWithValue("@Premium", (object)newBusinessParameters.Premium ?? DBNull.Value);
                        break;
                    case "@Cover":
                        command.Parameters.AddWithValue("@Cover", (object)newBusinessParameters.Cover ?? DBNull.Value);
                        break;
                    case "@Format":
                        command.Parameters.AddWithValue("@Format", (object)newBusinessParameters.DocumentFormat ?? DBNull.Value);
                        break;
                    case "@DocumentID":
                        command.Parameters.AddWithValue("@DocumentID", (object)newBusinessParameters.DocumentID ?? DBNull.Value);
                        break;
					case "@DateSigned":
						command.Parameters.AddWithValue("@DateSigned", (object)newBusinessParameters.DateSigned ?? DBNull.Value);
						break;
                    case "@RequestID":
                        command.Parameters.AddWithValue("@RequestID", (object)newBusinessParameters.RequestID ?? DBNull.Value);
                        break;
                    default:
                        break;
                }
            }
        }
        public void AddPolicyStatiiOverride(PolicyStatiiOverride policyOverride)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"INSERT INTO [dbo].[PolicyStatiiOvverides] (PolicyID, Status, Override, OverridenComment, OverridenBy, OverriddenOn) 
                                 VALUES (@PolicyID, @Status, @Override, @OverridenComment, @OverridenBy, @OverriddenOn)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PolicyID", policyOverride.PolicyID);
                command.Parameters.AddWithValue("@Status", policyOverride.Status);
                command.Parameters.AddWithValue("@Override", policyOverride.Override);
                command.Parameters.AddWithValue("@OverridenComment", (object)policyOverride.OverridenComment ?? DBNull.Value);
                command.Parameters.AddWithValue("@OverridenBy", policyOverride.OverridenBy);
                command.Parameters.AddWithValue("@OverriddenOn", policyOverride.OverriddenOn);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
