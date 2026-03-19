using LAIMS.Models.Questionnaires;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System;
using LAIMS.Interfaces.Questionnaires;
using System.Data;

namespace LAIMS.Repositories.Questionnaires
{
    public class QuestionExpectedResponseRepository: IQuestionExpectedResponseRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public QuestionExpectedResponseRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public int CheckExistence(QuestionExpectedResponse expectedResponse)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM QuestionExpectedResponses Where ([QuestionID]=@QuestionID) AND ([ExpectedResponse]=@ExpectedResponse) And ARCHIVED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, expectedResponse);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void Create(QuestionExpectedResponse expectedResponse)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"INSERT INTO QuestionExpectedResponses (ID, Sequence, Label, QuestionID, ExpectedResponse, Weight, AddedBy, AddedOn)
                                 VALUES (@ID, @Sequence, @Label, @QuestionID, @ExpectedResponse, @Weight, @AddedBy, @AddedOn);";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, expectedResponse);
                    command.ExecuteNonQuery();
                }
            }
        }

        public QuestionExpectedResponse Read(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM QuestionExpectedResponses WHERE ID = @ID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapExpectedResponseFromReader(reader);
                        }
                        return null;
                    }
                }
            }
        }

        public void Update(QuestionExpectedResponse expectedResponse)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"UPDATE QuestionExpectedResponses
                                 SET Sequence = @Sequence, Label = @Label, QuestionID = @QuestionID, ExpectedResponse = @ExpectedResponse,
                                     Weight = @Weight, AddedBy = @AddedBy, AddedOn = @AddedOn
                                 WHERE ID = @ID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, expectedResponse);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Archive(Guid id, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE QuestionExpectedResponses SET Archived=1,ArchivedOn=@ArchivedOn, ArchivedBy=@ArchivedBy WHERE QuestionID=@ID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("ArchivedOn", DateTime.Now);
                    command.Parameters.AddWithValue("ArchivedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<QuestionExpectedResponse> GetAll()
        {
            List<QuestionExpectedResponse> expectedResponses = new List<QuestionExpectedResponse>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM QuestionExpectedResponses;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            expectedResponses.Add(MapExpectedResponseFromReader(reader));
                        }
                    }
                }
            }
            return expectedResponses;
        }
        public DataTable Get()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [QuestionNo],[QuestionID],[Question],STRING_AGG ([ExpectedResponse], ',') as [Responses] FROM [dbo].[QuestionExpectedResponses] LEFT JOIN [Questions] ON [QuestionExpectedResponses].[QuestionID]=[Questions].[ID] WHERE [QuestionExpectedResponses].[Archived]=0 AND [Questions].[Archived]=0 GROUP BY [QuestionID],[Question],[QuestionNo] ORDER BY [Questions].[QuestionNo] Asc";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        private void AddParameters(SqlCommand command, QuestionExpectedResponse expectedResponse)
        {
            command.Parameters.AddWithValue("@ID", expectedResponse.ID);
            command.Parameters.AddWithValue("@Sequence", expectedResponse.Sequence);
            command.Parameters.AddWithValue("@Label", (object)expectedResponse.Label ?? DBNull.Value);
            command.Parameters.AddWithValue("@QuestionID", expectedResponse.QuestionID);
            command.Parameters.AddWithValue("@ExpectedResponse", expectedResponse.ExpectedResponse);
            command.Parameters.AddWithValue("@Weight", (object)expectedResponse.Weight ?? DBNull.Value);
            command.Parameters.AddWithValue("@AddedBy", (object)expectedResponse.AddedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@AddedOn", (object)expectedResponse.AddedOn ?? DBNull.Value);
        }

        private QuestionExpectedResponse MapExpectedResponseFromReader(SqlDataReader reader)
        {
            return new QuestionExpectedResponse
            {
                EntryNo = (int)reader["EntryNo"],
                ID = (Guid)reader["ID"],
                Sequence = (int)reader["Sequence"],
                Label = reader["Label"] != DBNull.Value ? reader["Label"].ToString() : null,
                QuestionID = (Guid)reader["QuestionID"],
                ExpectedResponse = reader["ExpectedResponse"].ToString(),
                Weight = reader["Weight"] != DBNull.Value ? (decimal)reader["Weight"] : (decimal?)null,
                AddedBy = reader["AddedBy"] != DBNull.Value ? reader["AddedBy"].ToString() : null,
                AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime)reader["AddedOn"] : (DateTime?)null
            };
        }
    }
}