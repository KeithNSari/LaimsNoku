using LAIMS.Interfaces.Investments;
using LAIMS.Models.Investments;
using LAIMS.Models.Policies;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Investments
{
    public class UnitTrustsLineRepository: IUnitTrustsLineRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public UnitTrustsLineRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public void Add(UnitTrustsLine unitTrustsLine)
            {
                using (var connection = new SqlConnection(Database))
                {
                    var command = new SqlCommand("INSERT INTO [dbo].[UnitTrustsLines] (HeaderID, ResidualValue, ResidualValueIsPercentage, CurrencyID, MinimumCashWithdrawal, MinimumSurrenderValue, WaitingPeriod, EffectiveDate, ValueMode, AddedBy, AddedOn) VALUES (@HeaderID, @ResidualValue, @ResidualValueIsPercentage, @CurrencyID, @MinimumCashWithdrawal, @MinimumSurrenderValue, @WaitingPeriod, @EffectiveDate, @ValueMode, @AddedBy, @AddedOn)", connection);

                    command.Parameters.AddWithValue("@HeaderID", unitTrustsLine.HeaderID);
                    command.Parameters.AddWithValue("@ResidualValue", unitTrustsLine.ResidualValue.HasValue ? (object)unitTrustsLine.ResidualValue.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@ResidualValueIsPercentage", unitTrustsLine.ResidualValueIsPercentage.HasValue ? (object)unitTrustsLine.ResidualValueIsPercentage.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@CurrencyID", unitTrustsLine.CurrencyID);
                    command.Parameters.AddWithValue("@MinimumCashWithdrawal", unitTrustsLine.MinimumCashWithdrawal);
                    command.Parameters.AddWithValue("@MinimumSurrenderValue", unitTrustsLine.MinimumSurrenderValue.HasValue ? (object)unitTrustsLine.MinimumSurrenderValue.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@WaitingPeriod", unitTrustsLine.WaitingPeriod);
                    command.Parameters.AddWithValue("@EffectiveDate", unitTrustsLine.EffectiveDate);
                    command.Parameters.AddWithValue("@ValueMode", unitTrustsLine.ValueMode.HasValue ? (object)unitTrustsLine.ValueMode.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", unitTrustsLine.AddedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", unitTrustsLine.AddedOn.HasValue ? (object)unitTrustsLine.AddedOn.Value : DBNull.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            public UnitTrustsLine Get(int id)
            {
                using (var connection = new SqlConnection(Database))
                {
                    var command = new SqlCommand("SELECT * FROM [dbo].[UnitTrustsLines] WHERE ID = @ID", connection);
                    command.Parameters.AddWithValue("@ID", id);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UnitTrustsLine
                            {
                                ID = (int)reader["ID"],
                                HeaderID = (Guid)reader["HeaderID"],
                                ResidualValue = reader["ResidualValue"] != DBNull.Value ? (decimal?)reader["ResidualValue"] : null,
                                ResidualValueIsPercentage = reader["ResidualValueIsPercentage"] != DBNull.Value ? (byte?)reader["ResidualValueIsPercentage"] : null,
                                CurrencyID = (int)reader["CurrencyID"],
                                MinimumCashWithdrawal = (decimal)reader["MinimumCashWithdrawal"],
                                MinimumSurrenderValue = reader["MinimumSurrenderValue"] != DBNull.Value ? (decimal?)reader["MinimumSurrenderValue"] : null,
                                WaitingPeriod = (int)reader["WaitingPeriod"],
                                EffectiveDate = (DateTime)reader["EffectiveDate"],
                                ValueMode = reader["ValueMode"] != DBNull.Value ? (int?)reader["ValueMode"] : null,
                                AddedBy = reader["AddedBy"] != DBNull.Value ? (string)reader["AddedBy"] : null,
                                AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime?)reader["AddedOn"] : null
                            };
                        }
                    }
                }
                return null;
            }

            public List<UnitTrustsLine> GetAll()
            {
                var unitTrustsLines = new List<UnitTrustsLine>();

                using (var connection = new SqlConnection(Database))
                {
                    var command = new SqlCommand("SELECT * FROM [dbo].[UnitTrustsLines]", connection);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            unitTrustsLines.Add(new UnitTrustsLine
                            {
                                ID = (int)reader["ID"],
                                HeaderID = (Guid)reader["HeaderID"],
                                ResidualValue = reader["ResidualValue"] != DBNull.Value ? (decimal?)reader["ResidualValue"] : null,
                                ResidualValueIsPercentage = reader["ResidualValueIsPercentage"] != DBNull.Value ? (byte?)reader["ResidualValueIsPercentage"] : null,
                                CurrencyID = (int)reader["CurrencyID"],
                                MinimumCashWithdrawal = (decimal)reader["MinimumCashWithdrawal"],
                                MinimumSurrenderValue = reader["MinimumSurrenderValue"] != DBNull.Value ? (decimal?)reader["MinimumSurrenderValue"] : null,
                                WaitingPeriod = (int)reader["WaitingPeriod"],
                                EffectiveDate = (DateTime)reader["EffectiveDate"],
                                ValueMode = reader["ValueMode"] != DBNull.Value ? (int?)reader["ValueMode"] : null,
                                AddedBy = reader["AddedBy"] != DBNull.Value ? (string)reader["AddedBy"] : null,
                                AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime?)reader["AddedOn"] : null
                            });
                        }
                    }
                }
                return unitTrustsLines;
            }

            public void Update(UnitTrustsLine unitTrustsLine)
            {
                using (var connection = new SqlConnection(Database))
                {
                    var command = new SqlCommand("UPDATE [dbo].[UnitTrustsLines] SET HeaderID = @HeaderID, ResidualValue = @ResidualValue, ResidualValueIsPercentage = @ResidualValueIsPercentage, CurrencyID = @CurrencyID, MinimumCashWithdrawal = @MinimumCashWithdrawal, MinimumSurrenderValue = @MinimumSurrenderValue, WaitingPeriod = @WaitingPeriod, EffectiveDate = @EffectiveDate, ValueMode = @ValueMode, AddedBy = @AddedBy, AddedOn = @AddedOn WHERE ID = @ID", connection);

                    command.Parameters.AddWithValue("@ID", unitTrustsLine.ID);
                    command.Parameters.AddWithValue("@HeaderID", unitTrustsLine.HeaderID);
                    command.Parameters.AddWithValue("@ResidualValue", unitTrustsLine.ResidualValue.HasValue ? (object)unitTrustsLine.ResidualValue.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@ResidualValueIsPercentage", unitTrustsLine.ResidualValueIsPercentage.HasValue ? (object)unitTrustsLine.ResidualValueIsPercentage.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@CurrencyID", unitTrustsLine.CurrencyID);
                    command.Parameters.AddWithValue("@MinimumCashWithdrawal", unitTrustsLine.MinimumCashWithdrawal);
                    command.Parameters.AddWithValue("@MinimumSurrenderValue", unitTrustsLine.MinimumSurrenderValue.HasValue ? (object)unitTrustsLine.MinimumSurrenderValue.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@WaitingPeriod", unitTrustsLine.WaitingPeriod);
                    command.Parameters.AddWithValue("@EffectiveDate", unitTrustsLine.EffectiveDate);
                    command.Parameters.AddWithValue("@ValueMode", unitTrustsLine.ValueMode.HasValue ? (object)unitTrustsLine.ValueMode.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", unitTrustsLine.AddedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", unitTrustsLine.AddedOn.HasValue ? (object)unitTrustsLine.AddedOn.Value : DBNull.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            public void Delete(int id)
            {
                using (var connection = new SqlConnection(Database))
                {
                    var command = new SqlCommand("DELETE FROM [dbo].[UnitTrustsLines] WHERE ID = @ID", connection);
                    command.Parameters.AddWithValue("@ID", id);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        public DataTable GetLatest()
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "UnitTrustsLines_GetLatest"; 
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }

    }
}
