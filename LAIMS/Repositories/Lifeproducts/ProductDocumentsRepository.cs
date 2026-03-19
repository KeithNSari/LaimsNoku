using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Membership;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Lifeproducts
{
    public class ProductDocumentsRepository: IProductDocumentRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public ProductDocumentsRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
        public int CheckExistence(ProductDocument productDocument)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM [dbo].[ProductDocuments] Where ([DocumentID]=@DocumentID) AND ([ProductID]=@ProductID) AND ([LIRoleID]=@LIRoleID) AND ([Tested]=@Tested) AND ([DELETED]=0); SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("DocumentID", productDocument.DocumentID);
                    command.Parameters.AddWithValue("ProductID", productDocument.ProductID);
                    command.Parameters.AddWithValue("LIRoleID", productDocument.LIRoleID);
                    command.Parameters.AddWithValue("Tested", productDocument.Tested); 
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void InsertDocument(ProductDocument productDocument)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"INSERT INTO [dbo].[ProductDocuments](ProductID,DocumentID,Optional,ValidationGroup,LIRoleID,Tested,AddedOn,AddedBy)
                             VALUES (@ProductID, @DocumentID, @Optional,@ValidationGroup,@LIRoleID,@Tested,@AddedOn, @AddedBy)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, productDocument);
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
            cmd.CommandText = "SELECT [ProductDocuments].[ID],CASE [Tested] WHEN 0 THEN 'Untested' WHEN 1 THEN 'Tested' WHEN 2 THEN 'All' END As [Tested],[LIRoles].[Role],[ProductDocuments].[ProductID],[DocumentID],[Document],[ValidationGroup],[Optional],CASE [Optional] When 1 Then 'Optional' When 0 Then 'Required' End As [OptionalDesc] FROM .[dbo].[ProductDocuments] LEFT JOIN [Documents] On [Documents].[ID]=[ProductDocuments].[DocumentID] LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[ProductDocuments].[LIRoleID] Where [ProductDocuments].[Deleted]=0 And [ProductDocuments].[Archived]=0 And [ProductDocuments].[ProductID]=@ID Order By [ValidationGroup] Asc,[Document] Asc";
            cmd.Parameters.AddWithValue("ID", PolicyDefinitionID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void DeleteDocument(int id, string DeletedBy, DateTime DeletedOn)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "Update [dbo].[ProductDocuments] SET [Deleted]=1,[DeletedBy]=@DeletedBy,[DeletedOn]=@DeletedOn WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("DeletedBy", DeletedBy);
                    command.Parameters.AddWithValue("DeletedOn", DeletedOn);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SetParameters(SqlCommand command, ProductDocument productDocument)
        {
            command.Parameters.AddWithValue("@ID", productDocument.ID);
            command.Parameters.AddWithValue("@ProductID", productDocument.ProductID);
            command.Parameters.AddWithValue("@DocumentID", productDocument.DocumentID);
            command.Parameters.AddWithValue("@Optional", productDocument.Optional);
            command.Parameters.AddWithValue("@ValidationGroup", (object)productDocument.ValidationGroup ?? DBNull.Value);
            command.Parameters.AddWithValue("@LIRoleID",productDocument.LIRoleID);
            command.Parameters.AddWithValue("@Tested",productDocument.Tested);
            command.Parameters.AddWithValue("@Current", productDocument.Current);
            command.Parameters.AddWithValue("@AddedOn", (object)productDocument.AddedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@AddedBy", (object)productDocument.AddedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@Archived", (object)productDocument.Archived ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedBy", (object)productDocument.ArchivedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedComment", (object)productDocument.ArchivedComment ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedOn", (object)productDocument.ArchivedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@Deleted", (object)productDocument.Deleted ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedBy", (object)productDocument.DeletedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedComment", (object)productDocument.DeletedComment ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedOn", (object)productDocument.DeletedOn ?? DBNull.Value);
        }

        private ProductDocument MapDocumentFromReader(SqlDataReader reader)
        {
            return new ProductDocument
            {
                ID = (int)reader["ID"],
                ProductID = (Guid)reader["ProductID"],
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
