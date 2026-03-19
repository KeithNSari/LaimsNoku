using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Lifeproducts
{
    public class PolicyTypeDocumentsRepository: IPolicyTypeDocumentsRepository
    { 
            private IConfiguration _configuration;
            private IWebHostEnvironment _environment;
            public PolicyTypeDocumentsRepository(IConfiguration configuration, IWebHostEnvironment environment)
            {
                _configuration = configuration;
                _environment = environment;
            }
        public int CheckExistence(PolicyTypeDocument policyTypeDocument)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM [dbo].[PolicyTypesDocuments] Where [DocumentID]=@DocumentID AND [PolicyTypesID]=@PolicyTypesID AND ARCHIVED=0 AND ValidationGroup=@ValidationGroup; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("DocumentID", policyTypeDocument.DocumentID);
                    command.Parameters.AddWithValue("PolicyTypesID", policyTypeDocument.PolicyTypesID);
                    command.Parameters.AddWithValue("ValidationGroup",(object)policyTypeDocument.ValidationGroup ?? string.Empty);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
            public void InsertDocument(PolicyTypeDocument policyTypeDocument)
            {
                var Database = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(Database))
                {
                    connection.Open();

                    string query = @"INSERT INTO [dbo].[PolicyTypesDocuments](PolicyTypesID,DocumentID,Optional,ValidationGroup,AddedOn,AddedBy)
                             VALUES (@PolicyTypesID, @DocumentID, @Optional,@ValidationGroup,@AddedOn, @AddedBy)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SetParameters(command, policyTypeDocument);
                        command.ExecuteNonQuery();
                    }
                }
            } 
            public DataTable GetDocuments(Guid PolicyDefinitionID)
            {
                var Database = _configuration.GetConnectionString("DefaultConnection");
                DataTable DT = new DataTable();
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = Database;
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT [PolicyTypesDocuments].[ID],[PolicyTypesDocuments].[PolicyTypesID],[DocumentID],[Document],[ValidationGroup],[Optional],CASE [Optional] When 1 Then 'Optional' When 0 Then 'Required' End As [OptionalDesc] FROM .[dbo].[PolicyTypesDocuments] LEFT JOIN [Documents] On [Documents].[ID]=[PolicyTypesDocuments].[DocumentID] Where [PolicyTypesDocuments].[Deleted]=0 And [PolicyTypesDocuments].[Archived]=0 And [PolicyTypesDocuments].[PolicyTypesID]=@ID Order By [ValidationGroup] Asc,[Document] Asc";
                cmd.Parameters.AddWithValue("ID", PolicyDefinitionID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(DT);
                return DT;
            }
        public void ArchiveDocument(int id, string ArchivedBy, DateTime ArchivedOn)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "Update [dbo].[PolicyTypesDocuments] SET [Archived]=1,[ArchivedBy]=@ArchivedBy,[ArchivedOn]=@ArchivedOn WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("ArchivedBy", ArchivedBy);
                    command.Parameters.AddWithValue("ArchivedOn", ArchivedOn);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SetParameters(SqlCommand command, PolicyTypeDocument  policyTypeDocument)
            {
                command.Parameters.AddWithValue("@ID", policyTypeDocument.ID);
                command.Parameters.AddWithValue("@PolicyTypesID", policyTypeDocument.PolicyTypesID);
                command.Parameters.AddWithValue("@DocumentID", policyTypeDocument.DocumentID);
                command.Parameters.AddWithValue("@Optional", policyTypeDocument.Optional);
                command.Parameters.AddWithValue("@ValidationGroup", (object)policyTypeDocument.ValidationGroup ?? string.Empty);
                command.Parameters.AddWithValue("@Current", policyTypeDocument.Current);
                command.Parameters.AddWithValue("@AddedOn", (object)policyTypeDocument.AddedOn ?? DBNull.Value);
                command.Parameters.AddWithValue("@AddedBy", (object)policyTypeDocument.AddedBy ?? DBNull.Value);
                command.Parameters.AddWithValue("@Archived", (object)policyTypeDocument.Archived ?? DBNull.Value);
                command.Parameters.AddWithValue("@ArchivedBy", (object)policyTypeDocument.ArchivedBy ?? DBNull.Value);
                command.Parameters.AddWithValue("@ArchivedComment", (object)policyTypeDocument.ArchivedComment ?? DBNull.Value);
                command.Parameters.AddWithValue("@ArchivedOn", (object)policyTypeDocument.ArchivedOn ?? DBNull.Value);
                command.Parameters.AddWithValue("@Deleted", (object)policyTypeDocument.Deleted ?? DBNull.Value);
                command.Parameters.AddWithValue("@DeletedBy", (object)policyTypeDocument.DeletedBy ?? DBNull.Value);
                command.Parameters.AddWithValue("@DeletedComment", (object)policyTypeDocument.DeletedComment ?? DBNull.Value);
                command.Parameters.AddWithValue("@DeletedOn", (object)policyTypeDocument.DeletedOn ?? DBNull.Value);
            }

            private PolicyTypeDocument MapDocumentFromReader(SqlDataReader reader)
            {
                return new PolicyTypeDocument
                {
                    ID = (int)reader["ID"], 
                    PolicyTypesID = (Guid)reader["PolicyTypesID"],
                    DocumentID = (Guid)reader["DocumentID"],
                    Optional = (byte)reader["Optional"],
                    ValidationGroup = (string)reader["ValidationGroup"],
                    Current = (byte)reader["Current"],
                    AddedOn = reader["AddedOn"] is DBNull ? (DateTime?)null : (DateTime)reader["AddedOn"],
                    AddedBy = reader["AddedBy"] is DBNull ? null : (string)reader["AddedBy"],
                    Archived = reader["Archived"] is DBNull ? (byte?)null : (byte)reader["Archived"],
                    ArchivedBy = reader["ArchivedBy"] is DBNull ? null : (string)reader["ArchivedBy"],
                    ArchivedComment = reader["ArchivedComment"] is DBNull ? null : (string)reader["ArchivedComment"],
                    ArchivedOn = reader["ArchivedOn"] is DBNull ? (DateTime?)null : (DateTime)reader["ArchivedOn"],
                    Deleted = reader["Deleted"] is DBNull ? (byte?)null : (byte)reader["Deleted"],
                    DeletedBy = reader["DeletedBy"] is DBNull ? null : (string)reader["DeletedBy"],
                    DeletedComment = reader["DeletedComment"] is DBNull ? null : (string)reader["DeletedComment"],
                    DeletedOn = reader["DeletedOn"] is DBNull ? (DateTime?)null : (DateTime)reader["DeletedOn"]
                };
            }
        }
}
