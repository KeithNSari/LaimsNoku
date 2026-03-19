using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.Data.SqlClient;
namespace LAIMS.Repositories.Membership
{ 
    public class StatiiRepository: IStatiiRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public StatiiRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public List<Statii> GetAllMemberStatii()
        {
            List<Statii> statiiList = new List<Statii>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "SELECT ID, Status, Sequence, StatusGroupID, Active FROM Statii WHERE [Members]=1 AND [Selectable]=1 ORDER BY Status ASC";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Statii statii = new Statii();
                    statii.ID = Convert.ToInt32(reader["ID"]);
                    statii.Status = reader["Status"].ToString();
                    statii.Sequence = Convert.ToInt32(reader["Sequence"]);
                    statii.StatusGroupID = Convert.ToInt32(reader["StatusGroupID"]);
                    statii.Active = Convert.ToBoolean(reader["Active"]);

                    statiiList.Add(statii);
                }

                reader.Close();
            }

            return statiiList;
        }
        public List<Statii> GetAllSelectablePolicyStatii()
        {
            List<Statii> statiiList = new List<Statii>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "Statii_PoliciesSelectable";
                SqlCommand command = new SqlCommand(query, connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Statii statii = new Statii();
                    statii.ID = Convert.ToInt32(reader["ID"]);
                    statii.Status = reader["Status"].ToString();
                    statii.Sequence = Convert.ToInt32(reader["Sequence"]);
                    statii.StatusGroupID = Convert.ToInt32(reader["StatusGroupID"]);
                    statii.Active = Convert.ToBoolean(reader["Active"]);

                    statiiList.Add(statii);
                }
                reader.Close();
            }

            return statiiList;
        }
        public Statii GetStatiiByID(int statiiID)
        {
            Statii statii = null;

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "SELECT ID, Status, Sequence, StatusGroupID, Active FROM Statii WHERE ID = @ID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", statiiID);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    statii = new Statii();
                    statii.ID = Convert.ToInt32(reader["ID"]);
                    statii.Status = reader["Status"].ToString();
                    statii.Sequence = Convert.ToInt32(reader["Sequence"]);
                    statii.StatusGroupID = Convert.ToInt32(reader["StatusGroupID"]);
                    statii.Active = Convert.ToBoolean(reader["Active"]);
                }

                reader.Close();
            }

            return statii;
        }

        public void AddStatii(Statii statii)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "INSERT INTO Statii (ID, Status, Sequence, StatusGroupID, Active) " +
                               "VALUES (@ID, @Status, @Sequence, @StatusGroupID, @Active)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", statii.ID);
                command.Parameters.AddWithValue("@Status", statii.Status);
                command.Parameters.AddWithValue("@Sequence", statii.Sequence);
                command.Parameters.AddWithValue("@StatusGroupID", statii.StatusGroupID);
                command.Parameters.AddWithValue("@Active", statii.Active);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateStatii(Statii statii)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "UPDATE Statii SET Status = @Status, Sequence = @Sequence, " +
                               "StatusGroupID = @StatusGroupID, Active = @Active WHERE ID = @ID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", statii.ID);
                command.Parameters.AddWithValue("@Status", statii.Status);
                command.Parameters.AddWithValue("@Sequence", statii.Sequence);
                command.Parameters.AddWithValue("@StatusGroupID", statii.StatusGroupID);
                command.Parameters.AddWithValue("@Active", statii.Active);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteStatii(int statiiID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "DELETE FROM Statii WHERE ID = @ID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", statiiID);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
