using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using Microsoft.Data.SqlClient;
using System.Data;
namespace LAIMS.Repositories.Lifeproducts
{
    public class DocumentsRepository: IDocumentsRepository 
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public DocumentsRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
        public int CheckExistence(string DocumentName)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM Documents Where ([Document]=@DocumentName) And ARCHIVED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DocumentName", DocumentName);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int CheckExistenceOther(string DocumentName, Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM Documents WHERE ([Document]=@DocumentName) AND ([ID]!=@ID) AND ARCHIVED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DocumentName", DocumentName);
                    command.Parameters.AddWithValue("@ID", ID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void InsertDocument(Document document)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "INSERT INTO [dbo].[Documents]([Document],[AddedBy],[AddedOn]) VALUES(@DocumentName,@AddedBy,@AddedOn)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, document);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void ArchiveDocument(Guid ID, string AddedBy, DateTime AddedOn)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Documents] Set [Archived]=1,[ArchivedBy]=@AddedBy,[ArchivedOn]=@AddedOn WHERE [ID]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", AddedOn);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateDocument(Document document)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Documents] Set [Document]=@DocumentName,[AddedBy]=@AddedBy,[AddedOn]=@AddedOn WHERE [Id]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, document);
                    command.ExecuteNonQuery();
                }
            }
        } 
        public Document GetDocument(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            Document document = new Document();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM Documents WHERE [ID]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            document = MapDataReaderToDocuments(reader);
                        }
                    }
                }
            }
            return document;
        }
        public DataTable Get()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT * FROM Documents WHERE [Archived]=0 Order By [Document] ASC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public List<Document> GetAllDocuments()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<Document> documents = new List<Document>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM Documents WHERE [Archived]=0 Order By [Document] ASC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Document document = MapDataReaderToDocuments(reader);
                            documents.Add(document); 
                        }
                    }
                }
            }
            return documents;
        }
        public List<Document> GetClaimRequiredDocuments(Guid RequestID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<Document> documents = new List<Document>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "ClaimRequiredDocuments_List";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClaimRequestID", RequestID);
                    command.CommandType = CommandType.StoredProcedure; 
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Document document = new()
                            {
                                ID = Guid.Parse(reader["ID"].ToString()),
                                DocumentName = reader["Document"].ToString(),
                                AddedOn = reader["AddedOn"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["AddedOn"]),
                                AddedBy = reader["AddedBy"].ToString(),
                                FilingNo = reader["FilingNo"].ToString(),
                                UploadID = reader["MediaUploadID"] is DBNull ? Guid.Empty : Guid.Parse(reader["MediaUploadID"].ToString()),
                                Uploaded = Convert.ToBoolean(reader["Uploaded"])                                
                            }; 
                            documents.Add(document);
                        }
                    }
                }
            }
            return documents;
        }
        private Document MapDataReaderToDocuments(SqlDataReader reader)
        {
            return new Document
            {
                ID = Guid.Parse(reader["ID"].ToString()),
                DocumentName = reader["Document"].ToString(), 
                AddedOn = reader["AddedOn"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["AddedOn"]),
                AddedBy = reader["AddedBy"].ToString(),
            };
        }
        private void AddParameters(SqlCommand command, Document document)
        {
            command.Parameters.AddWithValue("@ID", document.ID);
            command.Parameters.AddWithValue("@DocumentName", document.DocumentName); 
            command.Parameters.AddWithValue("@AddedBy", document.AddedBy);
            command.Parameters.AddWithValue("@AddedOn", document.AddedOn);
        }

    }
}
