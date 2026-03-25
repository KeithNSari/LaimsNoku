using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Investments;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace LAIMS.Repositories.Premiums
{
    public class PolicyBeneficiaryRepository: IPolicyBeneficiaryRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PolicyBeneficiaryRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
		public int CountPrincipalMembers(Guid PolicyID)
		{
			var Database = _configuration.GetConnectionString("DefaultConnection");
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "DECLARE @Count int=0;SELECT @Count=Count(*) FROM [dbo].[PolicyBeneficiaries] WHERE [HeaderID]=@HeaderID AND [LIRole]=1 AND [Archived]=0; SELECT @Count;";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@HeaderID", PolicyID);  
					return Convert.ToInt32(command.ExecuteScalar());
				}
			}
		}
        public int CountRelationship(int RelationshipID, Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0;SELECT @Count=Count(*) FROM [dbo].[PolicyBeneficiaries] WHERE ([HeaderID]=@PolicyID) AND ([RelationshipID]=@RelationshipID) AND ([Archived]=0); SELECT @Count;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@RelationshipID", RelationshipID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int GetBeneficiaryID(int MemberID, Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @ID int=0; SELECT @ID=[ID] FROM [dbo].[PolicyBeneficiaries] WHERE ([Archived]=0) AND [HeaderID]=@HeaderID AND [MemberID]=@MemberID; SELECT @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HeaderID", PolicyID);
                    command.Parameters.AddWithValue("@MemberID", MemberID); 
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void UpdateBeneficiaryStatus(int PolicyBeneficiaryID, int Beneficiary)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[PolicyBeneficiaries] SET [Beneficiary]=@Beneficiary WHERE ID=@PolicyBeneficiaryID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PolicyBeneficiaryID", PolicyBeneficiaryID);
                    command.Parameters.AddWithValue("@Beneficiary", Beneficiary);
                    command.ExecuteNonQuery();
                }
            }
        }
        public int InsertPolicyBeneficiary(PolicyBeneficiary beneficiary)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @ID int=0; SELECT @ID=[ID] FROM [dbo].[PolicyBeneficiaries] WHERE ([Archived]=0) AND [HeaderID]=@HeaderID AND [MemberID]=@MemberID; IF(@ID=0) BEGIN INSERT INTO PolicyBeneficiaries (HeaderID, MemberID, RelationshipID, LIRole,Insured,Beneficiary,IDType,RiskGroupID, AddedOn, AddedBy) VALUES (@HeaderID, @MemberID, @RelationshipID, @LIRole,@Insured,@Beneficiary,@IDType,@RiskGroupID, @AddedOn, @AddedBy); SELECT @ID=SCOPE_IDENTITY() END; SELECT @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HeaderID", beneficiary.HeaderID);
                    command.Parameters.AddWithValue("@MemberID", beneficiary.MemberID);
                    command.Parameters.AddWithValue("@RelationshipID", beneficiary.RelationshipID);
                    command.Parameters.AddWithValue("@IDType", beneficiary.IDType);
                    command.Parameters.AddWithValue("@LIRole", beneficiary.LIRole);
                    command.Parameters.AddWithValue("@Insured", beneficiary.Insured);
                    command.Parameters.AddWithValue("@RiskGroupID", beneficiary.RiskGroupID);
                    command.Parameters.AddWithValue("@Beneficiary", beneficiary.Beneficiary);
                    command.Parameters.AddWithValue("@AddedOn", beneficiary.AddedOn ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", beneficiary.AddedBy ?? (object)DBNull.Value);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int InsertPolicyBeneficiaryStaging(PolicyBeneficiary beneficiary, Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyBeneficiariesStaging_Upsert";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@HeaderID", beneficiary.HeaderID);
                    command.Parameters.AddWithValue("@MemberID", beneficiary.MemberID);
                    command.Parameters.AddWithValue("@RelationshipID", beneficiary.RelationshipID);
                    command.Parameters.AddWithValue("@IDType", beneficiary.IDType);
                    command.Parameters.AddWithValue("@Beneficiary", beneficiary.Beneficiary);
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.Parameters.AddWithValue("@AddedOn", beneficiary.AddedOn ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", beneficiary.AddedBy ?? (object)DBNull.Value);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public (int? MinAgeAtEntry, int? MaxAgeAtEntry) GetPolicyTypeRelationshipAgeLimits(Guid policyTypeID, int relationshipID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("PolicyTypeRelationships_GetAgeLimits", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyTypeID", policyTypeID);
                    command.Parameters.AddWithValue("@RelationshipID", relationshipID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int? minAge = reader["MinAgeAtEntry"] == DBNull.Value ? null : Convert.ToInt32(reader["MinAgeAtEntry"]);
                            int? maxAge = reader["MaxAgeAtEntry"] == DBNull.Value ? null : Convert.ToInt32(reader["MaxAgeAtEntry"]);
                            return (minAge, maxAge);
                        }
                    }
                }
            }
            return (null, null);
        }
        public int ProposeAdditionalLifeAssured(PolicyBeneficiary beneficiary, Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @ID int=0; SELECT @ID=[ID] FROM [dbo].[PolicyBeneficiaries] WHERE ([Archived]=0) AND [HeaderID]=@HeaderID AND [MemberID]=@MemberID; IF(@ID=0) BEGIN INSERT INTO PolicyBeneficiaries (RequestID, HeaderID, MemberID, RelationshipID,Beneficiary,IDType,AddedOn, AddedBy,Approved) VALUES (@RequestID, @HeaderID, @MemberID, @RelationshipID,@Beneficiary,@IDType,@AddedOn, @AddedBy,0); SELECT @ID=SCOPE_IDENTITY() END; UPDATE PolicyBeneficiaries SET ProposeLIRole=2 WHERE [ID]=@ID; SELECT @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HeaderID", beneficiary.HeaderID);
                    command.Parameters.AddWithValue("@MemberID", beneficiary.MemberID);
                    command.Parameters.AddWithValue("@RelationshipID", beneficiary.RelationshipID);
                    command.Parameters.AddWithValue("@IDType", beneficiary.IDType);
                    command.Parameters.AddWithValue("@LIRole", beneficiary.LIRole);
                    command.Parameters.AddWithValue("@Insured", beneficiary.Insured);
                    command.Parameters.AddWithValue("@RiskGroupID", beneficiary.RiskGroupID);
                    command.Parameters.AddWithValue("@Beneficiary", beneficiary.Beneficiary);
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.Parameters.AddWithValue("@AddedOn", beneficiary.AddedOn ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", beneficiary.AddedBy ?? (object)DBNull.Value);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int GetRiskGroup(List<int> Parameters)
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.AppendLine("DECLARE @RiskGroupID int =-1; SELECT @RiskGroupID=[GroupID] FROM [RiskGroupParameters] WHERE [RiskGroupParameters].[ParameterID] IN ({0}) GROUP BY [GroupID] HAVING COUNT(DISTINCT [RiskGroupParameters].[ParameterID]) = {1}; SELECT @RiskGroupID;");
            string parameterValues = string.Join(",", Parameters);
            int NoOfParameters = Parameters.Count();
            string sql = string.Format(queryBuilder.ToString(), parameterValues, NoOfParameters);
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void UpdateRiskGroup(int PolicyBeneficiaryID,int RiskGroup)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[PolicyBeneficiaries] SET [RiskGroupID]=@RiskGroup WHERE [ID]=@ID AND [RiskGroupID]!=@RiskGroup";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", PolicyBeneficiaryID);
                    command.Parameters.AddWithValue("@RiskGroup", RiskGroup);
                    command.ExecuteNonQuery();
                }
            }
        }
		public int GetPolicyBeneficiaryRiskGroup(int PolicyBeneficiaryID)
		{
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "DECLARE @RiskGroup int=0; SELECT @RiskGroup=[RiskGroupID] FROM [dbo].[PolicyBeneficiaries] WHERE [ID]=@ID; SELECT @RiskGroup";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@ID", PolicyBeneficiaryID); 
                    return Convert.ToInt32(command.ExecuteScalar());
				}
			}
		}

		public PolicyBeneficiary GetPolicyBeneficiary(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM PolicyBeneficiaries WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToPolicyBeneficiary(reader);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }
        public int GetMemberID(int PolicyBeneficiaryID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @MemberID int =0; SELECT @MemberID=ISNULL([MemberID],0) FROM  [dbo].[PolicyBeneficiaries] WHERE [ID]=@PBID; SELECT @MemberID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PBID", PolicyBeneficiaryID);
                    return Convert.ToInt32( command.ExecuteScalar());
                }
            }
        }
        public void UpdatePolicyBeneficiary(PolicyBeneficiary beneficiary)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE PolicyBeneficiaries SET HeaderID = @HeaderID, MemberID = @MemberID, RelationshipID = @RelationshipID, LIRole = @LIRole, AddedOn = @AddedOn, AddedBy = @AddedBy WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", beneficiary.ID);
                    command.Parameters.AddWithValue("@HeaderID", beneficiary.HeaderID);
                    command.Parameters.AddWithValue("@MemberID", beneficiary.MemberID);
                    command.Parameters.AddWithValue("@RelationshipID", beneficiary.RelationshipID);
                    command.Parameters.AddWithValue("@LIRole", beneficiary.LIRole);
                    command.Parameters.AddWithValue("@AddedOn", beneficiary.AddedOn ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", beneficiary.AddedBy ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeletePolicyBeneficiary(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM PolicyBeneficiaries WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    command.ExecuteNonQuery();
                }
            }
        }
        //public DataTable GetBeneficiaryList(Guid PolicyID)
        //{
        //    var Database = _configuration.GetConnectionString("DefaultConnection");
        //    DataTable DT = new DataTable();
        //    SqlConnection connection = new SqlConnection();
        //    connection.ConnectionString = Database;
        //    SqlCommand cmd = connection.CreateCommand();
        //    cmd.CommandType = CommandType.Text;
        //    cmd.CommandText = "SELECT [PolicyBeneficiaries].[ID],[LIRoles].[Role],[MemberID],[Name3] + ' ' + IsNull([Name2] + ' ','') + [Name1] AS [FullName],Convert(varchar,[DOB],103) As [DOB],[Relationship],CASE [PolicyBeneficiaries].[IDType] WHEN 1 THEN [Members].[NationalID] WHEN 2 THEN [Members].[BirthCertificate] WHEN 3 THEN [Members].[Passport] END AS [IDDocument], [IDTypes].[IDType],[PolicyBeneficiaries].[HeaderID] AS [PolicyID],[Members].[UID] FROM [dbo].[PolicyBeneficiaries] LEFT JOIN [Members] ON [PolicyBeneficiaries].[MemberID]=[Members].[ID] LEFT JOIN [Relationships] ON [Relationships].[ID]=[RelationshipID] LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[LIRole] LEFT JOIN [IDTypes] ON [IDTypes].[TypeID]=[PolicyBeneficiaries].[IDType] WHERE [HeaderID]=@PolicyID AND [PolicyBeneficiaries].[Archived]=0 AND [Beneficiary]=1 ORDER BY [LIRoles].[ID] ASC, [Name3] ASC, [Name2] ASC, [NAME1] ASC";
        //    cmd.Parameters.AddWithValue("PolicyID", PolicyID);
        //    SqlDataAdapter da = new SqlDataAdapter(cmd);
        //    da.Fill(DT);
        //    return DT;
        //}
        public DataTable GetBeneficiaryList(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [PolicyBeneficiaries].[ID],[LIRoles].[Role],[MemberID],[Name3] + ' ' + IsNull([Name2] + ' ','') + [Name1] AS [FullName],Convert(varchar,[DOB],106) As [DOB],[Relationship],CASE [PolicyBeneficiaries].[IDType] WHEN 1 THEN [Members].[NationalID] WHEN 2 THEN [Members].[BirthCertificate] WHEN 3 THEN [Members].[Passport] END AS [IDDocument], [IDTypes].[IDType],[PolicyBeneficiaries].[HeaderID] AS [PolicyID],[Members].[UID] FROM [dbo].[PolicyBeneficiaries] LEFT JOIN [Members] ON [PolicyBeneficiaries].[MemberID]=[Members].[ID] LEFT JOIN [Relationships] ON [Relationships].[ID]=[RelationshipID] LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[LIRole] LEFT JOIN [IDTypes] ON [IDTypes].[TypeID]=[PolicyBeneficiaries].[IDType] WHERE [HeaderID]=@PolicyID AND [PolicyBeneficiaries].[Archived]=0 AND [Beneficiary]=1 ORDER BY [LIRoles].[ID] ASC, [Name3] ASC, [Name2] ASC, [NAME1] ASC";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID); 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetStagingBeneficiaryList(Guid PolicyID, Guid RequestID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [PolicyBeneficiariesStaging].[ID],[LIRoles].[Role],[MemberID],[Name3] + ' ' + IsNull([Name2] + ' ','') + [Name1] AS [FullName],Convert(varchar,[DOB],106) As [DOB],[Relationship],CASE [PolicyBeneficiariesStaging].[IDType] WHEN 1 THEN [Members].[NationalID] WHEN 2 THEN [Members].[BirthCertificate] WHEN 3 THEN [Members].[Passport] END AS [IDDocument], [IDTypes].[IDType],[PolicyBeneficiariesStaging].[HeaderID] AS [PolicyID],[Members].[UID],CASE [PolicyBeneficiariesStaging].[Beneficiary] WHEN 1 THEN 'No' WHEN 0 THEN 'Yes' END AS [Archived] FROM [dbo].[PolicyBeneficiariesStaging] LEFT JOIN [Members] ON [PolicyBeneficiariesStaging].[MemberID]=[Members].[ID] LEFT JOIN [Relationships] ON [Relationships].[ID]=[RelationshipID] LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[LIRole] LEFT JOIN [IDTypes] ON [IDTypes].[TypeID]=[PolicyBeneficiariesStaging].[IDType] WHERE [RequestID]=@RequestID AND [HeaderID]=@PolicyID AND [Beneficiary]=1 ORDER BY [LIRoles].[ID] ASC, [Name3] ASC, [Name2] ASC, [NAME1] ASC";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            cmd.Parameters.AddWithValue("RequestID", RequestID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void CopyBeneficiaryList(Guid PolicyID, Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyBeneficiaries_Copy";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;  
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateBeneficiaryListFromCopy(Guid PolicyID, Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyBeneficiaries_UpdateFromCopy";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetAdditionalLifeAssured(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [PolicyBeneficiaries].[ID],[LIRoles].[Role],[MemberID],[Name3] + ' ' + IsNull([Name2] + ' ','') + [Name1] AS [FullName],Convert(varchar,[DOB],106) As [DOB],[Relationship],CASE [PolicyBeneficiaries].[IDType] WHEN 1 THEN [Members].[NationalID] WHEN 2 THEN [Members].[BirthCertificate] WHEN 3 THEN [Members].[Passport] END AS [IDDocument], [IDTypes].[IDType],[PolicyBeneficiaries].[HeaderID] AS [PolicyID],[Members].[UID],[PolicyBeneficiaries].[ProposeToArchive] FROM [dbo].[PolicyBeneficiaries] LEFT JOIN [Members] ON [PolicyBeneficiaries].[MemberID]=[Members].[ID] LEFT JOIN [Relationships] ON [Relationships].[ID]=[RelationshipID] LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[LIRole] LEFT JOIN [IDTypes] ON [IDTypes].[TypeID]=[PolicyBeneficiaries].[IDType] WHERE [HeaderID]=@PolicyID AND [PolicyBeneficiaries].[Archived]=0 AND [LIRole]=2 ORDER BY [LIRoles].[ID] ASC, [Name3] ASC, [Name2] ASC, [NAME1] ASC";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetAdditionalLifeAssured(Guid PolicyID, Guid RequestID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [PolicyBeneficiaries].[ID],[LIRoles].[Role],[MemberID],[Name3] + ' ' + IsNull([Name2] + ' ','') + [Name1] AS [FullName],Convert(varchar,[DOB],106) As [DOB],[Relationship],CASE [PolicyBeneficiaries].[IDType] WHEN 1 THEN [Members].[NationalID] WHEN 2 THEN [Members].[BirthCertificate] WHEN 3 THEN [Members].[Passport] END AS [IDDocument], [IDTypes].[IDType],[PolicyBeneficiaries].[HeaderID] AS [PolicyID],[Members].[UID],[PolicyBeneficiaries].[ProposeToArchive],[PolicyBeneficiaries].[Approved] FROM [dbo].[PolicyBeneficiaries] LEFT JOIN [Members] ON [PolicyBeneficiaries].[MemberID]=[Members].[ID] LEFT JOIN [Relationships] ON [Relationships].[ID]=[RelationshipID] LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[LIRole] LEFT JOIN [IDTypes] ON [IDTypes].[TypeID]=[PolicyBeneficiaries].[IDType] WHERE [HeaderID]=@PolicyID AND [PolicyBeneficiaries].[Archived]=0 AND [LIRole]=2 OR (ProposeLIRole=2 AND [PolicyBeneficiaries].[RequestID]=@RequestID) AND [PolicyBeneficiaries].[Archived]=0 ORDER BY [LIRoles].[ID] ASC, [Name3] ASC, [Name2] ASC, [NAME1] ASC";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            cmd.Parameters.AddWithValue("RequestID", RequestID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetMainLifeAssured(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [PolicyBeneficiaries].[ID],[LIRoles].[Role],[MemberID],[Name3] + ' ' + IsNull([Name2] + ' ','') + [Name1] AS [FullName],Convert(varchar,[DOB],106) As [DOB],[Relationship],CASE [PolicyBeneficiaries].[IDType] WHEN 1 THEN [Members].[NationalID] WHEN 2 THEN [Members].[BirthCertificate] WHEN 3 THEN [Members].[Passport] END AS [IDDocument], [IDTypes].[IDType],[PolicyBeneficiaries].[HeaderID] AS [PolicyID],[Members].[UID] FROM [dbo].[PolicyBeneficiaries] LEFT JOIN [Members] ON [PolicyBeneficiaries].[MemberID]=[Members].[ID] LEFT JOIN [Relationships] ON [Relationships].[ID]=[RelationshipID] LEFT JOIN [LIRoles] ON [LIRoles].[ID]=[LIRole] LEFT JOIN [IDTypes] ON [IDTypes].[TypeID]=[PolicyBeneficiaries].[IDType] WHERE [HeaderID]=@PolicyID AND [PolicyBeneficiaries].[Archived]=0 AND [LIRole]=1 ORDER BY [LIRoles].[ID] ASC, [Name3] ASC, [Name2] ASC, [NAME1] ASC";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetBeneficiaryShares(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PBLSplits_Get";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }        
        public DataTable GetStagingBeneficiaryShares(Guid PolicyID, Guid RequestID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PBLSplitsStaging_Get";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            cmd.Parameters.AddWithValue("RequestID", RequestID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public decimal GetSplitTotal(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @SUM decimal(18,2)=0; SELECT @SUM=IsNULL(SUM([SplitPercentage]),0) FROM [dbo].[PBLSplits] WHERE [PolicyID]=@PolicyID AND [Archived]=0; SELECT @SUM";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (connection)
                    {
                        command.Parameters.AddWithValue("PolicyID", PolicyID);
                        return Convert.ToDecimal(command.ExecuteScalar());
                    }
                }
            }
        }
        public DataTable GetBeneficiaryFullDetails(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [Members].[ID],[Members].[UID],[MemberNo],[IsOrganisation],CASE [IsOrganisation] WHEN 1 Then 'Organisation' WHEN 0 THEN 'Person' END  AS [Type],[Name1],[Name2],[Name3],[GenderID],[Genders].[Name] AS [Gender],[Members].[TitleID],[Titles].[Title],[Members].[MaritalStatusID],[MaritalStatus],[Members].[CountryID],A.[Country],[DOB],[BirthCountryID],B.[Country] AS [BirthCountry],[PlaceOfBirth],[NationalID],[BirthCertificate],[Passport],[PolicyBeneficiaries].[AddedOn],[PolicyBeneficiaries].[AddedBy] FROM [dbo].[PolicyBeneficiaries] LEFT JOIN [Members] On [PolicyBeneficiaries].[MemberID]=[Members].[ID] LEFT JOIN [Genders] ON [Genders].[Id]=[Members].[GenderID] LEFT JOIN [Titles] ON [Titles].[TitleID]=[Members].[TitleID] LEFT JOIN [Countries] A ON A.[CountryID]=[Members].[CountryID] LEFT JOIN [Countries] B On B.[CountryID]=[Members].[BirthCountryID] LEFT JOIN [MaritalStatii] On [MaritalStatii].[MaritalStatusID]=[Members].[MaritalStatusID] WHERE [PolicyBeneficiaries].[HeaderID]=@PolicyID AND [PolicyBeneficiaries].[Archived]=0 ";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetRequiredPolicyDocumentsList(Guid PolicyID, int TestedBusiness)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            if (TestedBusiness == 1)
            {
                cmd.CommandText = "PolicyBeneficiariesLines_TBDocumentsMenu";
            }
            else
            {
                cmd.CommandText = "PolicyBeneficiariesLines_UTBDocumentsMenu";
            }
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public List<PBLSplit> GetAllPBLSplits(Guid PolicyID)
        {
            List<PBLSplit> pblSplits = new List<PBLSplit>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "SELECT ID, PBLID, PolicyBeneficiaryID, SplitPercentage, AddedOn, AddedBy FROM PBLSplits LEFT JOIN [PolicyBeneficiaries] ON [PolicyBeneficiaries].[ID]=PBLSplit.[PolicyBeneficiaryID] WHERE PBLSplit.Archived=0 AND [PolicyBeneficiaries].Archived=0 AND [PolicyBeneficiaries].[HeaderID]=@PolicyID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("PolicyID", PolicyID);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PBLSplit pblSplit = new PBLSplit
                        {
                            ID = (int)reader["ID"],
                            PBLID = reader["PBLID"] != DBNull.Value ? (int)reader["PBLID"] : (int?)null,
                            PolicyBeneficiaryID = (int)reader["PolicyBeneficiaryID"],
                            SplitPercentage = (decimal)reader["SplitPercentage"],
                            AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime)reader["AddedOn"] : (DateTime?)null,
                            AddedBy = reader["AddedBy"] != DBNull.Value ? (string)reader["AddedBy"] : null
                        };
                        pblSplits.Add(pblSplit);
                    }
                }
            }

            return pblSplits;
        }
        public void AddPBLSplit(PBLSplit pblSplit)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "INSERT INTO PBLSplits (BatchID,PolicyID,PBLID, PolicyBeneficiaryID, SplitPercentage, AddedOn, AddedBy) VALUES (@BatchID,@PolicyID,@PBLID, @PolicyBeneficiaryID, @SplitPercentage, @AddedOn, @AddedBy)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BatchID", pblSplit.BatchID);
                command.Parameters.AddWithValue ("@PolicyID", pblSplit.PolicyID);
                command.Parameters.AddWithValue("@PBLID", pblSplit.PBLID ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PolicyBeneficiaryID", pblSplit.PolicyBeneficiaryID);
                command.Parameters.AddWithValue("@SplitPercentage", pblSplit.SplitPercentage);
                command.Parameters.AddWithValue("@AddedOn", pblSplit.AddedOn ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AddedBy", pblSplit.AddedBy ?? (object)DBNull.Value);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public void AddPBLSplitStaging(PBLSplit pblSplit, Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "INSERT INTO PBLSplitsStaging (BatchID,RequestID,PolicyID,PBLID, PolicyBeneficiaryID, SplitPercentage, AddedOn, AddedBy) VALUES (@BatchID,@RequestID,@PolicyID,@PBLID, @PolicyBeneficiaryID, @SplitPercentage, @AddedOn, @AddedBy)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BatchID", pblSplit.BatchID);
                command.Parameters.AddWithValue("@PolicyID", pblSplit.PolicyID);
                command.Parameters.AddWithValue("@PBLID", pblSplit.PBLID ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PolicyBeneficiaryID", pblSplit.PolicyBeneficiaryID);
                command.Parameters.AddWithValue("@SplitPercentage", pblSplit.SplitPercentage);
                command.Parameters.AddWithValue("@RequestID", RequestID);
                command.Parameters.AddWithValue("@AddedOn", pblSplit.AddedOn ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AddedBy", pblSplit.AddedBy ?? (object)DBNull.Value);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public void UpdatePBLSplitFromStaging(Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "PBLSplits_UpdateFromCopy";
                SqlCommand command = new SqlCommand(query, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@RequestID", RequestID); 
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public void ArchivePBLSplit(Guid PolicyID, string ArchivedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "UPDATE PBLSplits SET Archived=1, ArchivedOn=GetDate(), ArchivedBy=@ArchivedBy WHERE PolicyID=@PolicyID AND Archived=0";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PolicyID", PolicyID);
                command.Parameters.AddWithValue("@ArchivedBy", ArchivedBy); 
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public void ArchivePBLSplitStaging(Guid PolicyID, string ArchivedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "UPDATE PBLSplitsStaging SET Archived=1, ArchivedOn=GetDate(), ArchivedBy=@ArchivedBy WHERE PolicyID=@PolicyID AND Archived=0";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PolicyID", PolicyID);
                command.Parameters.AddWithValue("@ArchivedBy", ArchivedBy);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        private PolicyBeneficiary MapReaderToPolicyBeneficiary(SqlDataReader reader)
        {
            return new PolicyBeneficiary
            {
                ID = (int)reader["ID"],
                HeaderID = (Guid)reader["HeaderID"],
                MemberID = (int)reader["MemberID"],
                RelationshipID = (int)reader["RelationshipID"],
                LIRole = (int)reader["LIRole"],
                IDType = (int)reader["IDType"],
                AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime)reader["AddedOn"] : (DateTime?)null,
                AddedBy = reader["AddedBy"] != DBNull.Value ? (string)reader["AddedBy"] : null
            };
        }
        public void ProposeBeneficiaryArchive(int PolicyBeneficiaryID, Guid RequestID, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyBeneficiaries_ProposeToArchive";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyBeneficiaryID", PolicyBeneficiaryID);
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
