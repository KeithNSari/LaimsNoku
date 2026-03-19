using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Lifeproducts
{
    public class ProductRepository: IProductRepository 
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment; 
        public ProductRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

        } 

        public void InsertProduct(Product product)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "INSERT INTO Products (ID,CategoryID,TermID, Product, Description, AddedBy) VALUES (@ID,@CategoryID,@TermID, @ProductName, @Description, @AddedBy)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", product.ID);
                    command.Parameters.AddWithValue("@CategoryID", product.CategoryID);
                    command.Parameters.AddWithValue("@TermID", product.TermID);
                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@Description", product.Description);
                    command.Parameters.AddWithValue("@AddedBy", product.AddedBy);

                    command.ExecuteNonQuery();
                }
            }
        }
        public int CheckExistence(string ProductName)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM Products Where Product=@ProductName And DELETED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                { 
                    command.Parameters.AddWithValue("@ProductName",ProductName); 
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int CheckOtherExistence(string ProductName,Guid ProductID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM Products Where Product=@ProductName And ID!=@ID And DELETED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductName", ProductName);
                    command.Parameters.AddWithValue("@ID", ProductID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int CheckExistence(Guid ProductID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM Products Where ID=@ID And DELETED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ProductID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void UpdateProduct(Product product)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE Products SET Product = @ProductName,TermID=@TermID,Description = @Description, CategoryID=@CategoryID WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@CategoryID", product.CategoryID);
                    command.Parameters.AddWithValue("@TermID", product.TermID);
                    command.Parameters.AddWithValue("@Description", product.Description);
                    //command.Parameters.AddWithValue("@AddedBy", (object)product.AddedBy ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ID", product.ID);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteProduct(Guid ID, string DeletedBy, DateTime DeletedOn)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Update [dbo].[Products] SET [Deleted]=1,[DeletedBy]=@DeletedBy,[DeletedOn]=@DeletedOn WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("DeletedBy", DeletedBy);
                    command.Parameters.AddWithValue("DeletedOn", DeletedOn);
                    command.ExecuteNonQuery();
                }
            }
        }
        public Product GetProduct(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM Products WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID",ID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToProduct(reader);
                        }
                    }
                }
            }

            return null;
        }
        public DataTable Get()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [Products].[EntryNo],[Products].[ID],[Product],[CategoryID],[Category],[Description],[Products].[AddedOn],[Products].[AddedBy],[Username] FROM [dbo].[Products] LEFT JOIN [ProductCategories] ON [Products].[CategoryID]=[ProductCategories].[ID] LEFT JOIN [AspNetUsers] ON [AspNetUsers].[Id]=[Products].[AddedBy] WHERE [DELETED]=0 AND [Archived]=0 ORDER BY [Category] Asc, [Product] Asc";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetByID(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [Products].[EntryNo],[Products].[ID],[Product],[Products].[TermID],[ProductTerms].[Term],[CategoryID],[Category],[Description],[Products].[AddedOn],[Products].[AddedBy],[Username] FROM [dbo].[Products] LEFT JOIN [ProductCategories] ON [Products].[CategoryID]=[ProductCategories].[ID] LEFT JOIN [AspNetUsers] ON [AspNetUsers].[Id]=[Products].[AddedBy] LEFT JOIN [ProductTerms] On [Products].[TermID]=[ProductTerms].[TermID] WHERE [DELETED]=0 AND [Archived]=0 AND [Products].[ID]=@ProductID ORDER BY [Category] Asc, [Product] Asc";
            cmd.Parameters.AddWithValue("ProductID", ID);  
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public List<Product> GetAllProducts()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<Product> products = new List<Product>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM Products Where [Deleted]=0 AND [ARCHIVED]=0 ORDER BY [Product] ASC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Product product = MapToProduct(reader);
                            products .Add(product);
                        }
                    }
                }
            }
            return products;
        }
        public Guid GetID(string ProductName)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection connection = new(Database);
            connection.Open();
            string query = "DECLARE @ID uniqueidentifier='00000000-0000-0000-0000-000000000000'; SELECT @ID=[ID] FROM [dbo].[Products] Where [Product]=@Product; SELECT @ID";
            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("Product", ProductName);
            return Guid.Parse(command.ExecuteScalar().ToString());
        }
        private Product MapToProduct(SqlDataReader reader)
        {
            return new Product
            {
                EntryNo = (int)reader["EntryNo"],
                ID = (Guid)reader["ID"],
                ProductName = (string)reader["Product"],
                TermID = (int)reader["TermID"],
                Description = (string)reader["Description"],
                AddedOn = reader["AddedOn"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["AddedOn"],
                AddedBy = reader["AddedBy"] == DBNull.Value ? null : (string)reader["AddedBy"],
                // Map other properties similarly
            };
        }
    }
}
