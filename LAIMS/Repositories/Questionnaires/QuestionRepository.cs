using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Questionnaires;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Questionnaires
{
    public class QuestionRepository: IQuestionRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public QuestionRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public int CheckExistence(Question question)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM Questions Where ([Question]=@QuestionText) And ARCHIVED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, question);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void Create(Question question)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"INSERT INTO Questions (ID, QuestionNo, QuestionLabel, Question, QuestionTypesID, Weight, AddedBy, AddedOn)
                                 VALUES (@ID, @QuestionNo, @QuestionLabel, @QuestionText, @QuestionTypeID, @Weight, @AddedBy, @AddedOn);";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, question);
                    command.ExecuteNonQuery();
                }
            }
        }
        public List<Question> GetAll()
        {
            List<Question> questions = new List<Question>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT  * FROM [dbo].[Questions] Where [Archived]=0 Order By [EntryNo] Asc;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            questions.Add(MapQuestionFromReader(reader));
                        }
                    }
                }
            }
            return questions;
        }
       
        public List<Question> GetAllNonOpen()
        {
            List<Question> questions = new List<Question>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT  * FROM [dbo].[Questions] Where [Archived]=0 AND ([QuestionTypesID]=1 OR [QuestionTypesID]=2) Order By [EntryNo] Asc;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            questions.Add(MapQuestionFromReader(reader));
                        }
                    }
                }
            }
            return questions;
        }
        public DataTable Get()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [EntryNo],ROW_NUMBER() OVER (Order by [Questions].[EntryNo]) AS [Index],[Questions].[ID],[QuestionNo],[QuestionLabel],[Question],[QuestionType],[QuestionTypesID],[Weight],[AddedBy],[AddedOn] FROM [dbo].[Questions] LEFT JOIN [QuestionTypes] ON [QuestionTypes].[ID]=[Questions].[QuestionTypesID] WHERE [Questions].[Archived]=0 Order By [EntryNo] Asc"; 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public Question Read(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM Questions WHERE ID = @ID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapQuestionFromReader(reader);
                        }
                        return null;
                    }
                }
            }
        }

        public void Update(Question question)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"UPDATE Questions
                                 SET QuestionnaireID = @QuestionnaireID, QuestionNo = @QuestionNo, QuestionLabel = @QuestionLabel,
                                     Question = @QuestionText, QuestionTypesID = @QuestionTypeID, Weight = @Weight, AddedBy = @AddedBy, AddedOn = @AddedOn
                                 WHERE ID = @ID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, question);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE Questions SET Archived=1,ArchivedOn=@ArchivedOn,ArchivedBy=@ArchivedBy WHERE ID=@ID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("@ArchivedOn", DateTime.Now);
                    command.Parameters.AddWithValue("@ArchivedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddParameters(SqlCommand command, Question question)
        {
            command.Parameters.AddWithValue("@ID", question.ID); 
            command.Parameters.AddWithValue("@QuestionNo", question.QuestionNo);
            command.Parameters.AddWithValue("@QuestionLabel", (object)question.QuestionLabel ?? string.Empty);
            command.Parameters.AddWithValue("@QuestionText", question.QuestionText);
            command.Parameters.AddWithValue("@QuestionTypeID", question.QuestionTypeID);
            command.Parameters.AddWithValue("@Weight", (object)question.Weight ?? DBNull.Value);
            command.Parameters.AddWithValue("@AddedBy", question.AddedBy);
            command.Parameters.AddWithValue("@AddedOn", question.AddedOn);
        }

        private Question MapQuestionFromReader(SqlDataReader reader)
        {
            return new Question
            {
                EntryNo = (int)reader["EntryNo"],
                ID = (Guid)reader["ID"], 
                QuestionNo = (int)reader["QuestionNo"],
                QuestionLabel = reader["QuestionLabel"].ToString(),
                QuestionText = reader["Question"].ToString(),
                QuestionTypeID = (byte)reader["QuestionTypesID"],
                Weight = reader["Weight"] != DBNull.Value ? (decimal)reader["Weight"] : (decimal?)null,
                AddedBy = reader["AddedBy"].ToString(),
                AddedOn = (DateTime)reader["AddedOn"]
            };
        }
    }
}