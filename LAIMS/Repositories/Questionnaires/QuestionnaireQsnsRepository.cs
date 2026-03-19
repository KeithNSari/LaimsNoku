using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.Questionnaires;
using Microsoft.Data.SqlClient; 
using System.Data;

namespace LAIMS.Repositories.Questionnaires
{
    public class QuestionnaireQsnsRepository: IQuestionnaireQsnsRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public QuestionnaireQsnsRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public int CheckExistence(QuestionnaireQsn questionnaireQsns)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM QuestionnaireQsns Where ([Questionnaire]=@Questionnaire) And ([Question]=@Question) And ARCHIVED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, questionnaireQsns);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void Create(QuestionnaireQsn questionnaireQsns)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"INSERT INTO QuestionnaireQsns (ID, Questionnaire, Question, AddedBy, AddedOn)
                                 VALUES (@ID, @Questionnaire, @Question, @AddedBy, @AddedOn);";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, questionnaireQsns);
                    command.ExecuteNonQuery();
                }
            }
        }

        public QuestionnaireQsn Read(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM QuestionnaireQsns WHERE ID = @ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapQuestionnaireQsnsFromReader(reader);
                        }
                        return null;
                    }
                }
            }
        }

        public void Update(QuestionnaireQsn questionnaireQsns)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"UPDATE QuestionnaireQsns
                                 SET Questionnaire = @Questionnaire, Question = @Question, AddedBy = @AddedBy, AddedOn = @AddedOn,
                                     Archived = @Archived, ArchivedBy = @ArchivedBy, ArchivedOn = @ArchivedOn
                                 WHERE ID = @ID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, questionnaireQsns);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Archive(Guid id, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE QuestionnaireQsns SET Archived=1,ArchivedOn=@ArchivedOn,ArchivedBy=@ArchivedBy WHERE ID=@ID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("@ArchivedOn", DateTime.Now);
                    command.Parameters.AddWithValue("@ArchivedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<QuestionnaireQsn> GetAll()
        {
            List<QuestionnaireQsn> questionnaireQsnsList = new List<QuestionnaireQsn>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM QuestionnaireQsns;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            questionnaireQsnsList.Add(MapQuestionnaireQsnsFromReader(reader));
                        }
                    }
                }
            }
            return questionnaireQsnsList;
        }
        //public DataTable GetQuestions(Guid EntryID)
        //{
        //    var Database = _configuration.GetConnectionString("DefaultConnection");
        //    DataTable DT = new DataTable();
        //    SqlConnection connection = new SqlConnection();
        //    connection.ConnectionString = Database;
        //    SqlCommand cmd = connection.CreateCommand();
        //    cmd.CommandType = CommandType.Text;
        //    cmd.CommandText = "SELECT ROW_NUMBER() OVER (Order by [QuestionnaireQsns].[EntryNo]) AS [QuestionNo],[QuestionID],[Questions].[Question],STRING_AGG ([ExpectedResponse], ',') AS [Responses],[Questionnaires].[Title] AS [Questionnaire]   FROM [dbo].[QuestionnaireQsns] LEFT JOIN [Questions] ON [QuestionnaireQsns].[Question]=[Questions].[ID] LEFT JOIN  [QuestionExpectedResponses] ON [QuestionExpectedResponses].[QuestionID]=[Questions].[ID] LEFT JOIN [Questionnaires] ON [Questionnaires].[ID]=[QuestionnaireQsns].[Questionnaire] WHERE [QuestionnaireQsns].[Questionnaire]=@EntryID AND [QuestionExpectedResponses].[Archived]=0 AND [Questions].[Archived]=0 AND [QuestionnaireQsns].[Archived]=0 AND [Questionnaires].[Archived]=0 GROUP BY [QuestionID],[Questions].[Question],[QuestionNo],[QuestionnaireQsns].[EntryNo] ORDER BY [Questions].[QuestionNo] Asc";
        //    cmd.Parameters.AddWithValue("EntryID", EntryID);
        //    SqlDataAdapter da = new SqlDataAdapter(cmd);
        //    da.Fill(DT);
        //    return DT;
        //}
        public List<Question> GetQuestions(Guid QuestionnaireID)
        {
            
            List<Question> questions = new List<Question>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT [Questions].[EntryNo],[Questions].[QuestionNo],[Questions].[QuestionLabel],[Questions].[ID] AS [QuestionID],[QuestionTypesID],[Questions].[Question] FROM [dbo].[QuestionnaireQsns] LEFT JOIN [Questions] ON [QuestionnaireQsns].[Question]=[Questions].[ID]WHERE [Questions].[Archived]=0 AND [QuestionnaireQsns].[Archived]=0 AND [Questionnaire]=@Questionnaire GROUP BY [Questions].[ID],[Questions].[Question],[QuestionNo],[Questions].[QuestionLabel],[Questions].[EntryNo],[QuestionnaireQsns].[EntryNo],[QuestionTypesID] ORDER BY [Questions].[QuestionNo] Asc, [Questions].[EntryNo] Asc";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("Questionnaire", QuestionnaireID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Question qsn = MapQuestionFromReader(reader);
                            //recalculate positioning for this questionnaire. Could be done at saving, once off?
                            if (questions.Count == 0)
                            {
                                qsn.Position = 1;
                            }
                            else if (questions.Count>0)
                            {
                                int previousIndex = questions.Count-1;
                                if (qsn.QuestionNo != questions[previousIndex].QuestionNo)
                                {
                                    qsn.Position = questions[previousIndex].Position + 1;
                                }
                                else
                                {
                                    qsn.Position = questions[previousIndex].Position;
                                }
                            }
                            questions.Add(qsn);
                        }
                    }
                }
            }
            using (SqlConnection  con = new SqlConnection(Database))
            {
                SqlCommand command = con.CreateCommand();
                command.Connection = con;
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT * FROM [dbo].[QuestionExpectedResponses] WHERE [Archived]=0 AND [QuestionID]=@QuestionID";
                SqlParameter QuestionIDParm = new()
                {
                    ParameterName = "QuestionID"
                };
                command .Parameters.Add(QuestionIDParm);
                con.Open();
                foreach (Question question in questions)
                {
                    if ((question.QuestionTypeID == 1) || (question.QuestionTypeID == 2))
                    {
                        QuestionIDParm.Value = question.ID;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var expectedResponse = MapExpectedResponseFromReader(reader);  
                                if (!string.IsNullOrEmpty(expectedResponse.ExpectedResponse))
                                {
                                    question.AnswerOptions.Add(expectedResponse);
                                }
                            }
                        }
                    }                   
                }
            }           
            return questions;
        }
        public DataTable GetAllQuestions()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [Questions].[QuestionNo],[Questions].[QuestionLabel],[Questions].[ID] AS [QuestionID],[Questions].[Question],[Questionnaires].[Title] AS [Questionnaire],[Questionnaires].[ID] AS [QuestionnaireID],[QuestionnaireQsns].[ID] AS [QQID] FROM [dbo].[QuestionnaireQsns] LEFT JOIN [Questions] ON [QuestionnaireQsns].[Question]=[Questions].[ID] LEFT JOIN [Questionnaires] ON [Questionnaires].[ID]=[QuestionnaireQsns].[Questionnaire] WHERE [Questions].[Archived]=0 AND [QuestionnaireQsns].[Archived]=0 AND [Questionnaires].[Archived]=0 GROUP BY [Questions].[ID],[Questions].[Question],[QuestionNo],[Questions].[QuestionLabel],[QuestionnaireQsns].[EntryNo],[QuestionnaireQsns].[ID],[Questionnaires].[Title],[Questionnaires].[ID] ORDER BY [Questionnaires].[Title] Asc,[Questions].[QuestionNo] Asc";
            SqlDataAdapter da = new(cmd);
            da.Fill(DT);
            if(DT.Rows .Count  > 0)
            {
                //recalculate question order
                DataTable NDT = new DataTable();
                DataColumn QuestionNoCol = new("QuestionNo");
                NDT.Columns.Add(QuestionNoCol);
                DataColumn QuestionLabelCol = new("QuestionLabel");
                NDT.Columns.Add(QuestionLabelCol);
                DataColumn QuestionIDCol = new("QuestionID");
                NDT.Columns.Add(QuestionIDCol);
                DataColumn QuestionCol = new("Question");
                NDT.Columns.Add(QuestionCol);
                DataColumn QuestionnaireCol = new("Questionnaire");
                NDT.Columns.Add(QuestionnaireCol);
                DataColumn QuestionnaireIDCol = new("QuestionnaireID");
                NDT.Columns.Add(QuestionnaireIDCol);
                DataColumn QQIDCol = new("QQID");
                NDT.Columns.Add(QQIDCol);
                int counter = 0;
                for(int i= 0;i < DT.Rows.Count;i++)
                {
                    if (i > 0)
                    {
                        DataRow currentDR = DT.Rows[i];
                        DataRow previousDR = DT.Rows[i - 1];
                        if (currentDR["Questionnaire"].ToString() == previousDR["Questionnaire"].ToString())
                        {
                            int qNo = Convert.ToInt32(currentDR["QuestionNo"]);
                            int previousqNo = Convert.ToInt32(previousDR["QuestionNo"]);
                            if (qNo > previousqNo)
                            {
                                counter++;
                            }//else it's still the same question, different question part
                        }
                        else
                        {
                            //restart counter, different questionnaire
                            counter = 1;
                        }
                    }
                    else
                    {
                        counter = 1;
                    }
                    NDT.Rows.Add(counter, DT.Rows[i][1], DT.Rows[i][2], DT.Rows[i][3], DT.Rows[i][4], DT.Rows[i][5], DT.Rows[i][6]);                    
                }
                return NDT;
            }
            return DT;
        }
        private void AddParameters(SqlCommand command, QuestionnaireQsn questionnaireQsns)
        {
            command.Parameters.AddWithValue("@ID", questionnaireQsns.ID);
            command.Parameters.AddWithValue("@Questionnaire", questionnaireQsns.Questionnaire);
            command.Parameters.AddWithValue("@Question", questionnaireQsns.Question);
            command.Parameters.AddWithValue("@AddedBy", (object)questionnaireQsns.AddedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@AddedOn", (object)questionnaireQsns.AddedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@Archived", questionnaireQsns.Archived);
            command.Parameters.AddWithValue("@ArchivedBy", (object)questionnaireQsns.ArchivedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedOn", (object)questionnaireQsns.ArchivedOn ?? DBNull.Value);
        }
        private QuestionnaireQsn MapQuestionnaireQsnsFromReader(SqlDataReader reader)
        {
            return new QuestionnaireQsn
            {
                EntryNo = (int)reader["EntryNo"],
                ID = (Guid)reader["ID"],
                Questionnaire = (Guid)reader["Questionnaire"],
                Question = (Guid)reader["Question"],
                AddedBy = reader["AddedBy"] != DBNull.Value ? reader["AddedBy"].ToString() : null,
                AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime)reader["AddedOn"] : (DateTime?)null,
                Archived = (byte)reader["Archived"],
                ArchivedBy = reader["ArchivedBy"] != DBNull.Value ? reader["ArchivedBy"].ToString() : null,
                ArchivedOn = reader["ArchivedOn"] != DBNull.Value ? (DateTime)reader["ArchivedOn"] : (DateTime?)null
            };
        }
        private Question MapQuestionFromReader(SqlDataReader reader)
        {
            return new Question
            {
                EntryNo = (int)reader["EntryNo"],
                ID = (Guid)reader["QuestionID"],
                QuestionNo = (int)reader["QuestionNo"],
                QuestionLabel = reader["QuestionLabel"].ToString(),
                QuestionText = reader["Question"].ToString(),
                QuestionTypeID = (byte)reader["QuestionTypesID"],
                //Weight = reader["Weight"] != DBNull.Value ? (decimal)reader["Weight"] : (decimal?)null,
                //AddedBy = reader["AddedBy"].ToString(),
                //AddedOn = (DateTime)reader["AddedOn"]
            };
        }
        private AnswerOption MapExpectedResponseFromReader(SqlDataReader reader)
        {
            return new AnswerOption 
            { 
                ID = (Guid)reader["ID"], 
                Label = reader["Label"] != DBNull.Value ? reader["Label"].ToString() : string.Empty, 
                ExpectedResponse = reader["ExpectedResponse"].ToString(), 
            };
        }
    }
}
 
