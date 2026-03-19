using LAIMS.Interfaces.Investments;
using LAIMS.Models.Investments;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Transactions;

namespace LAIMS.Repositories.Investments
{
    public class UnitsPricesListRepository: IUnitsPricesListRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public UnitsPricesListRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        // Create a new UnitsPricesList record
        public void Create(UnitsPricesList unitsPricesList)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"INSERT INTO UnitsPricesList (UnitTrustID, CurrencyID, BidPrice, OfferPrice, EffectiveDate, AddedBy, AddedOn)
                             VALUES (@UnitTrustID, @CurrencyID, @BidPrice, @OfferPrice, @EffectiveDate, @AddedBy, @AddedOn)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UnitTrustID", unitsPricesList.UnitTrustID);
                    command.Parameters.AddWithValue("@CurrencyID", unitsPricesList.CurrencyID);
                    command.Parameters.AddWithValue("@BidPrice", unitsPricesList.BidPrice);
                    command.Parameters.AddWithValue("@OfferPrice", unitsPricesList.OfferPrice);
                    command.Parameters.AddWithValue("@EffectiveDate", unitsPricesList.EffectiveDate);
                    command.Parameters.AddWithValue("@AddedBy", unitsPricesList.AddedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", unitsPricesList.AddedOn ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Read a UnitsPricesList record by ID
        public UnitsPricesList Read(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM UnitsPricesList WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UnitsPricesList
                            {
                                ID = (int)reader["ID"],
                                UnitTrustID = (Guid)reader["UnitTrustID"],
                                CurrencyID = (int)reader["CurrencyID"],
                                BidPrice = (decimal)reader["BidPrice"],
                                OfferPrice = (decimal)reader["OfferPrice"],
                                EffectiveDate = (DateTime)reader["EffectiveDate"],
                                AddedBy = reader["AddedBy"] is DBNull ? null : (string)reader["AddedBy"],
                                AddedOn = reader["AddedOn"] is DBNull ? (DateTime?)null : (DateTime)reader["AddedOn"]
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Read all UnitsPricesList records
        public List<UnitsPricesList> GetCurrentPrices()
        {
            List<UnitsPricesList> unitsPricesLists = new List<UnitsPricesList>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UnitPriceList_GetCurrentPrices";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UnitsPricesList unitsPricesList = MapUnitsPricesListFromReader(reader);
                            unitsPricesLists.Add(unitsPricesList);
                        }
                    }
                }
            }

