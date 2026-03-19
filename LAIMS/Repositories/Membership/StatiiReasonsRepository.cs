using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Membership
{
    public class StatiiReasonsRepository: IStatiiReasonsRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public StatiiReasonsRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public List<StatiiReason> GetAllStatiiReasons(int statusID)
        {
            List<StatiiReason> statiiReasonsList = new List<StatiiReason>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "SELECT StatusID, ReasonID, Reason, AddedOn, AddedBy FROM StatiiReasons WHERE StatusID = @StatusID AND Selectable=1";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@StatusID", statusID);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    StatiiReason statiiReason = new StatiiReason();
                    statiiReason.StatusID = Convert.ToInt32(reader["StatusID"]);
                    statiiReason.ReasonID = Convert.ToInt32(reader["ReasonID"]);
                    statiiReason.Reason = reader["Reason"].ToString();
                    statiiReason.AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime?)reader["AddedOn"] : null;
                    statiiReason.AddedBy = reader["AddedBy"].ToString();

                    statiiReasonsList.Add(statiiReason);
                }

                reader.Close();
            }

            return statiiReasonsList;
        }

        public StatiiReason GetStatiiReasonByID(int reasonID)
        {
            StatiiReason statiiReason = null;

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "SELECT StatusID, ReasonID, Reason, AddedOn, AddedBy FROM StatiiReasons WHERE ReasonID = @ReasonID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ReasonID", reasonID);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    statiiReason = new StatiiReason();
                    statiiReason.StatusID = Convert.ToInt32(reader["StatusID"]);
                    statiiReason.ReasonID = Convert.ToInt32(reader["ReasonID"]);
                    statiiReason.Reason = reader["Reason"].ToString();
                    statiiReason.AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime?)reader["AddedOn"] : null;
                    statiiReason.AddedBy = reader["AddedBy"].ToString();
                }

                reader.Close();
            }

            return statiiReason;
        }

        public void AddStatiiReason(StatiiReason statiiReason)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "INSERT INTO StatiiReasons (StatusID, ReasonID, Reason, AddedOn, AddedBy) " +
                               "VALUES (@StatusID, @ReasonID, @Reason, @AddedOn, @AddedBy)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@StatusID", statiiReason.StatusID);
                command.Parameters.AddWithValue("@ReasonID", statiiReason.ReasonID);
                command.Parameters.AddWithValue("@Reason", statiiReason.Reason);
                command.Parameters.AddWithValue("@AddedOn", statiiReason.AddedOn ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AddedBy", statiiReason.AddedBy);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateStatiiReason(StatiiReason statiiReason)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "UPDATE StatiiReasons SET Reason = @Reason, AddedOn = @AddedOn, AddedBy = @AddedBy " +
                               "WHERE StatusID = @StatusID AND ReasonID = @ReasonID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@StatusID", statiiReason.StatusID);
                command.Parameters.AddWithValue("@ReasonID", statiiReason.ReasonID);
                command.Parameters.AddWithValue("@Reason", statiiReason.Reason);
                command.Parameters.AddWithValue("@AddedOn", statiiReason.AddedOn ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AddedBy", statiiReason.AddedBy);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteStatiiReason(int statusID, int reasonID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "DELETE FROM StatiiReasons WHERE StatusID = @StatusID AND ReasonID = @ReasonID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@StatusID", statusID);
                command.Parameters.AddWithValue("@ReasonID", reasonID);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
