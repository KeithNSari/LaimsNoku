using LAIMS.Interfaces.BusinessRules;
using LAIMS.Models.BusinessRules;
using Microsoft.Data.SqlClient;
using System.Data;
namespace LAIMS.Repositories.BusinessRules
{
    public class ObjectRulesStatiiHistoryRepository: IObjectRulesStatiiHistoryRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public ObjectRulesStatiiHistoryRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public List<ObjectRulesStatiiHistory> GetAll()
        {
            var result = new List<ObjectRulesStatiiHistory>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "SELECT * FROM [dbo].[ObjectRulesStatiiHistory]";
                SqlCommand command = new SqlCommand(query, connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    result.Add(MapReaderToObjectRulesStatiiHistory(reader));
                }
            }

            return result;
        }

        public ObjectRulesStatiiHistory GetById(long id)
        {
            ObjectRulesStatiiHistory result = null;

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "SELECT * FROM [dbo].[ObjectRulesStatiiHistory] WHERE ID = @ID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", id);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    result = MapReaderToObjectRulesStatiiHistory(reader);
                }
            }

            return result;
        }

        public void Add(ObjectRulesStatiiHistory entity)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"INSERT INTO [dbo].[ObjectRulesStatiiHistory] 
                                 (RequestID, SourceID, MemberID,SuccessStatus, StatusRuleID, Status, StatusReason, StatusDate, StatusComment, StatusAddedBy)
                                 VALUES (@RequestID, @SourceID, @MemberID,@SuccessStatus, @StatusRuleID, @Status, @StatusReason, @StatusDate, @StatusComment, @StatusAddedBy)";

                SqlCommand command = new SqlCommand(query, connection);
                SetCommandParameters(command, entity);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Update(ObjectRulesStatiiHistory entity)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"UPDATE [dbo].[ObjectRulesStatiiHistory]
                                 SET UID = @UID,
                                     RequestID = @RequestID,
                                     SourceID = @SourceID,
                                     MemberID = @MemberID,
                                     StatusRuleID = @StatusRuleID,
                                     Status = @Status,
                                     StatusReason = @StatusReason,
                                     StatusDate = @StatusDate,
                                     StatusComment = @StatusComment,
                                     StatusAddedBy = @StatusAddedBy
                                 WHERE ID = @ID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", entity.ID);
                SetCommandParameters(command, entity);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Delete(long id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "DELETE FROM [dbo].[ObjectRulesStatiiHistory] WHERE ID = @ID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private void SetCommandParameters(SqlCommand command, ObjectRulesStatiiHistory entity)
        {
            command.Parameters.AddWithValue("@UID", entity.UID);
            command.Parameters.AddWithValue("@RequestID", (object)entity.RequestID ?? DBNull.Value);
            command.Parameters.AddWithValue("@SourceID", (object)entity.SourceID ?? DBNull.Value);
            command.Parameters.AddWithValue("@MemberID", entity.MemberID);
            command.Parameters.AddWithValue("@SuccessStatus", (object)entity.SuccessStatus ?? DBNull.Value);
            command.Parameters.AddWithValue("@StatusRuleID", (object)entity.StatusRuleID ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", (object)entity.Status ?? DBNull.Value);
            command.Parameters.AddWithValue("@StatusReason", (object)entity.StatusReason ?? DBNull.Value);
            command.Parameters.AddWithValue("@StatusDate", (object)entity.StatusDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@StatusComment", (object)entity.StatusComment ?? DBNull.Value);
            command.Parameters.AddWithValue("@StatusAddedBy", (object)entity.StatusAddedBy ?? DBNull.Value);
        }
        private ObjectRulesStatiiHistory MapReaderToObjectRulesStatiiHistory(SqlDataReader reader)
        {
            return new ObjectRulesStatiiHistory
            {
                ID = reader.GetInt64(reader.GetOrdinal("ID")),
                UID = reader.GetGuid(reader.GetOrdinal("UID")),
                RequestID = reader.IsDBNull(reader.GetOrdinal("RequestID")) ? (Guid?)null : reader.GetGuid(reader.GetOrdinal("RequestID")),
                SourceID = reader.IsDBNull(reader.GetOrdinal("SourceID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("SourceID")),
                MemberID = reader.GetInt32(reader.GetOrdinal("MemberID")),
                StatusRuleID = reader.IsDBNull(reader.GetOrdinal("StatusRuleID")) ? (Guid?)null : reader.GetGuid(reader.GetOrdinal("StatusRuleID")),
                Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Status")),
                StatusReason = reader.IsDBNull(reader.GetOrdinal("StatusReason")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("StatusReason")),
                StatusDate = reader.IsDBNull(reader.GetOrdinal("StatusDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("StatusDate")),
                StatusComment = reader.IsDBNull(reader.GetOrdinal("StatusComment")) ? null : reader.GetString(reader.GetOrdinal("StatusComment")),
                StatusAddedBy = reader.IsDBNull(reader.GetOrdinal("StatusAddedBy")) ? null : reader.GetString(reader.GetOrdinal("StatusAddedBy")),
            };
        }
        public DataTable GetHistory(Guid RequestID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("RequestID", RequestID);
            cmd.CommandText = "ObjectRulesStatiiHistory_Get";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
    }
}
