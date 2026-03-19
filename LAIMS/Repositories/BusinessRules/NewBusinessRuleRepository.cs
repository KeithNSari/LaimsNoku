using LAIMS.Interfaces.BusinessRules;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Questionnaires;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.NetworkInformation;
namespace LAIMS.Repositories.BusinessRules
{
    public class NewBusinessRuleRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public NewBusinessRuleRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
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
            for(int i = 0; i < procedures.Count; i++)
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

                    // Output parameters
                    SqlParameter paramStatus = new SqlParameter("@Status", SqlDbType.TinyInt);
                    paramStatus.Direction = ParameterDirection.Output;
                    command.Parameters.Add(paramStatus);

                    SqlParameter paramStatusCode = new SqlParameter("@StatusCode", SqlDbType.VarChar, 50);
                    paramStatusCode.Direction = ParameterDirection.Output;
                    command.Parameters.Add(paramStatusCode);

                    SqlParameter paramStatusMessage = new SqlParameter("@StatusMessage", SqlDbType.NVarChar, 500);
                    paramStatusMessage.Direction = ParameterDirection.Output;
                    command.Parameters.Add(paramStatusMessage);

                    //Set Input Parameters
                    SetInputParameters(command, newBusinessParameters);
                    // Execute the stored procedure
                    command.ExecuteNonQuery();

                    // Retrieve output parameter values
                    statusReport.SuccessStatus = (int)paramStatus.Value;
                    statusReport.StatusCode = paramStatusCode.Value.ToString();
                    statusReport.StatusMessage= paramStatusMessage.Value.ToString();
                }
            }
            return statusReport;
        }
        public void SetInputParameters(SqlCommand command, BusinessRulesParameters newBusinessParameters)
        {
            command.Parameters.AddWithValue("@LifeAssuredMinAge", DBNullIfNull(newBusinessParameters.LifeAssuredMinAge));
            command.Parameters.AddWithValue("@LifeAssuredMaxAge", DBNullIfNull(newBusinessParameters.LifeAssuredMaxAge));
            command.Parameters.AddWithValue("@ProposerMinAge", DBNullIfNull(newBusinessParameters.ProposerMinAge));
            command.Parameters.AddWithValue("@ProposerMaxAge", DBNullIfNull(newBusinessParameters.ProposerMaxAge));
            command.Parameters.AddWithValue("@PremiumPayerMinAge", DBNullIfNull(newBusinessParameters.PremiumPayerMinAge));
            command.Parameters.AddWithValue("@PremiumPayerMaxAge", DBNullIfNull(newBusinessParameters.PremiumPayerMaxAge));
            command.Parameters.AddWithValue("@MinimumTerm", DBNullIfNull(newBusinessParameters.MinimumTerm));
            command.Parameters.AddWithValue("@MaximumTerm", DBNullIfNull(newBusinessParameters.MaximumTerm));
            command.Parameters.AddWithValue("@Gender", DBNullIfNull(newBusinessParameters.Gender));
            command.Parameters.AddWithValue("@PolicyID", newBusinessParameters.PolicyID);
            command.Parameters.AddWithValue("@ProposerUID", DBNullIfNull(newBusinessParameters.ProposerUID));
            command.Parameters.AddWithValue("@ProposerDateOfBirth", DBNullIfNull(newBusinessParameters.ProposerDateOfBirth));
            command.Parameters.AddWithValue("@ProposerAgeNextBirthday", DBNullIfNull(newBusinessParameters.ProposerAgeNextBirthday));
            command.Parameters.AddWithValue("@ProposerCurrentAge", DBNullIfNull(newBusinessParameters.ProposerCurrentAge));
            command.Parameters.AddWithValue("@PremiumPayerUID", DBNullIfNull(newBusinessParameters.PremiumPayerUID));
            command.Parameters.AddWithValue("@PremiumPayerDateOfBirth", DBNullIfNull(newBusinessParameters.PremiumPayerDateOfBirth));
            command.Parameters.AddWithValue("@PremiumPayerAgeNextBirthday", DBNullIfNull(newBusinessParameters.PremiumPayerAgeNextBirthday));
            command.Parameters.AddWithValue("@PremiumPayerCurrentAge", DBNullIfNull(newBusinessParameters.PremiumPayerCurrentAge));
            command.Parameters.AddWithValue("@BeneficiaryUID", DBNullIfNull(newBusinessParameters.BeneficiaryUID));
            command.Parameters.AddWithValue("@BeneficiaryDateOfBirth", DBNullIfNull(newBusinessParameters.BeneficiaryDateOfBirth));
            command.Parameters.AddWithValue("@BeneficiaryAgeNextBirthday", DBNullIfNull(newBusinessParameters.BeneficiaryAgeNextBirthday));
            command.Parameters.AddWithValue("@BeneficiaryCurrentAge", DBNullIfNull(newBusinessParameters.BeneficiaryCurrentAge));
            command.Parameters.AddWithValue("@Relationship", DBNullIfNull(newBusinessParameters.Relationship));
            command.Parameters.AddWithValue("@ILRoleID", DBNullIfNull(newBusinessParameters.ILRoleID));
            command.Parameters.AddWithValue("@Premium", DBNullIfNull(newBusinessParameters.Premium));
            command.Parameters.AddWithValue("@Cover", DBNullIfNull(newBusinessParameters.Cover));
            command.Parameters.AddWithValue("@DateSigned", DBNullIfNull(newBusinessParameters.DateSigned));
        }

        private object DBNullIfNull(object value)
        {
            return value ?? DBNull.Value;
        }

    }
}
