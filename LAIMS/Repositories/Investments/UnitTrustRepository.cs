using LAIMS.Interfaces.Investments;
using LAIMS.Models.Investments;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Investments
{
    public class UnitTrustRepository: IUnitTrustRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public UnitTrustRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        // Create a new UnitTrust record
        public void Create(UnitTrust unitTrust)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "INSERT INTO UnitTrusts (ID, UnitTrust, Active, InceptionDate, UnitsIssued) VALUES (@ID, @UnitTrust, @Active, @InceptionDate, @UnitsIssued)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", unitTrust.ID);
                    command.Parameters.AddWithValue("@UnitTrust", unitTrust.UnitTrustName);
                    command.Parameters.AddWithValue("@Active", unitTrust.IsActive);
                    command.Parameters.AddWithValue("@InceptionDate", unitTrust.InceptionDate);
                    command.Parameters.AddWithValue("@UnitsIssued", unitTrust.UnitsIssued);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Read a UnitTrust record by ID
        public UnitTrust Read(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM UnitTrusts WHERE ID = @ID ORDER BY [UnitTrust] ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapUnitTrustFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }
        public List<UnitTrust> ReadAll()
        {
            List<UnitTrust> unitTrusts = new List<UnitTrust>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM UnitTrusts ORDER BY [UnitTrust] ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UnitTrust unitTrust = MapUnitTrustFromReader(reader);
                            unitTrusts.Add(unitTrust);
                        }
                    }
                }
            }

            return unitTrusts;
        }
        public DataTable GetTotals(Guid PolicyID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT  [UnitTrusts].[UnitTrust],[TotalUnits], [UnitTrusts].[LastUpdated] FROM [dbo].[PolicyUnits] LEFT JOIN [UnitTrusts] ON [UnitTrusts].[ID]=[PolicyUnits].[UnitTrustID] WHERE [PolicyID]=@PolicyID ORDER BY [UnitTrusts].[UnitTrust] ASC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetTotals(string PolicyNo)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("PolicyNo", PolicyNo);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "DECLARE @PolicyID uniqueidentifier; SELECT @PolicyID=[ID] FROM [Policy] WHERE [PolicyNo]=@PolicyNo; SELECT  [UnitTrusts].[UnitTrust],[TotalUnits], [UnitTrusts].[LastUpdated] FROM [dbo].[PolicyUnits] LEFT JOIN [UnitTrusts] ON [UnitTrusts].[ID]=[PolicyUnits].[UnitTrustID] WHERE [PolicyID]=@PolicyID ORDER BY [UnitTrusts].[UnitTrust] ASC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetSalesDetails(string PolicyNo)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("PolicyNo", PolicyNo);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "UnitPriceList_GetSalesDetails";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetSalesDetails(int ClaimID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("ClaimID", ClaimID);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "UnitPriceList_GetSalesDetailsByClaimID";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetLatestPurchaseTransactions(Guid PolicyId)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("PolicyID", PolicyId);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT TOP(100) [UnitTrusts].[UnitTrust],[PolicyUnitsLines].[Units],[UnitsPricesList].[OfferPrice],[UnitsPricesList].[BidPrice],[TransactionTypes].[Name] AS [TransactionType], [PolicyUnitsLines].[AddedOn] FROM [PolicyUnits] " +
                    "LEFT JOIN [PolicyUnitsLines] ON [PolicyUnitsLines].[PolicyUnitsID]=[PolicyUnits].[ID] LEFT JOIN [UnitTrusts] ON [UnitTrusts].[ID]=[PolicyUnits].[UnitTrustID] " +
                    "LEFT JOIN [UnitsPricesList] ON [UnitsPricesList].[ID]=[PolicyUnitsLines].[UnitPricesListID] LEFT JOIN [TransactionTypes] ON [TransactionTypes].[ID]=[PolicyUnitsLines].[TransactionTypeID] " +
                    "WHERE [PolicyUnitsLines].[TransactionTypeID]=3 AND [PolicyUnits].[PolicyID]=@PolicyID AND [PolicyUnits].[Archived]=0 ORDER BY [PolicyUnitsLines].[AddedOn] DESC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
		public DataTable GetLatestTransactions(Guid PolicyId)
		{
			DataTable DT = new DataTable();
			SqlConnection connection = new SqlConnection();
			connection.ConnectionString = Database;
			SqlCommand cmd = connection.CreateCommand();
			cmd.Parameters.AddWithValue("PolicyID", PolicyId);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "PolicyUnits_GetLatestTransactions";
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			da.Fill(DT);
			return DT;
		}
		public DataTable GetLatestSalesTransactions(Guid PolicyId)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("PolicyID", PolicyId);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT TOP(100) [UnitTrusts].[UnitTrust],[PolicyUnitsLines].[Units],[UnitsPricesList].[OfferPrice],[UnitsPricesList].[BidPrice],ISNULL([PolicyUnitsLines].[Units],0)*ISNULL([UnitsPricesList].[BidPrice],0) AS [Value],[TransactionTypes].[Name] AS [TransactionType], [PolicyUnitsLines].[AddedOn] FROM [PolicyUnits] " +
                    "LEFT JOIN [PolicyUnitsLines] ON [PolicyUnitsLines].[PolicyUnitsID]=[PolicyUnits].[ID] LEFT JOIN [UnitTrusts] ON [UnitTrusts].[ID]=[PolicyUnits].[UnitTrustID] " +
                    "LEFT JOIN [UnitsPricesList] ON [UnitsPricesList].[ID]=[PolicyUnitsLines].[UnitPricesListID] LEFT JOIN [TransactionTypes] ON [TransactionTypes].[ID]=[PolicyUnitsLines].[TransactionTypeID] " +
                    "WHERE [PolicyUnitsLines].[TransactionTypeID]=2 AND [PolicyUnits].[PolicyID]=@PolicyID AND [PolicyUnits].[Archived]=0 ORDER BY [PolicyUnitsLines].[AddedOn] DESC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetLatestSalesTransactions(string PolicyNo)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("PolicyNo", PolicyNo);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "DECLARE @PolicyID uniqueidentifier; SELECT @PolicyID=[ID] FROM [Policy] WHERE [PolicyNo]=@PolicyNo; " + 
                    "SELECT TOP(100) [UnitTrusts].[UnitTrust],[PolicyUnitsLines].[Units],[UnitsPricesList].[OfferPrice],[UnitsPricesList].[BidPrice],ISNULL([PolicyUnitsLines].[Units],0)*ISNULL([UnitsPricesList].[BidPrice],0) AS [Value],[TransactionTypes].[Name] AS [TransactionType], [PolicyUnitsLines].[AddedOn] FROM [PolicyUnits] " +
                    "LEFT JOIN [PolicyUnitsLines] ON [PolicyUnitsLines].[PolicyUnitsID]=[PolicyUnits].[ID] LEFT JOIN [UnitTrusts] ON [UnitTrusts].[ID]=[PolicyUnits].[UnitTrustID] " +
                    "LEFT JOIN [UnitsPricesList] ON [UnitsPricesList].[ID]=[PolicyUnitsLines].[UnitPricesListID] LEFT JOIN [TransactionTypes] ON [TransactionTypes].[ID]=[PolicyUnitsLines].[TransactionTypeID] " +
                    "WHERE [PolicyUnitsLines].[TransactionTypeID]=2 AND [PolicyUnits].[PolicyID]=@PolicyID AND [PolicyUnits].[Archived]=0 ORDER BY [PolicyUnitsLines].[AddedOn] DESC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetLatestPurchaseTransactions(string PolicyNo)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("PolicyNo", PolicyNo);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "DECLARE @PolicyID uniqueidentifier; SELECT @PolicyID=[ID] FROM [Policy] WHERE [PolicyNo]=@PolicyNo; " +
                " SELECT TOP(100) [UnitTrusts].[UnitTrust],[PolicyUnitsLines].[Units],[UnitsPricesList].[OfferPrice],[UnitsPricesList].[BidPrice],[TransactionTypes].[Name] AS [TransactionType], [PolicyUnitsLines].[AddedOn] FROM [PolicyUnits] " +
                    "LEFT JOIN [PolicyUnitsLines] ON [PolicyUnitsLines].[PolicyUnitsID]=[PolicyUnits].[ID] LEFT JOIN [UnitTrusts] ON [UnitTrusts].[ID]=[PolicyUnits].[UnitTrustID] " +
                    "LEFT JOIN [UnitsPricesList] ON [UnitsPricesList].[ID]=[PolicyUnitsLines].[UnitPricesListID] LEFT JOIN [TransactionTypes] ON [TransactionTypes].[ID]=[PolicyUnitsLines].[TransactionTypeID] " +
                    "WHERE [PolicyUnitsLines].[TransactionTypeID]=3 AND [PolicyUnits].[PolicyID]=@PolicyID AND [PolicyUnits].[Archived]=0 ORDER BY [PolicyUnitsLines].[AddedOn] DESC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public decimal GetTotalAvailableunits(Guid PolicyId, Guid TrustID)
        {  
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @Qty decimal(18,7)=0;SELECT @Qty=[TotalUnits] FROM [PolicyUnits] WHERE [PolicyID]=@PolicyID AND [UnitTrustID]=@TrustID; SELECT @Qty";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyId", PolicyId);
                    command.Parameters.AddWithValue("@TrustID", TrustID);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        // Update a UnitTrust record
        public void Update(UnitTrust unitTrust)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE UnitTrusts SET UnitTrust = @UnitTrust, Active = @Active, InceptionDate = @InceptionDate, UnitsIssued = @UnitsIssued WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", unitTrust.ID);
                    command.Parameters.AddWithValue("@UnitTrust", unitTrust.UnitTrustName);
                    command.Parameters.AddWithValue("@Active", unitTrust.IsActive);
                    command.Parameters.AddWithValue("@InceptionDate", unitTrust.InceptionDate);
                    command.Parameters.AddWithValue("@UnitsIssued", unitTrust.UnitsIssued);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Delete a UnitTrust record
        public void Delete(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM UnitTrusts WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Helper method to map UnitTrust data from SqlDataReader to UnitTrust object
        private UnitTrust MapUnitTrustFromReader(SqlDataReader reader)
        {
            return new UnitTrust
            {
                ID = (Guid)reader["ID"],
                UnitTrustName = reader["UnitTrust"].ToString(),
                IsActive = (bool)reader["Active"],
                InceptionDate = (DateTime)reader["InceptionDate"],
                UnitsIssued = (decimal)reader["UnitsIssued"]
            };
        }
    }
} 
