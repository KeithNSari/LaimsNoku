using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.Questionnaires;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Questionnaires
{
    public class QuestionnaireResponseLineRepository: IQuestionnaireResponseLineRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public QuestionnaireResponseLineRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public void InsertQuestionnaireResponseLine(QuestionnaireResponseLine responseLine)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Submitted int=0;SELECT @Submitted=[Submitted] FROM [dbo].[QuestionnaireResponses] WHERE [ID]=@HeaderID; IF(@Submitted=0) BEGIN UPDATE [dbo].[QuestionnaireResponses] SET [Submitted]=1 WHERE [ID]=@HeaderID END; UPDATE [dbo].[QuestionnaireResponses] SET [LastUpdatedOn]=GetDate(),[LastUpdatedBy]=@AddedBy WHERE [ID]=@HeaderID; INSERT INTO QuestionnaireResponseLines (HeaderID, QuestionID, ResponseID, ResponseText, AddedOn, AddedBy) VALUES (@HeaderID, @QuestionID, @ResponseID, @ResponseText, @AddedOn, @AddedBy)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HeaderID", responseLine.HeaderID);
                    command.Parameters.AddWithValue("@QuestionID", responseLine.QuestionID);
                    command.Parameters.AddWithValue("@ResponseID", responseLine.ResponseID);
                    command.Parameters.AddWithValue("@ResponseText", responseLine.ResponseText);
                    command.Parameters.AddWithValue("@AddedOn", responseLine.AddedOn);
                    command.Parameters.AddWithValue("@AddedBy", responseLine.AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }

        public QuestionnaireResponseLine GetQuestionnaireResponseLine(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM QuestionnaireResponseLines WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToQuestionnaireResponseLine(reader);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }
        public DataTable GetResponses(Guid Questionnaire, string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [Questions].[Question], STRING_AGG([ExpectedResponse],',') FROM [dbo].[QuestionnaireResponses] LEFT JOIN [QuestionnaireResponseLines] ON [QuestionnaireResponses].[ID]=[QuestionnaireResponseLines].[HeaderID] LEFT JOIN [Questions] ON [QuestionnaireResponseLines].[QuestionID]=[Questions].[ID] LEFT JOIN [QuestionExpectedResponses] ON [QuestionnaireResponseLines].[QuestionID]=[QuestionExpectedResponses].[QuestionID] WHERE [QuestionnaireResponses].[Questionnaire]=@Questionnaire AND [QuestionnaireResponses].[MemberUID] =@MemberUID AND [QuestionnaireResponseLines].[ResponseID]=[QuestionExpectedResponses].[ID] GROUP BY [Questions].[Question]";
            cmd.Parameters.AddWithValue("Questionnaire", Questionnaire);
            cmd.Parameters.AddWithValue("AddedBy", AddedBy);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void UpdateQuestionnaireResponseLine(QuestionnaireResponseLine responseLine)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE QuestionnaireResponseLines SET HeaderID = @HeaderID, QuestionID = @QuestionID, ResponseID = @ResponseID, ResponseText = @ResponseText, AddedOn = @AddedOn, AddedBy = @AddedBy WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", responseLine.ID);
                    command.Parameters.AddWithValue("@HeaderID", responseLine.HeaderID);
                    command.Parameters.AddWithValue("@QuestionID", responseLine.QuestionID);
                    command.Parameters.AddWithValue("@ResponseID", responseLine.ResponseID);
                    command.Parameters.AddWithValue("@ResponseText", responseLine.ResponseText);
                    command.Parameters.AddWithValue("@AddedOn", responseLine.AddedOn);
                    command.Parameters.AddWithValue("@AddedBy", responseLine.AddedBy);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void ArchiveQuestionnaireResponseLines(Guid HeaderID, string ArchivedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE QuestionnaireResponseLines SET [Archived]=1,[ArchivedBy]=@ArchivedBy,[ArchivedOn]=GetUTCDate() WHERE HeaderID=@HeaderID AND [Archived]=0";
                using (SqlCommand command = new SqlCommand(query, connection))
                { 
                    command.Parameters.AddWithValue("@HeaderID", HeaderID); 
                    command.Parameters.AddWithValue("@ArchivedBy", ArchivedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeleteQuestionnaireResponseLine(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM QuestionnaireResponseLines WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        private QuestionnaireResponseLine MapReaderToQuestionnaireResponseLine(SqlDataReader reader)
        {
            return new QuestionnaireResponseLine
            {
                ID = (int)reader["ID"],
                HeaderID = (Guid)reader["HeaderID"],
                QuestionID = (Guid)reader["QuestionID"],
                ResponseID = (Guid)reader["ResponseID"],
                ResponseText = (string)reader["ResponseText"],
                AddedOn = (DateTime)reader["AddedOn"],
                AddedBy = (string)reader["AddedBy"]
            };
        }
    }
}
