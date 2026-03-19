using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Lifeproducts
{
    public class ProductQuestionnaireRepository: IProductQuestionnaireRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public ProductQuestionnaireRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
        public int CheckExistence(ProductQuestionnaire productQuestionnaire)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM [dbo].[ProductQuestionnaires] Where [QuestionnaireID]=@QuestionnaireID AND [ProductID]=@ProductID AND DELETED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("QuestionnaireID", productQuestionnaire.QuestionnaireID);
                    command.Parameters.AddWithValue("ProductID", productQuestionnaire.ProductID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void InsertQuestionnaire(ProductQuestionnaire productQuestionnaire)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"INSERT INTO [dbo].[ProductQuestionnaires](ProductID,QuestionnaireID,Tested,AddedOn,AddedBy)
                             VALUES (@ProductID, @QuestionnaireID, @Tested,@AddedOn, @AddedBy)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, productQuestionnaire);
                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetQuestionnaires(Guid ProductID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [ProductQuestionnaires].[ID],[ProductQuestionnaires].[ProductID],[QuestionnaireID],[Questionnaires].[Title] AS [Questionnaire],[Tested],CASE [Tested] When 1 Then 'Tested' When 0 Then 'Untested' When 2 Then 'All' End As [TestedDesc] FROM [dbo].[ProductQuestionnaires] LEFT JOIN [Questionnaires] On [Questionnaires].[ID]=[ProductQuestionnaires].[QuestionnaireID] Where [ProductQuestionnaires].[Deleted]=0 And [ProductQuestionnaires].[Archived]=0 And [ProductQuestionnaires].[ProductID]=@ID Order By [Questionnaires].[Title] Asc";
            cmd.Parameters.AddWithValue("ID", ProductID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void DeleteQuestionnaire(int id, string DeletedBy, DateTime DeletedOn)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "Update [dbo].[ProductQuestionnaires] SET [Deleted]=1,[DeletedBy]=@DeletedBy,[DeletedOn]=@DeletedOn WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("DeletedBy", DeletedBy);
                    command.Parameters.AddWithValue("DeletedOn", DeletedOn);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SetParameters(SqlCommand command, ProductQuestionnaire productQuestionnaire)
        {
            command.Parameters.AddWithValue("@ID", productQuestionnaire.ID);
            command.Parameters.AddWithValue("@ProductID", productQuestionnaire.ProductID);
            command.Parameters.AddWithValue("@QuestionnaireID", productQuestionnaire.QuestionnaireID);
            command.Parameters.AddWithValue("@Tested", productQuestionnaire.Tested); 
            command.Parameters.AddWithValue("@Current", productQuestionnaire.Current);
            command.Parameters.AddWithValue("@AddedOn", (object)productQuestionnaire.AddedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@AddedBy", (object)productQuestionnaire.AddedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@Archived", (object)productQuestionnaire.Archived ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedBy", (object)productQuestionnaire.ArchivedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedComment", (object)productQuestionnaire.ArchivedComment ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedOn", (object)productQuestionnaire.ArchivedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@Deleted", (object)productQuestionnaire.Deleted ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedBy", (object)productQuestionnaire.DeletedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedComment", (object)productQuestionnaire.DeletedComment ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedOn", (object)productQuestionnaire.DeletedOn ?? DBNull.Value);
        }

        private ProductQuestionnaire MapQuestionnaireFromReader(SqlDataReader reader)
        {
            return new ProductQuestionnaire
            {
                ID = (int)reader["ID"],
                ProductID = (Guid)reader["ProductID"],
                QuestionnaireID = (Guid)reader["QuestionnaireID"],
                Tested = (byte)reader["Tested"], 
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
