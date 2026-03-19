using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.Questionnaires;
using Microsoft.Data.SqlClient;
using System.Data;
namespace LAIMS.Repositories.Questionnaires
{ 
        public class QuestionnaireResponseRepository: IQuestionnaireResponseRepository
        {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public QuestionnaireResponseRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public void InsertQuestionnaireResponse(QuestionnaireResponse response)
            {
                using (SqlConnection connection = new SqlConnection(Database))
                {
                    connection.Open();
                    string query = "UPDATE [dbo].[QuestionnaireResponses] SET [Current]=0 WHERE [MemberUID]=@MemberUID AND [Questionnaire]=@Questionnaire; INSERT INTO QuestionnaireResponses (ID, MemberUID, Questionnaire, AddedOn, AddedBy) VALUES (@ID, @MemberUID, @Questionnaire, @AddedOn, @AddedBy)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", response.ID);
                        command.Parameters.AddWithValue("@MemberUID", response.MemberUID);
                        command.Parameters.AddWithValue("@Questionnaire", response.Questionnaire);
                        command.Parameters.AddWithValue("@AddedOn", response.AddedOn);
                        command.Parameters.AddWithValue("@AddedBy", response.AddedBy);
                        command.ExecuteNonQuery();
                    }
                }
            }
        public void PreInsertQuestionnaireResponse(QuestionnaireResponse response)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "QuestionnaireResponses_Add";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ID", response.ID);
                    command.Parameters.AddWithValue("@PolicyID", response.PolicyID);
                    command.Parameters.AddWithValue("@MemberUID", response.MemberUID);
                    command.Parameters.AddWithValue("@Questionnaire", response.Questionnaire);
                    command.Parameters.AddWithValue("@AddedOn", response.AddedOn);
                    command.Parameters.AddWithValue("@AddedBy", response.AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void PreInsertQuestionnaireResponse(QuestionnaireResponse response, string QuestionnaireList)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                //archive all questionnaires not in the list                            
                string query = "UPDATE [dbo].[QuestionnaireResponses] SET [Archived]=1 " +
                "WHERE [MemberUID]=@MemberUID AND [PolicyID]=@PolicyID AND [Questionnaire] NOT IN (" + QuestionnaireList + "); " +
                //unarchive all questionnaires in the list which have been previously archived
                "UPDATE [dbo].[QuestionnaireResponses] SET [Archived]=0 WHERE [MemberUID]=@MemberUID AND [PolicyID]=@PolicyID AND [Questionnaire] " +
                "IN (" + QuestionnaireList + "); " +
                //Add any new entries
                "DECLARE @Count int=0; SELECT @Count=Count(*) FROM [dbo].[QuestionnaireResponses] WHERE [MemberUID]=@MemberUID AND [PolicyID]=@PolicyID " +
                "AND [Questionnaire]=@Questionnaire AND [Archived]=0 IF(@Count=0) " +
                "BEGIN INSERT INTO [dbo].[QuestionnaireResponses]([ID],[PolicyID],[MemberUID],[Questionnaire],[AddedOn],[AddedBy]) " +
                "VALUES (@ID,@PolicyID,@MemberUID,@Questionnaire,@AddedOn,@AddedBy) END "; 
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ID", response.ID);
                    command.Parameters.AddWithValue("@PolicyID", response.PolicyID);
                    command.Parameters.AddWithValue("@MemberUID", response.MemberUID);
                    command.Parameters.AddWithValue("@Questionnaire", response.Questionnaire);
                    command.Parameters.AddWithValue("@AddedOn", response.AddedOn);
                    command.Parameters.AddWithValue("@AddedBy", response.AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        public int CountQuestionnaireResponses(Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=COUNT(*) FROM [dbo].[QuestionnaireResponses] WHERE [PolicyID]=@PolicyID AND [Archived]=0; SELECT @Count";
                using (SqlCommand command = new SqlCommand(query, connection))
                { 
                    command.Parameters.AddWithValue("@PolicyID", PolicyID); 
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void ArchiveQuestionnaireResponses(Guid MemberUID,Guid PolicyID,string ArchivedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[QuestionnaireResponses] SET [Archived]=1,[ArchivedOn]=GetDate(),[ArchivedBy]=@ArchivedBy WHERE [MemberUID]=@MemberUID AND [PolicyID]=@PolicyID";
                using (SqlCommand command = new SqlCommand(query, connection))
                { 
                    command.Parameters.AddWithValue("@MemberUID", MemberUID);
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@ArchivedBy", ArchivedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        public List<QuestionnaireResponse> GetQuestionnaireResponseHeaders(Guid PolicyID)
        {         
            List<QuestionnaireResponse> questionnaireResponses= new List<QuestionnaireResponse>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "QuestionnaireResponses_GetHeadersByPolicy";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            QuestionnaireResponse questionnaireResponse = new QuestionnaireResponse
                            {
                                ID = Guid.Parse(reader["ID"].ToString()),
                                QuestionnaireTitle = reader["QuestionnaireTitle"].ToString(),
                                Questionnaire = Guid.Parse(reader["Questionnaire"].ToString()),
                                MemberUID = Guid.Parse(reader["MemberUID"].ToString()),
                                MemberName= reader["MemberName"].ToString(),
                                Submitted = (byte)reader["Submitted"],
                                SubmittedOn = reader["SubmittedOn"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["SubmittedOn"]
                            };
                            questionnaireResponses.Add(questionnaireResponse);
                        }
                    }
                }
            }
            return questionnaireResponses;
        }
        public QuestionnaireResponse GetQuestionnaireResponse(Guid id)
            {
                using (SqlConnection connection = new SqlConnection(Database))
                {
                    connection.Open();
                    string query = "SELECT * FROM QuestionnaireResponses WHERE ID = @ID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapReaderToQuestionnaireResponse(reader);
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }
        public DateTime? GetResponseDate(Guid id)
        {
            DateTime? AddedOn;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT Max([LastUpdatedOn]) AS [LastUpdatedOn] FROM [dbo].[QuestionnaireResponses] Where [Questionnaire]='443baafe-1b1a-46ab-bcfa-fa4566082eda' AND [Current]=1";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    return AddedOn =Convert.ToDateTime(command.ExecuteScalar());
                }
            }
        }
        public DataTable GetResponses(Guid Questionnaire, string MemberUID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Questionnaires_GetResponses";
            cmd.Parameters.AddWithValue("Questionnaire", Questionnaire);
            cmd.Parameters.AddWithValue("MemberUID", MemberUID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void UpdateQuestionnaireResponse(QuestionnaireResponse response)
            {
                using (SqlConnection connection = new SqlConnection(Database))
                {
                    connection.Open();
                    string query = "UPDATE QuestionnaireResponses SET MemberUID = @MemberUID, Questionnaire = @Questionnaire, AddedOn = @AddedOn, AddedBy = @AddedBy WHERE ID = @ID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", response.ID);
                        command.Parameters.AddWithValue("@MemberUID", response.MemberUID);
                        command.Parameters.AddWithValue("@Questionnaire", response.Questionnaire);
                        command.Parameters.AddWithValue("@AddedOn", response.AddedOn);
                        command.Parameters.AddWithValue("@AddedBy", response.AddedBy);

                        command.ExecuteNonQuery();
                    }
                }
            }

            public void DeleteQuestionnaireResponse(Guid id)
            {
                using (SqlConnection connection = new SqlConnection(Database))
                {
                    connection.Open();
                    string query = "DELETE FROM QuestionnaireResponses WHERE ID = @ID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", id);

                        command.ExecuteNonQuery();
                    }
                }
            }

            private QuestionnaireResponse MapReaderToQuestionnaireResponse(SqlDataReader reader)
            {
                return new QuestionnaireResponse
                {
                    EntryNo = (int)reader["EntryNo"],
                    ID = (Guid)reader["ID"],
                    MemberUID = (Guid)reader["MemberUID"],
                    Questionnaire = (Guid)reader["Questionnaire"],
                    AddedOn = (DateTime)reader["AddedOn"],
                    AddedBy = (string)reader["AddedBy"]
                };
            }
        }
}
