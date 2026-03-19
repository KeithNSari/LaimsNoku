using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.Questionnaires;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Questionnaires
{
    public class QuestionnaireRepository: IQuestionnaireRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public QuestionnaireRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public int CheckExistence(Questionnaire questionnaire)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM Questionnaires Where ([Title]=@Title) And ARCHIVED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, questionnaire);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void Create(Questionnaire questionnaire)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"INSERT INTO Questionnaires (ID, Title, Category, Weighted, TotalWeight, AddedBy, AddedOn)
                                 VALUES (@ID, @Title, @Category, @Weighted, @TotalWeight, @AddedBy, @AddedOn);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, questionnaire);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Questionnaire Read(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM Questionnaires WHERE ID = @ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapQuestionnaireFromReader(reader);
                        }
                        return null;
                    }
                }
            }
        }
        public string? GetTitle(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT  [Title] FROM  [dbo].[Questionnaires] WHERE [ID]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id); 
                    return command.ExecuteScalar() as string;
                }
            }
        }
        public List<Questionnaire> GetAll()
        {
            List<Questionnaire> questionnaires = new List<Questionnaire>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM Questionnaires Where [Archived]=0;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            questionnaires.Add(MapQuestionnaireFromReader(reader));
                        }
                    }
                }
            }
            return questionnaires;
        }
        public DataTable Get()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT * FROM  [dbo].[Questionnaires] WHERE [Archived]=0";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void Update(Questionnaire questionnaire)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"UPDATE Questionnaires
                                 SET Title = @Title, Category = @Category, Weighted = @Weighted, TotalWeight = @TotalWeight,
                                     AddedBy = @AddedBy, AddedOn = @AddedOn, Archived = @Archived, ArchivedBy = @ArchivedBy, ArchivedOn = @ArchivedOn
                                 WHERE ID = @ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, questionnaire);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Archive(Guid id, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE Questionnaires SET Archived=1,ArchivedOn=@ArchivedOn,ArchivedBy=@ArchivedBy WHERE ID=@ID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("@ArchivedOn", DateTime.Now);
                    command.Parameters.AddWithValue("@ArchivedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddParameters(SqlCommand command, Questionnaire questionnaire)
        {
            command.Parameters.AddWithValue("@ID", questionnaire.ID);
            command.Parameters.AddWithValue("@Title", questionnaire.Title);
            command.Parameters.AddWithValue("@Category", questionnaire.Category);
            command.Parameters.AddWithValue("@Weighted", questionnaire.Weighted);
            command.Parameters.AddWithValue("@TotalWeight", (object)questionnaire.TotalWeight ?? DBNull.Value);
            command.Parameters.AddWithValue("@AddedBy", (object)questionnaire.AddedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@AddedOn", (object)questionnaire.AddedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@Archived", (object)questionnaire.Archived ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedBy", (object)questionnaire.ArchivedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedOn", (object)questionnaire.ArchivedOn ?? DBNull.Value);
        }

        private Questionnaire MapQuestionnaireFromReader(SqlDataReader reader)
        {
            return new Questionnaire
            {
                EntryNo = (int)reader["EntryNo"],
                ID = (Guid)reader["ID"],
                Title = reader["Title"].ToString(),
                Category = (byte)reader["Category"],
                Weighted = (byte)reader["Weighted"],
                TotalWeight = reader["TotalWeight"] != DBNull.Value ? (decimal)reader["TotalWeight"] : (decimal?)null,
                AddedBy = reader["AddedBy"] != DBNull.Value ? reader["AddedBy"].ToString() : null,
                AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime)reader["AddedOn"] : (DateTime?)null,
                Archived = reader["Archived"] != DBNull.Value ? (byte?)reader["Archived"] : null,
                ArchivedBy = reader["ArchivedBy"] != DBNull.Value ? reader["ArchivedBy"].ToString() : null,
                ArchivedOn = reader["ArchivedOn"] != DBNull.Value ? (DateTime)reader["ArchivedOn"] : (DateTime?)null
            };
        }
    }
}
