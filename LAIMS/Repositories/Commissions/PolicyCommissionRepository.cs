using LAIMS.Interfaces.Commissions;
using LAIMS.Models.Commissions;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Commissions
{
    public class PolicyCommissionRepository: IPolicyCommissionRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PolicyCommissionRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public List<PolicyCommission> GetAllPolicyCommissions()
        {
            List<PolicyCommission> policyCommissions = new List<PolicyCommission>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM PolicyCommissions";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyCommission policyCommission = MapReaderToPolicyCommission(reader);
                            policyCommissions.Add(policyCommission);
                        }
                    }
                }
            }

            return policyCommissions;
        }

        public PolicyCommission GetPolicyCommissionById(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM PolicyCommissions WHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToPolicyCommission(reader);
                        }
                    }
                }
            }

            return null;
        }

        public void AddPolicyCommission(PolicyCommission policyCommission)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "INSERT INTO PolicyCommissions([PolicyTypeCommissionsID],[PolicyPremiumLineID],[IntermediaryID],[Commission],[CPPStarts],[CPPEnds],[StatusID],[StatusDate],[AddedOn],[AddedBy]) VALUES (@PolicyTypeCommissionsID, @PolicyPremiumLineID, @IntermediaryID, @Commission, @CPPStarts, @CPPEnds, 4000, GetDate(), GetDate(), @AddedBy)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    MapPolicyCommissionToParameters(policyCommission, command);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdatePolicyCommission(PolicyCommission policyCommission)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "UPDATE PolicyCommissions SET PolicyTypeCommissionsID = @PolicyTypeCommissionsID, PolicyPremiumLineID = @PolicyPremiumLineID, IntermediaryID = @IntermediaryID, Commission = @Commission, CPPStarts = @CPPStarts, CPPEnds = @CPPEnds, StatusID = @StatusID, StatusDate = @StatusDate, AddedOn = @AddedOn, AddedBy = @AddedBy WHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", policyCommission.ID);
                    MapPolicyCommissionToParameters(policyCommission, command);

                    command.ExecuteNonQuery();
                }
            }
        }
        public decimal CalculateCommission(string ProcedureName, int PolicyTypeCommissionID, decimal CommissionRate,Guid PolicyID, int PolicyPremiumID)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();  
                using (SqlCommand command = new SqlCommand(ProcedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    command.CommandText = ProcedureName; 
                    command.Parameters.AddWithValue("@PolicyTypeCommissionID", PolicyTypeCommissionID);
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@CommissionRate", CommissionRate);
                    command.Parameters.AddWithValue("@PolicyPremiumID", PolicyPremiumID); 
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        public DataTable GetLatest()
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Commissions_GetLatest";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable Search(int CurrencyID,int StartMonth, int EndMonth, int Year)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Commissions_GetByYearMonth";
            cmd.Parameters.AddWithValue("@CurrencyID", CurrencyID);
            cmd.Parameters.AddWithValue("@StartMonth", StartMonth);
            cmd.Parameters.AddWithValue("@EndMonth", EndMonth);
            cmd.Parameters.AddWithValue("@Year", Year);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public CommissionSearchHeader GetSearchHeader(int IntermediaryID, int CurrencyID, int StartMonth, int EndMonth, int Year)
        {
            CommissionSearchHeader commissionSearchHeader = new();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Commissions_GetSearchHeader";                
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IntermediaryID", IntermediaryID);
                    command.Parameters.AddWithValue("@CurrencyID", CurrencyID);
                    command.Parameters.AddWithValue("@StartMonth", StartMonth);
                    command.Parameters.AddWithValue("@EndMonth", EndMonth);
                    command.Parameters.AddWithValue("@Year", Year);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            commissionSearchHeader.AgentCode = reader["AgentCode"].ToString();
                            commissionSearchHeader.AgentName = reader["AgentName"].ToString();
                            commissionSearchHeader.IntermediaryCommission = (decimal)reader["IntermediaryCommission"];
                            commissionSearchHeader.OverridingCommission = (decimal)reader["OverridingCommission"];
                        }
                    }
                }
            }
            return commissionSearchHeader;
        }
        public CommissionSearchHeader GetSearchHeader(int CurrencyID, int StartMonth, int EndMonth, int Year)
        {
            CommissionSearchHeader commissionSearchHeader = new();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Commissions_GetSearchHeaderAll";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    command.Parameters.AddWithValue("@CurrencyID", CurrencyID);
                    command.Parameters.AddWithValue("@StartMonth", StartMonth);
                    command.Parameters.AddWithValue("@EndMonth", EndMonth);
                    command.Parameters.AddWithValue("@Year", Year);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        { 
                            commissionSearchHeader.IntermediaryCommission = (decimal)reader["IntermediaryCommission"];
                            commissionSearchHeader.OverridingCommission = (decimal)reader["OverridingCommission"];
                        }
                    }
                }
            }
            return commissionSearchHeader;
        }
        public DataTable Search(int IntermediaryID, int CurrencyID, int StartMonth, int EndMonth, int Year)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Commissions_Search";
            cmd.Parameters.AddWithValue("@IntermediaryID", IntermediaryID);
            cmd.Parameters.AddWithValue("@CurrencyID", CurrencyID);
            cmd.Parameters.AddWithValue("@StartMonth", StartMonth);
            cmd.Parameters.AddWithValue("@EndMonth", EndMonth);
            cmd.Parameters.AddWithValue("@Year", Year);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetPayments()
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "IntermediaryCommissionPayments_Get"; 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void DeletePolicyCommission(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM PolicyCommissions WHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        private PolicyCommission MapReaderToPolicyCommission(SqlDataReader reader)
        {
            return new PolicyCommission
            {
                ID = (int)reader["ID"],
                PolicyTypeCommissionsID = (int)reader["PolicyTypeCommissionsID"],
                PolicyPremiumLineID = reader["PolicyPremiumLineID"] == DBNull.Value ? null : (int?)reader["PolicyPremiumLineID"],
                IntermediaryID = (int)reader["IntermediaryID"],
                Commission = (decimal)reader["Commission"],
                CPPStarts = (int)reader["CPPStarts"],
                CPPEnds = (int)reader["CPPEnds"],
                StatusID = (int)reader["StatusID"],
                StatusDate = (DateTime)reader["StatusDate"],
                AddedOn = (DateTime)reader["AddedOn"],
                AddedBy = reader["AddedBy"].ToString()
            };
        }

        private void MapPolicyCommissionToParameters(PolicyCommission policyCommission, SqlCommand command)
        {
            command.Parameters.AddWithValue("@PolicyTypeCommissionsID", policyCommission.PolicyTypeCommissionsID);
            command.Parameters.AddWithValue("@PolicyPremiumLineID", policyCommission.PolicyPremiumLineID ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IntermediaryID", policyCommission.IntermediaryID);
            command.Parameters.AddWithValue("@Commission", policyCommission.Commission);
            command.Parameters.AddWithValue("@CPPStarts", policyCommission.CPPStarts);
            command.Parameters.AddWithValue("@CPPEnds", policyCommission.CPPEnds);
            command.Parameters.AddWithValue("@StatusID", policyCommission.StatusID);
            //command.Parameters.AddWithValue("@StatusDate", policyCommission.StatusDate);
            //command.Parameters.AddWithValue("@AddedOn", policyCommission.AddedOn);
            command.Parameters.AddWithValue("@AddedBy", policyCommission.AddedBy);
        }
    }
} 
