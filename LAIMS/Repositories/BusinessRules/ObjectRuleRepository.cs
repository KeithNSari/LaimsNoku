using LAIMS.Interfaces.BusinessRules;
using LAIMS.Models.BusinessRules;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.BusinessRules
{
    public class ObjectRuleRepository: IObjectRuleRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public ObjectRuleRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public void CreateObjectRule(ObjectRule objectRule)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "INSERT INTO ObjectRules (ObjectRuleID, ObjectID, RuleID, Filter, AddedOn, AddedBy) " +
                               "VALUES (@ObjectRuleID, @ObjectID, @RuleID, @Filter, @AddedOn, @AddedBy)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ObjectRuleID", objectRule.ObjectRuleID);
                    command.Parameters.AddWithValue("@ObjectID", objectRule.ObjectID);
                    command.Parameters.AddWithValue("@RuleID", objectRule.RuleID);
                    command.Parameters.AddWithValue("@Filter", objectRule.Filter);
                    command.Parameters.AddWithValue("@AddedOn", objectRule.AddedOn);
                    command.Parameters.AddWithValue("@AddedBy", objectRule.AddedBy);

                    command.ExecuteNonQuery();
                }
            }
        }

        public ObjectRule GetObjectRuleById(int entryNo)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM ObjectRules WHERE EntryNo = @EntryNo";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EntryNo", entryNo);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToObjectRule(reader);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public List<ObjectRule> GetAllObjectRules()
        {
            List<ObjectRule> objectRules = new List<ObjectRule>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM ObjectRules";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ObjectRule objectRule = MapToObjectRule(reader);
                            objectRules.Add(objectRule);
                        }
                    }
                }
            }

            return objectRules;
        }
        public DataTable Get(Guid ObjectID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [ObjectRules].[EntryNo],[ObjectRuleID],[ObjectID],[Rules].[RuleName],[RuleID],[Filter] FROM [dbo].[ObjectRules] LEFT JOIN [Rules] ON [Rules].[ID]=[ObjectRules].[RuleID] WHERE ([ObjectRules].[Archived]=0) AND ([ObjectID]=@ObjectID) ORDER BY [Filter] ASC,[Rules].[RuleName] ASC";
            cmd.Parameters.AddWithValue("@ObjectID",ObjectID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void UpdateObjectRule(ObjectRule objectRule)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "UPDATE ObjectRules SET ObjectRuleID = @ObjectRuleID, ObjectID = @ObjectID, " +
                               "RuleID = @RuleID, Filter = @Filter, AddedOn = @AddedOn, AddedBy = @AddedBy " +
                               "WHERE EntryNo = @EntryNo";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EntryNo", objectRule.EntryNo);
                    command.Parameters.AddWithValue("@ObjectRuleID", objectRule.ObjectRuleID);
                    command.Parameters.AddWithValue("@ObjectID", objectRule.ObjectID);
                    command.Parameters.AddWithValue("@RuleID", objectRule.RuleID);
                    command.Parameters.AddWithValue("@Filter", objectRule.Filter);
                    command.Parameters.AddWithValue("@AddedOn", objectRule.AddedOn);
                    command.Parameters.AddWithValue("@AddedBy", objectRule.AddedBy);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteObjectRule(int entryNo)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM ObjectRules WHERE EntryNo = @EntryNo";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EntryNo", entryNo);

                    command.ExecuteNonQuery();
                }
            }
        }

        private ObjectRule MapToObjectRule(SqlDataReader reader)
        {
            return new ObjectRule
            {
                EntryNo = Convert.ToInt32(reader["EntryNo"]),
                ObjectRuleID = Guid.Parse(reader["ObjectRuleID"].ToString()),
                ObjectID = Guid.Parse(reader["ObjectID"].ToString()),
                RuleID = Guid.Parse(reader["RuleID"].ToString()),
                Filter = reader["Filter"].ToString(),
                AddedOn = Convert.ToDateTime(reader["AddedOn"]),
                AddedBy = reader["AddedBy"].ToString()
            };
        }
    }
}