            return unitsPricesLists;
        }
        public UnitsPricesList GetLatestPrice(int CurrencyID, Guid UnitTrustID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT TOP(1) * FROM [UnitsPricesList] WHERE [EffectiveDate] <= GETDATE()  " +
                    "AND [UnitTrustID]=@UnitTrustID AND [CurrencyID]=@CurrencyID  " +
                    "AND [Archived]=0 ORDER BY [EffectiveDate] DESC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CurrencyID", CurrencyID);
                    command.Parameters.AddWithValue("@UnitTrustID", UnitTrustID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UnitsPricesList
                            {
                                ID = (int)reader["ID"],
                                UnitTrustID = (Guid)reader["UnitTrustID"], 
                                CurrencyID = (int)reader["CurrencyID"],
                                BidPrice = (decimal)reader["BidPrice"],
                                OfferPrice = (decimal)reader["OfferPrice"],
                                EffectiveDate = (DateTime)reader["EffectiveDate"],
                                AddedBy = reader["AddedBy"] is DBNull ? null : (string)reader["AddedBy"],
                                AddedOn = reader["AddedOn"] is DBNull ? (DateTime?)null : (DateTime)reader["AddedOn"]
                            };
                        }
                    }
                }
            }

            return null;
        }
        //Get 100 latest prices
        public DataTable GetPriceHistory(Guid UnitTrustID)
        { 
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("UnitTrustID", UnitTrustID);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT Top (100) [UnitsPricesList].[ID],[Currencies].[Name] AS [Currency],[BidPrice],[OfferPrice],[EffectiveDate],[UnitsPricesList].[AddedOn],[AspNetUsers].[UserName] AS [AddedBy] FROM [dbo].[UnitsPricesList] LEFT JOIN [Currencies] ON [Currencies].[ID]=[UnitsPricesList].[CurrencyID] LEFT JOIN [AspNetUsers] ON [AspNetUsers].[Id]=[UnitsPricesList].[AddedBy] WHERE [UnitTrustID]=@UnitTrustID ORDER BY [UnitsPricesList].[ID] DESC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        } 
        public DataTable GetLatest()
        { 
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand(); 
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT Top (100) [UnitsPricesList].[ID],[Currencies].[Name] AS [Currency],[BidPrice],[OfferPrice],[EffectiveDate],[UnitsPricesList].[AddedOn],[AspNetUsers].[UserName] AS [AddedBy] FROM [dbo].[UnitsPricesList] LEFT JOIN [Currencies] ON [Currencies].[ID]=[UnitsPricesList].[CurrencyID] LEFT JOIN [AspNetUsers] ON [AspNetUsers].[Id]=[UnitsPricesList].[AddedBy] ORDER BY [UnitsPricesList].[ID] DESC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public int GetPolicyUnitsHeader(Guid PolicyId, Guid TrustID)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @ID INT=0;SELECT @ID=[ID]  FROM [PolicyUnits] WHERE [PolicyID]=@PolicyId " +
                    "AND [UnitTrustID] = @TrustID AND [Archived]=0;SELECT @ID AS [ID]";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyId", PolicyId);
                    command.Parameters.AddWithValue("@TrustID", TrustID);
                    return Convert.ToInt32(command.ExecuteScalar().ToString());
                }
            }
        }
        public int InsertPolicyUnitsHeader(Guid PolicyId, Guid TrustID, string AddedBy)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @Date datetime2(7)=GETDATE(); INSERT INTO [PolicyUnits] ([PolicyID],[UnitTrustID],[TotalUnits],[LastUpdated],[AddedBy],[AddedOn],[Archived]) OUTPUT inserted.[ID] " +
                    "VALUES (@PolicyID,@TrustID,0,@Date,@AddedBy,@Date,0)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (connection)
                    {
                        command.Parameters.AddWithValue("@PolicyId", PolicyId);
                        command.Parameters.AddWithValue("@TrustID", TrustID);
                        command.Parameters.AddWithValue("@AddedBy", AddedBy);
                        return Convert.ToInt32(command.ExecuteScalar().ToString());
                    }
                }
            }
        }
        public bool Buy(Guid PolicyId, Guid TrustID, int PolicyUnitsID, decimal Units, int PriceID, int TransactionTypeID, string AddedBy)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [PolicyUnits] SET [PolicyUnits].[TotalUnits]=[TotalUnits]+@Units, [PolicyUnits].[LastUpdated]=GetDate() " +
                    "WHERE [Policyid]=@PolicyId AND [UnitTrustID]=@TrustID; " +

                    "INSERT INTO [PolicyUnitsLines] ([PolicyUnitsID],[UnitPricesListID],[Units],[TransactionTypeID],[AddedBy],[AddedOn],[Archived]) " +
                    "VALUES(@PolicyUnitsID,@PriceID,@Units,@TransactionTypeID,@AddedBy,GETDATE(),0)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyId", PolicyId);
                    command.Parameters.AddWithValue("@TrustID", TrustID);
                    command.Parameters.AddWithValue("@PolicyUnitsID", PolicyUnitsID);
                    command.Parameters.AddWithValue("@PriceID", PriceID);
                    command.Parameters.AddWithValue("@Units", Units);
                    command.Parameters.AddWithValue("@TransactionTypeID", TransactionTypeID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);

                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public int Sell(Guid PolicyId, Guid TrustID, int PolicyUnitsID, decimal Units, int PriceID, int TransactionTypeID, string AddedBy)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [PolicyUnits] SET [PolicyUnits].[TotalUnits]=[TotalUnits]-@Units, [PolicyUnits].[LastUpdated]=GetDate() " +
                    "WHERE [Policyid]=@PolicyId AND [UnitTrustID]=@TrustID; " +

                    "INSERT INTO [PolicyUnitsLines] ([PolicyUnitsID],[UnitPricesListID],[Units],[TransactionTypeID],[AddedBy],[AddedOn],[Archived]) " +
                    "OUTPUT inserted.[ID]" +
                    "VALUES(@PolicyUnitsID,@PriceID,@Units,@TransactionTypeID,@AddedBy,GETDATE(),0)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyId", PolicyId);
                    command.Parameters.AddWithValue("@TrustID", TrustID);
                    command.Parameters.AddWithValue("@PolicyUnitsID", PolicyUnitsID);
                    command.Parameters.AddWithValue("@PriceID", PriceID);
                    command.Parameters.AddWithValue("@Units", Units);
                    command.Parameters.AddWithValue("@TransactionTypeID", TransactionTypeID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int Sell(Guid PolicyId, Guid TrustID, int ClaimID, int PolicyUnitsID, decimal Units, int PriceID, int TransactionTypeID, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [PolicyUnits] SET [PolicyUnits].[TotalUnits]=[TotalUnits]-@Units, [PolicyUnits].[LastUpdated]=GetDate() " +
                    "WHERE [Policyid]=@PolicyId AND [UnitTrustID]=@TrustID; DECLARE @Amount decimal (18,7); " +
                    "DECLARE @Price decimal(18,7); SELECT @Price=[BidPrice] FROM [dbo].[UnitsPricesList]  WHERE [ID]=@PriceID; SET @Amount=@Price*@Units;" +

                    "INSERT INTO [PolicyUnitsLines] ([ClaimID],[PolicyUnitsID],[UnitPricesListID],[Units],[Amount],[TransactionTypeID],[AddedBy],[AddedOn],[Archived]) " +
                    "OUTPUT inserted.[ID]" +
                    "VALUES(@ClaimID,@PolicyUnitsID,@PriceID,@Units,@Amount,@TransactionTypeID,@AddedBy,GETDATE(),0)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyId", PolicyId);
                    command.Parameters.AddWithValue("@ClaimID", ClaimID);
                    command.Parameters.AddWithValue("@TrustID", TrustID);
                    command.Parameters.AddWithValue("@PolicyUnitsID", PolicyUnitsID);
                    command.Parameters.AddWithValue("@PriceID", PriceID);
                    command.Parameters.AddWithValue("@Units", Units);
                    command.Parameters.AddWithValue("@TransactionTypeID", TransactionTypeID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int ProposeSell(int ClaimID, Guid PolicyId, Guid TrustID, int PolicyUnitsID, decimal Units, int PriceID, decimal Amount,string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "INSERT INTO [PolicyUnitsLines] ([ClaimID],[PolicyUnitsID],[UnitPricesListID],[Units],[Amount],[TransactionTypeID],[AddedBy],[AddedOn],[Archived]) " +
                    "OUTPUT inserted.[ID]" +
                    "VALUES(@ClaimID,@PolicyUnitsID,@PriceID,@Units,@Amount,4,@AddedBy,GETDATE(),0)"; //4 is proposed sell, it doesnt affect balances
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyId", PolicyId);
                    command.Parameters.AddWithValue("@TrustID", TrustID);
                    command.Parameters.AddWithValue("@PolicyUnitsID", PolicyUnitsID);
                    command.Parameters.AddWithValue("@PriceID", PriceID);
                    command.Parameters.AddWithValue("@Units", Units);
                    command.Parameters.AddWithValue("@Amount", Amount);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.Parameters.AddWithValue("@ClaimID", ClaimID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
       
        public void UpdateInvestmentContentBalance(Guid PolicyID, decimal InvestmentContent)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [Policy] SET [InvestmentContentBalance]=[InvestmentContentBalance]-@InvestmentContent" +
                    ",[InvestmentContentTotalDebit]=[InvestmentContentTotalDebit]+@InvestmentContent " +
                    "WHERE [Policy].[ID]=@PolicyID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@InvestmentContent", InvestmentContent);
                    command.ExecuteNonQuery();
                }
            }
        }
        // Update a UnitsPricesList record
        public void Update(UnitsPricesList unitsPricesList)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"UPDATE UnitsPricesList 
                             SET UnitTrustID = @UnitTrustID, CurrencyID = @CurrencyID, 
                                 BidPrice = @BidPrice, OfferPrice = @OfferPrice, 
                                 EffectiveDate = @EffectiveDate, AddedBy = @AddedBy, AddedOn = @AddedOn
                             WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", unitsPricesList.ID);
                    command.Parameters.AddWithValue("@UnitTrustID", unitsPricesList.UnitTrustID);
                    command.Parameters.AddWithValue("@CurrencyID", unitsPricesList.CurrencyID);
                    command.Parameters.AddWithValue("@BidPrice", unitsPricesList.BidPrice);
                    command.Parameters.AddWithValue("@OfferPrice", unitsPricesList.OfferPrice);
                    command.Parameters.AddWithValue("@EffectiveDate", unitsPricesList.EffectiveDate);
                    command.Parameters.AddWithValue("@AddedBy", unitsPricesList.AddedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", unitsPricesList.AddedOn ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Delete a UnitsPricesList record
        public void Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM UnitsPricesList WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Helper method to map UnitsPricesList data from SqlDataReader to UnitsPricesList object
        private UnitsPricesList MapUnitsPricesListFromReader(SqlDataReader reader)
        {
            return new UnitsPricesList
            {
                ID = (int)reader["ID"],
                UnitTrustID = (Guid)reader["UnitTrustID"],
                UnitTrust= reader["UnitTrust"] is DBNull ? null : (string)reader["UnitTrust"],
                CurrencyID = (int)reader["CurrencyID"],
                BidPrice = (decimal)reader["BidPrice"],
                OfferPrice = (decimal)reader["OfferPrice"],
                EffectiveDate = (DateTime)reader["EffectiveDate"]
                //,
                //AddedBy = reader["AddedBy"] is DBNull ? null : (string)reader["AddedBy"],
                //AddedOn = reader["AddedOn"] is DBNull ? (DateTime?)null : (DateTime)reader["AddedOn"]
            };
        }
    }
}
