using LAIMS.Models.Commissions;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Commissions
{
    public class ProductCommissionTypeRepository
    {
        private readonly string connectionString;

        public ProductCommissionTypeRepository(string connectionString)
        {
            this.connectionString = connectionString;
        } 
        public void AddProductCommissionType(ProductCommissionType productCommissionType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "INSERT INTO ProductCommissionTypes (CommissionTypeID, ProductID, FunctionType, FunctionID, CommissionRate, CPPStarts, CPPEnds) " +
                             "VALUES (@CommissionTypeID, @ProductID, @FunctionType, @FunctionID, @CommissionRate, @CPPStarts, @CPPEnds)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CommissionTypeID", productCommissionType.CommissionTypeID);
                    command.Parameters.AddWithValue("@ProductID", productCommissionType.ProductID);
                    command.Parameters.AddWithValue("@FunctionType", productCommissionType.FunctionType);
                    command.Parameters.AddWithValue("@FunctionID", productCommissionType.FunctionID);
                    command.Parameters.AddWithValue("@CommissionRate", productCommissionType.CommissionRate);
                    command.Parameters.AddWithValue("@CPPStarts", productCommissionType.CPPStarts);
                    command.Parameters.AddWithValue("@CPPEnds", productCommissionType.CPPEnds);

                    command.ExecuteNonQuery();
                }
            }
        } 
        public List<ProductCommissionType> GetAllProductCommissionTypes()
        {
            List<ProductCommissionType> productCommissionTypes = new List<ProductCommissionType>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM ProductCommissionTypes";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProductCommissionType productCommissionType = MapDataReaderToProductCommissionType(reader);
                            productCommissionTypes.Add(productCommissionType);
                        }
                    }
                }
            }

            return productCommissionTypes;
        } 
        public void UpdateProductCommissionType(ProductCommissionType productCommissionType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "UPDATE ProductCommissionTypes SET CommissionTypeID = @CommissionTypeID, " +
                             "ProductID = @ProductID, FunctionType = @FunctionType, FunctionID = @FunctionID, " +
                             "CommissionRate = @CommissionRate, CPPStarts = @CPPStarts, CPPEnds = @CPPEnds " +
                             "WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", productCommissionType.ID);
                    command.Parameters.AddWithValue("@CommissionTypeID", productCommissionType.CommissionTypeID);
                    command.Parameters.AddWithValue("@ProductID", productCommissionType.ProductID);
                    command.Parameters.AddWithValue("@FunctionType", productCommissionType.FunctionType);
                    command.Parameters.AddWithValue("@FunctionID", productCommissionType.FunctionID);
                    command.Parameters.AddWithValue("@CommissionRate", productCommissionType.CommissionRate);
                    command.Parameters.AddWithValue("@CPPStarts", productCommissionType.CPPStarts);
                    command.Parameters.AddWithValue("@CPPEnds", productCommissionType.CPPEnds);

                    command.ExecuteNonQuery();
                }
            }
        } 
        public void DeleteProductCommissionType(int productCommissionTypeId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM ProductCommissionTypes WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", productCommissionTypeId);

                    command.ExecuteNonQuery();
                }
            }
        }
         
        private ProductCommissionType MapDataReaderToProductCommissionType(SqlDataReader reader)
        {
            return new ProductCommissionType
            {
                ID = (int)reader["ID"],
                CommissionTypeID = (int)reader["CommissionTypeID"],
                ProductID = (Guid)reader["ProductID"],
                FunctionType = (byte)reader["FunctionType"],
                FunctionID = (int)reader["FunctionID"],
                CommissionRate = (decimal)reader["CommissionRate"],
                CPPStarts = (int)reader["CPPStarts"],
                CPPEnds = (int)reader["CPPEnds"],
            };
        }
    }
}
