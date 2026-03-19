using LAIMS.Models.LifeProducts;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using LAIMS.Interfaces.Lifeproducts;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace LAIMS.Repositories.Lifeproducts
{
    public class PolicyTypeRepository: IPolicyTypeRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public PolicyTypeRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

        }
        public int CheckExistence(PolicyType policyType)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM PolicyTypes Where [Name]=@Name Or [ID]=@ID And DELETED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", policyType.Name);
                    command.Parameters.AddWithValue("@ID", policyType.ID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int CheckExistenceOther(PolicyType policyType)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM PolicyTypes Where [Name]=@Name AND ([ID]!=@ID) And DELETED=0; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", policyType.Name);
                    command.Parameters.AddWithValue("@ID", policyType.ID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void InsertPolicyType(PolicyType policyType)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "INSERT INTO PolicyTypes (ID, Name, OpenForNewBusiness, CurrencyID, MinimumTerm, MaximumTerm, " +
                               "AllowDeferingOfMaturityDate, DefermentNoticePeriod, LifeAssuredMinAge, LifeAssuredMaxAge, " +
                               "ProposerMinAge, ProposerMaxAge, PremiumPayerMinAge, PremiumPayerMaxAge, [Current], AddedOn, AddedBy) " +
                               "VALUES (@ID, @Name, @OpenForNewBusiness, @CurrencyID, @MinimumTerm, @MaximumTerm, " +
                               "@AllowDeferingOfMaturityDate, @DefermentNoticePeriod, @LifeAssuredMinAge, @LifeAssuredMaxAge, " +
                               "@ProposerMinAge, @ProposerMaxAge, @PremiumPayerMinAge, @PremiumPayerMaxAge, @Current, @AddedOn, @AddedBy)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, policyType);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdatePolicyType(PolicyType policyType)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE PolicyTypes SET Name = @Name, OpenForNewBusiness = @OpenForNewBusiness, " +
                               "CurrencyID = @CurrencyID, MinimumTerm = @MinimumTerm, MaximumTerm = @MaximumTerm, " +
                               "AllowDeferingOfMaturityDate = @AllowDeferingOfMaturityDate, " +
                               "DefermentNoticePeriod = @DefermentNoticePeriod, LifeAssuredMinAge = @LifeAssuredMinAge, " +
                               "LifeAssuredMaxAge = @LifeAssuredMaxAge, ProposerMinAge = @ProposerMinAge, " +
                               "ProposerMaxAge = @ProposerMaxAge, PremiumPayerMinAge = @PremiumPayerMinAge, " +
                               "PremiumPayerMaxAge = @PremiumPayerMaxAge " +
                               "WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddParameters(command, policyType);
                   // command.Parameters.AddWithValue("@ID", policyType.ID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeletePolicyType(int entryNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM PolicyTypes WHERE EntryNo = @EntryNo";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EntryNo", entryNo);
                    command.ExecuteNonQuery();
                }
            }
        }

        public PolicyType GetPolicyType(int entryNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM PolicyTypes WHERE EntryNo = @EntryNo";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EntryNo", entryNo);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToPolicyType(reader);
                        }
                    }
                }
            }

            return null;
        }
        public PolicyType GetPolicyType(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT [PolicyTypes].[EntryNo],[PolicyTypes].[ID],[OpenForNewBusiness],[PolicyTypes].[Name],[OpenForNewBusiness],CASE [OpenForNewBusiness] When 0 Then 'No' When 1 Then 'Yes' End As [OpenFornewBusinessDesc],[PolicyTypes].[CurrencyID],[Currencies].[Name] As [CurrencyName],[MinimumTerm],[MaximumTerm],Case [AllowDeferingOfMaturityDate] When 0 Then 'No' When 1 Then 'Yes' End As [AllowDeferingOfMaturityDate],CASE [AllowDeferingOfMaturityDate] WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END AS [AllowDeferingOfMaturityDateDesc],[DefermentNoticePeriod],[LifeAssuredMinAge],[LifeAssuredMaxAge],[ProposerMinAge],[ProposerMaxAge],[PremiumPayerMinAge],[PremiumPayerMaxAge],[PolicyTypes].[AddedOn],[AspNetUsers].[UserName] AS [AddedBy],[PolicyTypes].[Current] FROM  [dbo].[PolicyTypes] LEFT JOIN [Currencies] On [Currencies].[ID]=[PolicyTypes].[CurrencyID] LEFT JOIN [AspNetUsers] On [AspNetUsers].[Id]=[PolicyTypes].[AddedBy] Where [PolicyTypes].[Current]=1 And [PolicyTypes].[Archived]=0 And [PolicyTypes].[Deleted]=0 AND [PolicyTypes].[ID]=@ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToPolicyType(reader);
                        }
                    }
                }
            }

            return null;
        }
        public PolicyType GetPolicyTypeFull(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM  [dbo].[PolicyTypes] Where [PolicyTypes].[Current]=1 And [PolicyTypes].[Archived]=0 And [PolicyTypes].[Deleted]=0 AND [PolicyTypes].[ID]=@ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return DirectMapToPolicyType(reader);
                        }
                    }
                }
            }

            return null;
        }
        public string? GetPolicyTypeName(Guid ID)
        {  
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection connection = new(Database);
            connection.Open();
            string query = "SELECT [Name] FROM  [dbo].[PolicyTypes] Where [PolicyTypes].[Current]=1 And [PolicyTypes].[Archived]=0 And [PolicyTypes].[Deleted]=0 AND [PolicyTypes].[ID]=@ID;";
            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@ID", ID);
            return command.ExecuteScalar() as string;
        }
        public bool IsLife(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection connection = new(Database);
            connection.Open();
            string query = "SELECT [IsLife] FROM  [dbo].[PolicyTypes] Where [PolicyTypes].[Current]=1 And [PolicyTypes].[Archived]=0 And [PolicyTypes].[Deleted]=0 AND [PolicyTypes].[ID]=@ID;";
            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@ID", ID);
            return Convert.ToBoolean(command.ExecuteScalar());
        }
        public bool IsPureInvestmentProduct(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection connection = new(Database);
            connection.Open();
            string query = "Policy_CheckIfMainProductIsPureInvestment";
            using SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@PolicyTypeID", ID);
            return Convert.ToBoolean(command.ExecuteScalar());
        }
        public bool HasInvestmentProduct(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection connection = new(Database);
            connection.Open();
            string query = "Policy_CheckInvestment";
            using SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@PolicyTypeID", ID);
            return Convert.ToBoolean(command.ExecuteScalar());
        }
        public bool HasRiskProduct(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection connection = new(Database);
            connection.Open();
            string query = "Policy_CheckRiskProduct";
            using SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@PolicyTypeID", ID);
            return Convert.ToBoolean(command.ExecuteScalar());
        }
        public bool AdditionalLifeAssuredAllowedCheck(Guid PolicyTypeID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "NewBusiness_GetAllowAdditionalLifeAssured";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyTypeID", PolicyTypeID);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public bool CheckLIRole(Guid ID, int LIRoleID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection connection = new(Database);
            connection.Open();
            string query = "PolicyType_CheckLIRole";
            using SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@PolicyTypeID", ID);
            command.Parameters.AddWithValue("@LIRoleID ", LIRoleID);
            return Convert.ToBoolean(command.ExecuteScalar());
        }

        public int GetMinimumTerm(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection connection = new(Database);
            connection.Open();
            string query = "SELECT [MinimumTerm] FROM [dbo].[PolicyTypes] Where [PolicyTypes].[ID]=@ID;";
            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@ID", ID);
            return Convert.ToInt32(command.ExecuteScalar());
        }
        public int GetMaximumTerm(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection connection = new(Database);
            connection.Open();
            string query = "SELECT [MaximumTerm] FROM [dbo].[PolicyTypes] Where [PolicyTypes].[Current]=1 And [PolicyTypes].[Archived]=0 And [PolicyTypes].[Deleted]=0 AND [PolicyTypes].[ID]=@ID;";
            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@ID", ID);
            return Convert.ToInt32(command.ExecuteScalar());
        }
        public Guid GetID(string PolicyTypeName)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection connection = new(Database);
            connection.Open();
            string query = "DECLARE @ID uniqueidentifier='00000000-0000-0000-0000-000000000000'; SELECT @ID=[ID] FROM [dbo].[PolicyTypes] Where [Name]=@PolicyTypeName; SELECT @ID";
            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("PolicyTypeName", PolicyTypeName);
            return Guid.Parse(command.ExecuteScalar().ToString());
        }
        public DataTable Get(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT [PolicyTypes].[EntryNo],[PolicyTypes].[ID],[PolicyTypes].[Name],Case [OpenForNewBusiness] When 0 Then 'No' When 1 Then 'Yes' End As [OpenFornewBusiness],[Currencies].[Name] As [Currency],[MinimumTerm],[MaximumTerm],Case [AllowDeferingOfMaturityDate] When 0 Then 'No' When 1 Then 'Yes' End As [AllowDeferingOfMaturityDate],[DefermentNoticePeriod],[LifeAssuredMinAge],[LifeAssuredMaxAge],[ProposerMinAge],[ProposerMaxAge],[PremiumPayerMinAge],[PremiumPayerMaxAge],[PolicyTypes].[AddedOn],[AspNetUsers].[UserName] FROM  [dbo].[PolicyTypes] LEFT JOIN [Currencies] On [Currencies].[ID]=[PolicyTypes].[CurrencyID] LEFT JOIN [AspNetUsers] On [AspNetUsers].[Id]=[PolicyTypes].[AddedBy] Where [PolicyTypes].[Current]=1 And [PolicyTypes].[Archived]=0 And [PolicyTypes].[Deleted]=0 AND [PolicyTypes].[ID]=@ID";
            command.Parameters.AddWithValue("@ID", ID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public List<PolicyType> GetAllPolicyTypes()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<PolicyType> policyTypes = new List<PolicyType>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT [PolicyTypes].[EntryNo],[PolicyTypes].[ID],[OpenForNewBusiness],[PolicyTypes].[Name],[OpenForNewBusiness],CASE [OpenForNewBusiness] When 0 Then 'No' When 1 Then 'Yes' End As [OpenFornewBusinessDesc],[PolicyTypes].[CurrencyID],[Currencies].[Name] As [CurrencyName],[MinimumTerm],[MaximumTerm],Case [AllowDeferingOfMaturityDate] When 0 Then 'No' When 1 Then 'Yes' End As [AllowDeferingOfMaturityDate],CASE [AllowDeferingOfMaturityDate] WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END AS [AllowDeferingOfMaturityDateDesc],[DefermentNoticePeriod],[LifeAssuredMinAge],[LifeAssuredMaxAge],[ProposerMinAge],[ProposerMaxAge],[PremiumPayerMinAge],[PremiumPayerMaxAge],[PolicyTypes].[AddedOn],[AspNetUsers].[UserName] AS [AddedBy],[PolicyTypes].[Current] FROM  [dbo].[PolicyTypes] LEFT JOIN [Currencies] On [Currencies].[ID]=[PolicyTypes].[CurrencyID] LEFT JOIN [AspNetUsers] On [AspNetUsers].[Id]=[PolicyTypes].[AddedBy] Where [PolicyTypes].[Current]=1 And [PolicyTypes].[Archived]=0 And [PolicyTypes].[Deleted]=0 ORDER BY [PolicyTypes].[Name] ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            policyTypes.Add(MapToPolicyType(reader));
                        }
                    }
                }
            }

            return policyTypes;
        }
		public List<PolicyType> GetAll()
		{
			var Database = _configuration.GetConnectionString("DefaultConnection");
			List<PolicyType> policyTypes = new List<PolicyType>();

			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "PolicyTypes_GetAllInclusive";

				using (SqlCommand command = new SqlCommand(query, connection))
				{
                    command.CommandType = CommandType.StoredProcedure;
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							policyTypes.Add(MapToPolicyTypeShort(reader));
						}
					}
				}
			}

			return policyTypes;
		}
		public DataTable Get()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT [PolicyTypes].[EntryNo],[PolicyTypes].[ID],[PolicyTypes].[Name],Case [OpenForNewBusiness] When 0 Then 'No' When 1 Then 'Yes' End As [OpenFornewBusiness],[Currencies].[Name] As [Currency],[MinimumTerm],[MaximumTerm],Case [AllowDeferingOfMaturityDate] When 0 Then 'No' When 1 Then 'Yes' End As [AllowDeferingOfMaturityDate],[DefermentNoticePeriod],[LifeAssuredMinAge],[LifeAssuredMaxAge],[ProposerMinAge],[ProposerMaxAge],[PremiumPayerMinAge],[PremiumPayerMaxAge],[PolicyTypes].[AddedOn],[AspNetUsers].[UserName] FROM  [dbo].[PolicyTypes] LEFT JOIN [Currencies] On [Currencies].[ID]=[PolicyTypes].[CurrencyID] LEFT JOIN [AspNetUsers] On [AspNetUsers].[Id]=[PolicyTypes].[AddedBy] Where [PolicyTypes].[Current]=1 And [PolicyTypes].[Archived]=0 And [PolicyTypes].[Deleted]=0  Order By [PolicyTypes].[Name] Asc";
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetByDesignation(string UserID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PolicyTypes_GetByDesignation";
            command.Parameters.AddWithValue("@UserID", UserID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        private void AddParameters(SqlCommand command, PolicyType policyType)
        {
            command.Parameters.AddWithValue("@ID", policyType.ID);
            command.Parameters.AddWithValue("@Name", policyType.Name);
            command.Parameters.AddWithValue("@OpenForNewBusiness", policyType.OpenForNewBusiness);
            command.Parameters.AddWithValue("@CurrencyID", policyType.CurrencyID);
            command.Parameters.AddWithValue("@MinimumTerm", policyType.MinimumTerm);
            command.Parameters.AddWithValue("@MaximumTerm", policyType.MaximumTerm);
            command.Parameters.AddWithValue("@AllowDeferingOfMaturityDate", policyType.AllowDeferingOfMaturityDate);
            command.Parameters.AddWithValue("@DefermentNoticePeriod", policyType.DefermentNoticePeriod);
            command.Parameters.AddWithValue("@LifeAssuredMinAge", policyType.LifeAssuredMinAge);
            command.Parameters.AddWithValue("@LifeAssuredMaxAge", policyType.LifeAssuredMaxAge);
            command.Parameters.AddWithValue("@ProposerMinAge", policyType.ProposerMinAge);
            command.Parameters.AddWithValue("@ProposerMaxAge", policyType.ProposerMaxAge);
            command.Parameters.AddWithValue("@PremiumPayerMinAge", policyType.PremiumPayerMinAge);
            command.Parameters.AddWithValue("@PremiumPayerMaxAge", policyType.PremiumPayerMaxAge);
            command.Parameters.AddWithValue("@Current", policyType.Current);
            command.Parameters.AddWithValue("@AddedOn", (object)policyType.AddedOn ?? DBNull.Value);
            command.Parameters.AddWithValue("@AddedBy", (object)policyType.AddedBy ?? DBNull.Value);
        }
        private PolicyType MapToPolicyType(SqlDataReader reader)
        {
            return new PolicyType
            {
                EntryNo = (int)reader["EntryNo"],
                ID = (Guid)reader["ID"],
                Name = (string)reader["Name"],
                OpenForNewBusinessDesc =(string)reader["OpenForNewBusinessDesc"],
               // OpenForNewBusiness = (byte)reader["OpenForNewBusiness"],
                CurrencyName= (string)reader["CurrencyName"],
                CurrencyID = (int)reader["CurrencyID"],
                MinimumTerm = (int)reader["MinimumTerm"],
                MaximumTerm = (int)reader["MaximumTerm"],
                AllowDeferingOfMaturityDateDesc= (string)reader["AllowDeferingOfMaturityDateDesc"],
                //AllowDeferingOfMaturityDate = (byte)reader["AllowDeferingOfMaturityDate"],
                DefermentNoticePeriod = (int)reader["DefermentNoticePeriod"],
                LifeAssuredMinAge = (int)reader["LifeAssuredMinAge"],
                LifeAssuredMaxAge = (int)reader["LifeAssuredMaxAge"],
                ProposerMinAge = (int)reader["ProposerMinAge"],
                ProposerMaxAge = (int)reader["ProposerMaxAge"],
                PremiumPayerMinAge = (int)reader["PremiumPayerMinAge"],
                PremiumPayerMaxAge = (int)reader["PremiumPayerMaxAge"],
               // Current = (byte)reader["Current"],
                AddedOn = reader["AddedOn"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["AddedOn"],
                AddedBy = reader["AddedBy"] == DBNull.Value ? null : (string)reader["AddedBy"]
            };    
        }
		private PolicyType MapToPolicyTypeShort(SqlDataReader reader)
		{
			return new PolicyType
			{ 
				ID = (Guid)reader["PolicyTypeID"],
				Name = (string)reader["Name"] 
			};
		}
		private PolicyType DirectMapToPolicyType(SqlDataReader reader)
        {
            return new PolicyType
            {
                EntryNo = (int)reader["EntryNo"],
                ID = (Guid)reader["ID"],
                Name = (string)reader["Name"], 
                OpenForNewBusiness = (byte)reader["OpenForNewBusiness"],
                CurrencyID = (int)reader["CurrencyID"],
                MinimumTerm = (int)reader["MinimumTerm"],
                MaximumTerm = (int)reader["MaximumTerm"], 
                AllowDeferingOfMaturityDate = (byte)reader["AllowDeferingOfMaturityDate"],
                DefermentNoticePeriod = (int)reader["DefermentNoticePeriod"],
                LifeAssuredMinAge = (int)reader["LifeAssuredMinAge"],
                LifeAssuredMaxAge = (int)reader["LifeAssuredMaxAge"],
                ProposerMinAge = (int)reader["ProposerMinAge"],
                ProposerMaxAge = (int)reader["ProposerMaxAge"],
                PremiumPayerMinAge = (int)reader["PremiumPayerMinAge"],
                PremiumPayerMaxAge = (int)reader["PremiumPayerMaxAge"],
                Current = (byte)reader["Current"],
                AddedOn = reader["AddedOn"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["AddedOn"],
                AddedBy = reader["AddedBy"] == DBNull.Value ? null : (string)reader["AddedBy"]
            };
        }
        public DataTable GetDesignationPolicyTypes(string UserID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "DesignationPolicyTypes_GetByUser";
            command.Parameters.AddWithValue("UserID", UserID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public bool CheckDesignationPermission(string UserID, Guid PolicyTypeID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection connection = new(Database);
            connection.Open();
            string query = "DECLARE @Allow tinyint=0; DECLARE @DesignationID int; SELECT @DesignationID=ISNULL([DesignationID],-1) FROM [AspNetUsers] WHERE [Id]=@UserID; SELECT @Allow=COUNT(*)  FROM [dbo].[DesignationPolicyTypes] WHERE [DesignationID]=@DesignationID AND ([PolicyType]=@PolicyTypeID OR [PolicyType]='00000000-0000-0000-0000-000000000000'); SELECT @Allow";
            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@UserID", UserID);
            command.Parameters.AddWithValue("@PolicyTypeID", PolicyTypeID);
            return Convert.ToBoolean(command.ExecuteScalar());
        }
    }
    
}
