namespace LAIMS.Repositories.Lifeproducts
{
    using LAIMS.Interfaces.Lifeproducts;
    using LAIMS.Models.LifeProducts;
    using LAIMS.Models.Questionnaires;
    using Microsoft.Data.SqlClient;
    using System.Data; 

    public class PolicyTypesQuestionnairesRepository: IPolicyTypesQuestionnairesRepository
    { 
            private IConfiguration _configuration;
            private IWebHostEnvironment _environment;
            public PolicyTypesQuestionnairesRepository(IConfiguration configuration, IWebHostEnvironment environment)
            {
                _configuration = configuration;
                _environment = environment;
            }
            public int CheckExistence(PTQuestionnaire pTQuestionnaire)
            {
                var Database = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(Database))
                {
                    connection.Open();
                    string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM PTQuestionnaires Where ([QuestionnaireID]=@QuestionnaireID) AND ([PolicyTypeID]=@PolicyTypeID) AND (ARCHIVED=0); SELECT @COUNT";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        AddParameters(command, pTQuestionnaire);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            public void InsertPTQuestionnaire(PTQuestionnaire pTQuestionnaire)
            {
                var Database = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(Database))
                {
                    connection.Open();
                    string query = "INSERT INTO [dbo].[PTQuestionnaires]([QuestionnaireID],[PolicyTypeID],[TestedBusiness],[StartAge],[EndAge],[CoverRangeStart],[CoverRangeEnd],[AddedBy],[AddedOn]) VALUES(@QuestionnaireID,@PolicyTypeID,@TestedBusiness,@StartAge,@EndAge,@CoverRangeStart,@CoverRangeEnd,@AddedBy,@AddedOn)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        AddParameters(command, pTQuestionnaire);
                        command.ExecuteNonQuery();
                    }
                }
            }
            public void ArchivePTQuestionnaire(PTQuestionnaire pTQuestionnaire)
            {
                var Database = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(Database))
                {
                    connection.Open();
                    string query = "UPDATE [dbo].[PTQuestionnaires] Set [Archived]=1,[ArchivedBy]=@AddedBy,[ArchivedOn]=@AddedOn WHERE [Id]=@ID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", pTQuestionnaire.ID);
                        command.Parameters.AddWithValue("@AddedBy", pTQuestionnaire.AddedBy);
                        command.Parameters.AddWithValue("@AddedOn", pTQuestionnaire.AddedOn);
                        command.ExecuteNonQuery();
                    }
                }
            }
        public DataTable GetByPolicyType(Guid PolicyTypeID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [PTQuestionnaires].[EntryNo],[PTQuestionnaires].[ID],[PTQuestionnaires].[PolicyTypeID],[PolicyTypes].[Name] As [PolicyType],[PTQuestionnaires].[QuestionnaireID],[Questionnaires].[Title] AS [Questionnaire], CASE [PTQuestionnaires].[TestedBusiness] WHEN 0 THEN 'Untested' WHEN 1 THEN 'Tested' WHEN 2 THEN 'All' END AS [TestedBusiness],[StartAge],[EndAge],[CoverRangeStart],[CoverRangeEnd],[PTQuestionnaires].[AddedOn],[PTQuestionnaires].[AddedBy] FROM [dbo].[PTQuestionnaires] LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[PTQuestionnaires].[PolicyTypeID] LEFT JOIN [Questionnaires] ON [PTQuestionnaires].[QuestionnaireID]=[Questionnaires].[ID] WHERE [PTQuestionnaires].[Archived]=0 AND [PTQuestionnaires].[PolicyTypeID]=@PolicyTypeID ORDER BY [Questionnaires].[Title] ASC";
            cmd.Parameters.AddWithValue("@PolicyTypeID", PolicyTypeID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable Get()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [PTQuestionnaires].[EntryNo],[PTQuestionnaires].[ID],[PTQuestionnaires].[PolicyTypeID],[PolicyTypes].[Name] As [PolicyType],[PTQuestionnaires].[QuestionnaireID],[Questionnaires].[Title] AS [Questionnaire],[PTQuestionnaires].[AddedOn],[PTQuestionnaires].[AddedBy] FROM [dbo].[PTQuestionnaires] LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[PTQuestionnaires].[PolicyTypeID] LEFT JOIN [Questionnaires] ON [PTQuestionnaires].[QuestionnaireID]=[Questionnaires].[ID] WHERE [PTQuestionnaires].[Archived]=0 ORDER BY [PolicyTypes].[Name] ASC, [Questionnaires].[Title] ASC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        private void AddParameters(SqlCommand command, PTQuestionnaire pTQuestionnaire)
        {
            command.Parameters.AddWithValue("@ID", pTQuestionnaire.ID);
            command.Parameters.AddWithValue("@QuestionnaireID", pTQuestionnaire.QuestionnaireID);
            command.Parameters.AddWithValue("@PolicyTypeID", pTQuestionnaire.PolicyTypeID);
            command.Parameters.AddWithValue("@TestedBusiness", pTQuestionnaire.TestedBusiness);
            command.Parameters.AddWithValue("@StartAge", pTQuestionnaire.StartAge);
            command.Parameters.AddWithValue("@EndAge", pTQuestionnaire.EndAge);
            command.Parameters.AddWithValue("@CoverRangeStart", pTQuestionnaire.CoverRangeStart);
            command.Parameters.AddWithValue("@CoverRangeEnd", pTQuestionnaire.CoverRangeEnd);
            command.Parameters.AddWithValue("@AddedBy", pTQuestionnaire.AddedBy);
            command.Parameters.AddWithValue("@AddedOn", pTQuestionnaire.AddedOn);
        }
    } 
}