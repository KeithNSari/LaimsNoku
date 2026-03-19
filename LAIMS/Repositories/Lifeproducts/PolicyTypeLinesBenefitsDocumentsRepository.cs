using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using Microsoft.Data.SqlClient;
using System.Data;
namespace LAIMS.Repositories.Lifeproducts
{
    public class PolicyTypeLinesBenefitsDocumentsRepository: IPolicyTypeLinesBenefitsDocumentsRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public PolicyTypeLinesBenefitsDocumentsRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
        public int CheckExistence(PolicyTypeLinesBenefitDocument policyTypeLinesBenefitDocument)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM [dbo].[PTLBenefitsDocuments] Where [DocumentID]=@DocumentID AND [PTLID]=@PTLID AND ValidationGroup=@ValidationGroup AND ARCHIVED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("DocumentID", policyTypeLinesBenefitDocument.DocumentID);
                    command.Parameters.AddWithValue("PTLID", policyTypeLinesBenefitDocument.PTLID);
                    command.Parameters.AddWithValue("ValidationGroup", (object)policyTypeLinesBenefitDocument.ValidationGroup ?? string.Empty);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void InsertDocument(PolicyTypeLinesBenefitDocument policyTypeLinesBenefitDocument)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"INSERT INTO [dbo].[PTLBenefitsDocuments](PTLID,TestedBusiness,DocumentID,Optional,ValidationGroup,AddedOn,AddedBy)
                             VALUES (@PTLID,@TestedBusiness, @DocumentID, @Optional,@ValidationGroup,@AddedOn, @AddedBy)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, policyTypeLinesBenefitDocument);
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
            cmd.CommandText = "SELECT [Products].[Product],CASE [TestedBusiness] WHEN 0 Then 'Untested' WHEN 1 THEN 'Tested' WHEN 2 THEN 'All' ELSE 'Undefined' END AS [TestedBusiness],[PTLBenefitsDocuments].[ID],[DocumentID],[Document],[ValidationGroup],[PTLBenefitsDocuments].[Optional],CASE [PTLBenefitsDocuments].[Optional] When 1 Then 'Optional' When 0 Then 'Required' End As [OptionalDesc], [PTLBenefitsDocuments].[AddedOn] FROM .[dbo].[PTLBenefitsDocuments] LEFT JOIN [Documents] On [Documents].[ID]=[PTLBenefitsDocuments].[DocumentID] LEFT JOIN [PolicyTypesLines] On [PolicyTypesLines].[ID]=[PTLBenefitsDocuments].[PTLID] LEFT JOIN [Products] On [PolicyTypesLines].[ProductID]=[Products].[ID] Where [PTLBenefitsDocuments].[Deleted]=0 And [PTLBenefitsDocuments].[Archived]=0 AND [PolicyTypesLines].[HeaderID]=@PolicyDefinitionID  Order By [ValidationGroup] Asc,[Document] Asc";
            cmd.Parameters.AddWithValue("PolicyDefinitionID", PolicyDefinitionID);
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

                string query = "Update [dbo].[PTLBenefitsDocuments] SET [Archived]=1,[ArchivedBy]=@ArchivedBy,[ArchivedOn]=@ArchivedOn WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("ArchivedBy", ArchivedBy);
                    command.Parameters.AddWithValue("ArchivedOn", ArchivedOn);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SetParameters(SqlCommand command, PolicyTypeLinesBenefitDocument policyTypeLinesBenefitDocument)
        {
            command.Parameters.AddWithValue("@ID", policyTypeLinesBenefitDocument.ID);
            command.Parameters.AddWithValue("@PTLID", policyTypeLinesBenefitDocument.PTLID);
            command.Parameters.AddWithValue("@TestedBusiness", policyTypeLinesBenefitDocument.TestedBusiness);
            command.Parameters.AddWithValue("@DocumentID", policyTypeLinesBenefitDocument.DocumentID);
            command.Parameters.AddWithValue("@Optional", policyTypeLinesBenefitDocument.Optional);
            command.Parameters.AddWithValue("@ValidationGroup", (object)policyTypeLinesBenefitDocument.ValidationGroup ?? string.Empty);
            command.Parameters.AddWithValue("@Current", policyTypeLinesBenefitDocument.Current);
            command.Parameters.AddWithValue("@AddedOn", (object)policyTypeLinesBenefitDocument.AddedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@AddedBy", (object)policyTypeLinesBenefitDocument.AddedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@Archived", (object)policyTypeLinesBenefitDocument.Archived ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedBy", (object)policyTypeLinesBenefitDocument.ArchivedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedComment", (object)policyTypeLinesBenefitDocument.ArchivedComment ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedOn", (object)policyTypeLinesBenefitDocument.ArchivedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@Deleted", (object)policyTypeLinesBenefitDocument.Deleted ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedBy", (object)policyTypeLinesBenefitDocument.DeletedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedComment", (object)policyTypeLinesBenefitDocument.DeletedComment ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedOn", (object)policyTypeLinesBenefitDocument.DeletedOn ?? DBNull.Value);
        }

        private PolicyTypeLinesBenefitDocument MapDocumentFromReader(SqlDataReader reader)
        {
            return new PolicyTypeLinesBenefitDocument
            {
                ID = (int)reader["ID"],
                PTLID = (Guid)reader["PTLBenefitsID"],
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
