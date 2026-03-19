using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Premiums
{
    public class BillingMessageRepository: IBillingMessageRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private readonly string Database;
        public BillingMessageRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public List<BillingMessage> GetAllBillingMessages()
        {
            List<BillingMessage> billingMessages = new List<BillingMessage>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "SELECT ID, BillID, Status, StatusReason, Message, AddedBy, AddedOn FROM BillingMessages";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        BillingMessage billingMessage = MapReaderToBillingMessage(reader);
                        billingMessages.Add(billingMessage);
                    }

                    reader.Close();
                }
            }

            return billingMessages;
        }

        public BillingMessage GetBillingMessageById(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "SELECT ID, BillID, Status, StatusReason, Message, AddedBy, AddedOn FROM BillingMessages WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        return MapReaderToBillingMessage(reader);
                    }
                }
            }

            return null;
        }
        public DataTable GetLatestBillingMessages()
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            string query = "SELECT * FROM (SELECT Top (100) BillingMessages.ID, BillingMessages.BillID,BillingHeader.InvoiceNo,Statii.Status, StatiiReasons.Reason AS StatusReason, BillingMessages.Message, BillingMessages.AddedBy, Convert(varchar,BillingMessages.AddedOn,103) AS [AddedOn] FROM BillingMessages  LEFT JOIN [Statii] ON [Statii].[ID]=[BillingMessages].[Status] LEFT JOIN StatiiReasons ON BillingMessages.StatusReason=[StatiiReasons].[ReasonID] LEFT JOIN [BillingHeader] ON [BillingHeader].[BillID]=BillingMessages.BillID Order BY BillingMessages.ID DESC) A LEFT JOIN (SELECT STRING_AGG([Policy].[PolicyNo],',') AS [Policies],[BillID] FROM [dbo].[BilledPremiums] LEFT JOIN [Policy] ON [BilledPremiums].[PolicyID]=[Policy].[ID] Group By [BillID]) B ON A.[BillID]=B.[BillID]";
            cmd.CommandText = query; 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetBillingMessages(long BatchID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            string query = "SELECT * FROM (SELECT BillingMessages.ID, BillingMessages.BillID,BillingHeader.InvoiceNo,Statii.Status, StatiiReasons.Reason AS StatusReason, BillingMessages.Message, BillingMessages.AddedBy, Convert(varchar,BillingMessages.AddedOn,103) AS [AddedOn] FROM [BillingHeader] LEFT JOIN [BillingMessages] ON [BillingHeader].[BillID]=BillingMessages.BillID LEFT JOIN [Statii] ON [Statii].[ID]=[BillingMessages].[Status] LEFT JOIN StatiiReasons ON BillingMessages.StatusReason=[StatiiReasons].[ReasonID] WHERE [BatchID]=@BatchID) A LEFT JOIN (SELECT STRING_AGG([Policy].[PolicyNo],',') AS [Policies],[BillID] FROM [dbo].[BilledPremiums] LEFT JOIN [Policy] ON [BilledPremiums].[PolicyID]=[Policy].[ID] WHERE [BatchID]=@BatchID Group By [BillID]) B ON A.[BillID]=B.[BillID]  Order BY A.ID DESC";
            cmd.Parameters.AddWithValue("@BatchID", BatchID);
            cmd.CommandText = query;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetBillingMessages(Guid PolicyID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            string query = "BillingMessages_GetByPolicyID";
            cmd.Parameters.AddWithValue("@PolicyID", PolicyID);
            cmd.CommandText = query;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void AddBillingMessage(BillingMessage billingMessage)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "INSERT INTO BillingMessages (BillID, Status, StatusReason, Message, AddedBy, AddedOn) " +
                               "VALUES (@BillID, @Status, @StatusReason, @Message, @AddedBy, @AddedOn)";

                using (SqlCommand command = new SqlCommand(query, connection))
                { 
                    command.Parameters.AddWithValue("@BillID", billingMessage.BillID);
                    command.Parameters.AddWithValue("@Status", billingMessage.Status);
                    command.Parameters.AddWithValue("@StatusReason", billingMessage.StatusReason);
                    command.Parameters.AddWithValue("@Message", (object)billingMessage.Message ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", (object)billingMessage.AddedBy ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", (object)billingMessage.AddedOn ?? DBNull.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateBillingMessage(BillingMessage billingMessage)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "UPDATE BillingMessages SET BillID = @BillID, Status = @Status, StatusReason = @StatusReason, " +
                               "Message = @Message, AddedBy = @AddedBy, AddedOn = @AddedOn WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", billingMessage.ID);
                    command.Parameters.AddWithValue("@BillID", billingMessage.BillID);
                    command.Parameters.AddWithValue("@Status", billingMessage.Status);
                    command.Parameters.AddWithValue("@StatusReason", billingMessage.StatusReason);
                    command.Parameters.AddWithValue("@Message", (object)billingMessage.Message ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", (object)billingMessage.AddedBy ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", (object)billingMessage.AddedOn ?? DBNull.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteBillingMessage(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "DELETE FROM BillingMessages WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private BillingMessage MapReaderToBillingMessage(SqlDataReader reader)
        {
            return new BillingMessage
            {
                ID = Convert.ToInt32(reader["ID"]),
                BillID = Convert.ToInt32(reader["BillID"]),
                Status = Convert.ToInt32(reader["Status"]),
                StatusReason = Convert.ToInt32(reader["StatusReason"]),
                Message = reader["Message"] is DBNull ? null : Convert.ToString(reader["Message"]),
                AddedBy = reader["AddedBy"] is DBNull ? null : Convert.ToString(reader["AddedBy"]),
                AddedOn = reader["AddedOn"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["AddedOn"])
            };
        }
    }
} 
