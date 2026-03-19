using LAIMS.Interfaces.Membership;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Membership;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Membership
{
    public class RelationshipRepository: IRelationshipRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public RelationshipRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
		public List<Relationship> GetRelationshipsInclusive()
		{
			var Database = _configuration.GetConnectionString("DefaultConnection");
			List<Relationship> relationships = new List<Relationship>();

			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();

				string query = "SELECT * FROM  [dbo].[Relationships] ORDER BY [Relationship] ASC";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							Relationship relationship = MapDataReaderToRelationship(reader);
							relationships.Add(relationship);
						}
					}
				}
			}

			return relationships;
		}
		public List<Relationship> GetRelationships()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<Relationship> relationships = new List<Relationship>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM  [dbo].[Relationships] WHERE [ID]>0 ORDER BY [Relationship] ASC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Relationship relationship = MapDataReaderToRelationship(reader);
                            relationships.Add(relationship);
                        }
                    }
                }
            }

            return relationships;
        }
        private Relationship MapDataReaderToRelationship(SqlDataReader reader)
        {
            return new Relationship
            {
                ID = Convert.ToInt32(reader["ID"]),
                RelationshipName = reader["Relationship"].ToString(),
                //AddedOn = reader["AddedOn"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["AddedOn"]),
                //AddedBy = reader["AddedBy"].ToString(),
                //Archived = reader["Archived"] is DBNull ? (bool?)null : Convert.ToBoolean(reader["Archived"]),
                //ArchivedBy = reader["ArchivedBy"].ToString(),
                //ArchivedComment = reader["ArchivedComment"].ToString(),
                //ArchivedOn = reader["ArchivedOn"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["ArchivedOn"]),
                //Deleted = reader["Deleted"] is DBNull ? (bool?)null : Convert.ToBoolean(reader["Deleted"]),
                //DeletedBy = reader["DeletedBy"].ToString(),
                //DeletedComment = reader["DeletedComment"].ToString(),
                //DeletedOn = reader["DeletedOn"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["DeletedOn"])
            };
        }
    }
}
