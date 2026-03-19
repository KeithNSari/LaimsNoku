using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Membership
{
    public class MemberStatusRepository : IMemberStatusRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public MemberStatusRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public List<MemberStatus> GetAllMemberStatus()
        {
            List<MemberStatus> memberStatusList = new List<MemberStatus>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "SELECT ID, MemberUID, Status, StatusReason, StatusDate, StatusComment, AddedBy, AddedOn FROM MemberStatii";

                SqlCommand command = new SqlCommand(query, connection);

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    MemberStatus memberStatus = new MemberStatus
                    {
                        ID = reader.GetInt32(0),
                        MemberUID = reader.GetGuid(1),
                        Status = reader.GetInt32(2),
                        StatusReason = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                        StatusDate = reader.GetDateTime(4),
                        StatusComment = reader.IsDBNull(5) ? null : reader.GetString(5),
                        AddedBy = reader.IsDBNull(6) ? null : reader.GetString(6),
                        AddedOn = reader.GetDateTime(7)
                    };

                    memberStatusList.Add(memberStatus);
                }

                reader.Close();
            }

            return memberStatusList;
        }
        public DataTable GetMemberStatusHistory(Guid MemberUID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT TOP (50) [Statii].[Status],[StatiiReasons].[Reason],[StatusDate],[StatusComment] FROM [dbo].[MemberStatii] LEFT JOIN [Statii] ON [Statii].[ID]=[MemberStatii].[Status] LEFT JOIN [StatiiReasons] ON [MemberStatii].[StatusReason]=[StatiiReasons].[ReasonID] WHERE [MemberUID]=@MemberUID ORDER BY [MemberStatii].[ID] DESC";
            command.Parameters.AddWithValue("MemberUID", MemberUID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public void AddMemberStatus(MemberStatus memberStatus)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"INSERT INTO MemberStatii (MemberUID, Status, StatusReason, StatusDate, StatusComment, AddedBy, AddedOn)
                                 VALUES (@MemberUID, @Status, @StatusReason, @StatusDate, @StatusComment, @AddedBy, @AddedOn)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MemberUID", memberStatus.MemberUID);
                command.Parameters.AddWithValue("@Status", memberStatus.Status);
                command.Parameters.AddWithValue("@StatusReason", memberStatus.StatusReason ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@StatusDate", memberStatus.StatusDate);
                command.Parameters.AddWithValue("@StatusComment", string.IsNullOrEmpty(memberStatus.StatusComment) ? DBNull.Value : memberStatus.StatusComment);
                command.Parameters.AddWithValue("@AddedBy", string.IsNullOrEmpty(memberStatus.AddedBy) ? DBNull.Value : memberStatus.AddedBy);
                command.Parameters.AddWithValue("@AddedOn", memberStatus.AddedOn);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateMemberStatus(MemberStatus memberStatus)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"UPDATE MemberStatii
                                 SET MemberUID = @MemberUID, 
                                     Status = @Status, 
                                     StatusReason = @StatusReason, 
                                     StatusDate = @StatusDate, 
                                     StatusComment = @StatusComment, 
                                     AddedBy = @AddedBy, 
                                     AddedOn = @AddedOn 
                                 WHERE ID = @ID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MemberUID", memberStatus.MemberUID);
                command.Parameters.AddWithValue("@Status", memberStatus.Status);
                command.Parameters.AddWithValue("@StatusReason", memberStatus.StatusReason ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@StatusDate", memberStatus.StatusDate);
                command.Parameters.AddWithValue("@StatusComment", string.IsNullOrEmpty(memberStatus.StatusComment) ? DBNull.Value : memberStatus.StatusComment);
                command.Parameters.AddWithValue("@AddedBy", string.IsNullOrEmpty(memberStatus.AddedBy) ? DBNull.Value : memberStatus.AddedBy);
                command.Parameters.AddWithValue("@AddedOn", memberStatus.AddedOn);
                command.Parameters.AddWithValue("@ID", memberStatus.ID);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteMemberStatus(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "DELETE FROM MemberStatii WHERE ID = @ID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

    }
}
