using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Lifeproducts
{ 
    public class PolicyTypesLinesRepository:IPolicyTypesLinesRepository 
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public PolicyTypesLinesRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
        public int CheckExistenceOfMain(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM PolicyTypesLines Where Main=1 AND HeaderID=@ID And ARCHIVED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int CheckExistence(Guid PolicyTypeID, Guid ProductID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM PolicyTypesLines Where (ProductID=@ProductID) AND (HeaderID=@PolicyTypeID) And ARCHIVED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PolicyTypeID", PolicyTypeID);
                    command.Parameters.AddWithValue("@ProductID", ProductID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void InsertPolicyTypeLine(PolicyTypesLines policyTypeLine)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"INSERT INTO [dbo].[PolicyTypesLines](ID,HeaderID,ProductID,Optional,Main,AddedOn,AddedBy)
                             VALUES (@ID, @HeaderID, @ProductID, @Optional,@Main,@AddedOn, @AddedBy)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, policyTypeLine);
                    command.ExecuteNonQuery();
                }
            }
        }
        public PolicyTypesLines GetPolicyTypeLineById(Guid id)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM [dbo].[PolicyTypesLines] WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapPolicyTypeLineFromReader(reader);
                        }

                        return null;
                    }
                }
            }
        }
        public DataTable GetProducts(Guid PolicyDefinitionID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [PolicyTypesLines].[HeaderID] AS [PTID],[PolicyTypesLines].[ID] As [PTLID],[ProductID],[Products].[Product],[Optional],CASE [Optional] When 1 Then 'Optional' When 0 Then 'Required' End As [OptionalDesc],[Main],CASE [Main] When 0 Then 'Rider' When 1 Then 'Main' End As [MainDesc] FROM [dbo].[PolicyTypesLines] LEFT JOIN [Products] On [Products].[ID]=[PolicyTypesLines].[ProductID] WHERE [PolicyTypesLines].[Archived]=0 AND [PolicyTypesLines].[Deleted]=0 AND [PolicyTypesLines].[Current]=1 AND [PolicyTypesLines].[HeaderID]=@ID";
            cmd.Parameters.AddWithValue("ID", PolicyDefinitionID);  
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public List<Product> GetAllProducts(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<Product> products = new List<Product>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT [PolicyTypesLines].[EntryNo], [PolicyTypesLines].[ProductID],[Products].[Product],[ProductCategories].[Category] FROM [dbo].[PolicyTypesLines] LEFT JOIN [Products] ON [Products].[ID]=[PolicyTypesLines].[ProductID] LEFT JOIN [ProductCategories] ON [ProductCategories].[ID]=[Products].[CategoryID] WHERE [PolicyTypesLines].[Current]=1 AND [PolicyTypesLines].[HeaderID]=@PolicyTypeID ORDER BY [PolicyTypesLines].[Main] DESC, [Product] ASC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("PolicyTypeID", ID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Product product = MapToProduct(reader);
                            products.Add(product);
                        }
                    }
                }
            }
            return products;
        }
        public List<Product> GetAllNonInvestmentProducts(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<Product> products = new List<Product>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT [PolicyTypesLines].[EntryNo], [PolicyTypesLines].[ProductID],[Products].[Product],[ProductCategories].[Category] FROM [dbo].[PolicyTypesLines] LEFT JOIN [Products] ON [Products].[ID]=[PolicyTypesLines].[ProductID] LEFT JOIN [ProductCategories] ON [ProductCategories].[ID]=[Products].[CategoryID] WHERE [PolicyTypesLines].[Current]=1 AND [PolicyTypesLines].[HeaderID]=@PolicyTypeID AND ([Products].[CategoryID] IN (2,3,4))  ORDER BY [PolicyTypesLines].[Main] DESC, [Product] ASC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("PolicyTypeID", ID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Product product = MapToProduct(reader);
                            products.Add(product);
                        }
                    }
                }
            }
            return products;
        }
        public List<Product> GetAllInvestmentProducts(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<Product> products = new List<Product>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT [PolicyTypesLines].[EntryNo], [PolicyTypesLines].[ProductID],[Products].[Product],[ProductCategories].[Category] FROM [dbo].[PolicyTypesLines] LEFT JOIN [Products] ON [Products].[ID]=[PolicyTypesLines].[ProductID] LEFT JOIN [ProductCategories] ON [ProductCategories].[ID]=[Products].[CategoryID] WHERE ([Products].[CategoryID]=1) AND [PolicyTypesLines].[Current]=1 AND [PolicyTypesLines].[HeaderID]=@PolicyTypeID ORDER BY [Product] ASC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("PolicyTypeID", ID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Product product = MapToProduct(reader);
                            products.Add(product);
                        }
                    }
                }
            }
            return products;
        }
        public void UpdatePolicyTypeLine(PolicyTypesLines policyTypeLine)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"UPDATE [dbo].[PolicyTypesLines] 
                             SET HeaderID = @HeaderID, ProductID = @ProductID, 
                                 Optional = @Optional, Current = @Current, 
                                 AddedOn = @AddedOn, AddedBy = @AddedBy, 
                                 Archived = @Archived, ArchivedBy = @ArchivedBy, 
                                 ArchivedComment = @ArchivedComment, 
                                 ArchivedOn = @ArchivedOn, Deleted = @Deleted, 
                                 DeletedBy = @DeletedBy, DeletedComment = @DeletedComment, 
                                 DeletedOn = @DeletedOn 
                             WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, policyTypeLine);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void Archive(Guid ID, string AddedBy, DateTime AddedOn)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[PolicyTypesLines] Set [Archived]=1,[ArchivedBy]=@AddedBy,[ArchivedOn]=@AddedOn WHERE [ID]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", AddedOn);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeletePolicyTypeLine(Guid id)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM [dbo].[PolicyTypesLines] WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SetParameters(SqlCommand command, PolicyTypesLines policyTypeLine)
        {
            command.Parameters.AddWithValue("@ID", policyTypeLine.ID);
            command.Parameters.AddWithValue("@HeaderID", policyTypeLine.HeaderID);
            command.Parameters.AddWithValue("@ProductID", policyTypeLine.ProductID);
            command.Parameters.AddWithValue("@Optional", policyTypeLine.Optional);
            command.Parameters.AddWithValue("@Main", policyTypeLine.Main);
            command.Parameters.AddWithValue("@Current", policyTypeLine.Current);
            command.Parameters.AddWithValue("@AddedOn", (object)policyTypeLine.AddedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@AddedBy", (object)policyTypeLine.AddedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@Archived", (object)policyTypeLine.Archived ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedBy", (object)policyTypeLine.ArchivedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedComment", (object)policyTypeLine.ArchivedComment ?? DBNull.Value);
            command.Parameters.AddWithValue("@ArchivedOn", (object)policyTypeLine.ArchivedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@Deleted", (object)policyTypeLine.Deleted ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedBy", (object)policyTypeLine.DeletedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedComment", (object)policyTypeLine.DeletedComment ?? DBNull.Value);
            command.Parameters.AddWithValue("@DeletedOn", (object)policyTypeLine.DeletedOn ?? DBNull.Value);
        }

        private PolicyTypesLines MapPolicyTypeLineFromReader(SqlDataReader reader)
        {
            return new PolicyTypesLines
            {
                EntryNo = (int)reader["EntryNo"],
                ID = (Guid)reader["ID"],
                HeaderID = (Guid)reader["HeaderID"],
                ProductID = (Guid)reader["ProductID"],
                Optional = (byte)reader["Optional"],
                Main=(byte)reader["Main"],
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
        private Product MapToProduct(SqlDataReader reader)
        {
            return new Product
            {
                EntryNo = (int)reader["EntryNo"],
                ID = (Guid)reader["ProductID"],
                ProductName = (string)reader["Product"],
                Category= (string)reader["Category"]
            };
        }
    }

}
