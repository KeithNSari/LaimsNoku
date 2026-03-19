using Azure.Core;
using LAIMS.Interfaces.Claims;
using LAIMS.Models.Claims;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Membership;
using LAIMS.Models.Premiums;
using LAIMS.Models.Utilities;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LAIMS.Repositories.Claims
{
    public class PolicyClaimRepository: IPolicyClaimRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private readonly string Database;

        public PolicyClaimRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public int GetClaimIDByRequestID(Guid RequestID)
        {
            int id = 0;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @ID int=0; SELECT @ID=[ID] FROM [dbo].[PolicyClaims] WHERE [RequestID]=@RequestID; SELECT @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return id;
        }
		public Guid GetPolicyIDByClaimRequestID(Guid RequestID)
		{ 
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "DECLARE @PolicyID uniqueidentifier='00000000-0000-0000-0000-000000000000'; SELECT @PolicyID=[PolicyID] FROM [dbo].[PolicyClaims] WHERE [RequestID]=@RequestID; SELECT @PolicyID";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@RequestID", RequestID);
					return Guid.Parse(command.ExecuteScalar().ToString());
				}
			} 
		}
		public decimal GetProposedUnits(int ClaimID)
        { 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Units decimal(18,7)=0;SELECT TOP (1) @Units=[Units] FROM [dbo].[PolicyUnitsLines] WHERE [ClaimID]=@ClaimID AND [TransactionTypeID]=4 AND [Archived]=0; SELECT @Units";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClaimID", ClaimID);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            } 
        }
        public Guid GetClaimTrust(int ClaimID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT TOP (1) [UnitTrustID] FROM [dbo].[PolicyUnitsLines] LEFT JOIN [PolicyUnits] ON [PolicyUnitsLines].[PolicyUnitsID]=[PolicyUnits].[ID] WHERE [ClaimID]=@ClaimID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClaimID", ClaimID);
                    return Guid.Parse(command.ExecuteScalar().ToString());
                }
            }
        }
        public bool PurchaseMade(int ClaimID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @PurchaseMade int=0;SELECT  @PurchaseMade=Count(*) FROM [dbo].[PolicyUnitsLines] WHERE [ClaimID]=@ClaimID AND [TransactionTypeID]=2 AND [Archived]=0; SELECT @PurchaseMade";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClaimID", ClaimID);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public decimal AllocationBalance(int ClaimID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @Allocated decimal(18,7)=0;DECLARE @Amount decimal(18,7)=0; SELECT TOP (1) @Amount=[Amount] FROM [dbo].[PolicyUnitsLines] WHERE ClaimID=@ClaimID AND [TransactionTypeID]=2 AND [Archived]=0 ORDER BY [ID] DESC; SELECT @Allocated=Sum([Amount]) FROM [dbo].[PolicyClaimaints] WHERE [ClaimID]=@ClaimID AND [Archived]=0; SELECT ISNULL(@Amount,0)-ISNULL(@Allocated,0) AS [Balance]";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ClaimID", ClaimID);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
		public decimal AllocationRequestBalance(int ClaimID)
		{
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string sql = "PolicyClaims_AllocationBalance";
				using (SqlCommand command = new SqlCommand(sql, connection))
				{
                    command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("@ClaimID", ClaimID);
					return Convert.ToDecimal(command.ExecuteScalar());
				}
			}
		}
		public List<PolicyClaim> GetAllClaims()
        {
            List<PolicyClaim> claims = new List<PolicyClaim>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM PolicyClaims";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        claims.Add(MapData(reader));
                    }
                }
            }

            return claims;
        }

        public PolicyClaim GetClaimById(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM PolicyClaims WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapData(reader);
                        }
                    }
                }

                return null;
            }
        }

        public void AddClaim(PolicyClaim claim)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"DECLARE @Currency int=0; SELECT @Currency=[CurrencyID] FROM [Policy] WHERE [ID]=@PolicyID; 
                 INSERT INTO PolicyClaims
                (PolicyID, RequestID, EventID, ClaimantID, ClaimDate,ClaimTypeID,ValueMode, TotalAmount, PaymentMethodID, CurrencyID, Paid, DatePaid, PaidComment, 
                StatusID, StatusDate, StatusComment, StatusAddedBy, AddedOn, AddedBy)
                VALUES
                (@PolicyID, @RequestID, @EventID, @ClaimantID, @ClaimDate,@ClaimTypeID,@ValueMode, @TotalAmount, @PaymentMethodID, @Currency, @Paid, @DatePaid, @PaidComment, 
                @StatusID, @StatusDate, @StatusComment, @StatusAddedBy, @AddedOn, @AddedBy);
                SELECT SCOPE_IDENTITY();";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    MapParameters(command, claim);

                    claim.ID = Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void UpdateClaim(PolicyClaim claim)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"
                UPDATE PolicyClaims
                SET PolicyID = @PolicyID, RequestID = @RequestID, EventID = @EventID, ClaimantID = @ClaimantID, ClaimDate = @ClaimDate, 
                    TotalAmount = @TotalAmount, PaymentMethodID = @PaymentMethodID, Paid = @Paid, DatePaid = @DatePaid, 
                    PaidComment = @PaidComment, StatusID = @StatusID, StatusDate = @StatusDate, StatusComment = @StatusComment, 
                    StatusAddedBy = @StatusAddedBy, AddedOn = @AddedOn, AddedBy = @AddedBy
                WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    MapParameters(command, claim);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateClaimTotal(Guid RequestID, decimal Total)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"UPDATE PolicyClaims 
                SET [TotalAmount]=@Total,[DisbursementAmount]=@Total WHERE RequestID=@RequestID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.Parameters.AddWithValue("@Total",Total);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeleteClaim(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM PolicyClaims WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    command.ExecuteNonQuery();
                }
            }
        }
        public int AddClaimDocument(Guid RequestID, Guid DocumentID, Guid MediaUploadID, string AddedBy)
        {
            int id = 0;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @ID int=0; INSERT INTO [dbo].[PolicyClaimDocuments]([ClaimRequestID],[DocumentID],[MediaUploadID],[AddedBy]) VALUES (@ClaimRequestID,@DocumentID,@MediaUploadID,@AddedBy); SELECT @ID=@@IDENTITY";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClaimRequestID", RequestID);
                    command.Parameters.AddWithValue("@DocumentID", DocumentID);
                    command.Parameters.AddWithValue("@MediaUploadID", MediaUploadID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return id;
        }
        public void ArchiveExpense(Guid RequestID, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DECLARE @PolicyClaimID bigint; SELECT @PolicyClaimID=[ID] FROM [dbo].[PolicyClaims] " +
                    "WHERE [RequestID]=@RequestID; " +
                    "UPDATE [dbo].[PolicyClaimExpenses] SET [Archived]=1,[ArchivedBy]=@AddedBy,[ArchivedOn]=GetDate() " +
                    "WHERE [PolicyClaimID]=@PolicyClaimID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RequestID", RequestID); 
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        public int AddClaimExpense(Guid RequestID, int ClaimTypeExpenseID,string AddedBy)//deprecated
        {
            int id = 0;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyClaimExpenses_Add";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.Parameters.AddWithValue("@ClaimTypeExpenseID", ClaimTypeExpenseID); 
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return id;
        }
		public int AddClaimUserExpense(Guid RequestID, int ClaimTypeExpenseID, decimal Amount, string AddedBy)
		{
			int id = 0;
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "PolicyClaimExpenses_AddUserExpense";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("@RequestID", RequestID);
					command.Parameters.AddWithValue("@Amount", Amount);
					command.Parameters.AddWithValue("@ClaimTypeExpenseID", ClaimTypeExpenseID);
					command.Parameters.AddWithValue("@AddedBy", AddedBy);
					id = Convert.ToInt32(command.ExecuteScalar());
				}
			}
			return id;
		}
		public int AddClaimSystemExpenses(Guid RequestID,string AddedBy)
		{ 
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "PolicyClaimExpenses_AddSystemExpenses";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("@RequestID", RequestID); 
					command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    return command.ExecuteNonQuery();
				}
			} 
		}
		public int AddClaimCalculatedExpenses(Guid RequestID, string AddedBy)
		{
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "PolicyClaim_AddCalculatedExpenses";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("@RequestID", RequestID);
					command.Parameters.AddWithValue("@AddedBy", AddedBy);
					return command.ExecuteNonQuery();
				}
			}
		}
		public void AddClaimant(PolicyClaimant policyClaimant)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[PolicyClaimaints] SET [Archived]=1 WHERE ([Archived]=0) AND [ClaimID]=@ClaimID AND [MemberID]=@MemberID AND [PolicyBeneficiariesLineID]=@PolicyBeneficiariesLineID; INSERT INTO PolicyClaimaints (ClaimID, MemberID,BankAccountID,CellPhoneID,TelephoneID,EmailAddressID,AddressID,RoleID,PolicyBeneficiariesLineID,[Main],[Amount],PayAfter,AddedOn, AddedBy) VALUES (@ClaimID, @MemberID,@BankAccountID,@CellPhoneID,@TelephoneID,@EmailAddressID,@AddressID,@RoleID,@PolicyBeneficiariesLineID,@Main,@Amount,@PayAfter,@AddedOn, @AddedBy)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClaimID", policyClaimant.ClaimID);
                    command.Parameters.AddWithValue("@MemberID", policyClaimant.MemberID);
                    command.Parameters.AddWithValue("@BankAccountID", policyClaimant.BankAccountID); 
                    command.Parameters.AddWithValue("@CellPhoneID", policyClaimant.CellPhoneID);
                    command.Parameters.AddWithValue("@TelephoneID", policyClaimant.TelephoneID);
                    command.Parameters.AddWithValue("@EmailAddressID", policyClaimant.EmailAddressID);
                    command.Parameters.AddWithValue("@AddressID", policyClaimant.AddressID);
                    command.Parameters.AddWithValue("@RoleID", policyClaimant.RoleID);
                    command.Parameters.AddWithValue("@PolicyBeneficiariesLineID", policyClaimant.PolicyBeneficiariesLineID);
                    command.Parameters.AddWithValue("@Main", policyClaimant.MainCover);
                    command.Parameters.AddWithValue("@Amount", policyClaimant.Amount);
                    command.Parameters.AddWithValue("@PayAfter",policyClaimant.PayAfter);
                    command.Parameters.AddWithValue("@AddedOn", policyClaimant.AddedOn ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", policyClaimant.AddedBy ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }
		public void AddClaimSubmitter(PolicyClaimant policyClaimant)
		{
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "UPDATE [dbo].[PolicyClaimaints] SET [Archived]=1 WHERE ([Archived]=0) AND [ClaimID]=@ClaimID AND [RoleID]=5; INSERT INTO PolicyClaimaints (ClaimID, MemberID,BankAccountID,CellPhoneID,TelephoneID,EmailAddressID,AddressID,RoleID,PolicyBeneficiariesLineID,[Main],[Amount],PayAfter,AddedOn, AddedBy) VALUES (@ClaimID, @MemberID,@BankAccountID,@CellPhoneID,@TelephoneID,@EmailAddressID,@AddressID,@RoleID,@PolicyBeneficiariesLineID,@Main,@Amount,@PayAfter,@AddedOn, @AddedBy)";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@ClaimID", policyClaimant.ClaimID);
					command.Parameters.AddWithValue("@MemberID", policyClaimant.MemberID);
					command.Parameters.AddWithValue("@BankAccountID", policyClaimant.BankAccountID);
					command.Parameters.AddWithValue("@CellPhoneID", policyClaimant.CellPhoneID);
					command.Parameters.AddWithValue("@TelephoneID", policyClaimant.TelephoneID);
					command.Parameters.AddWithValue("@EmailAddressID", policyClaimant.EmailAddressID);
					command.Parameters.AddWithValue("@AddressID", policyClaimant.AddressID);
					command.Parameters.AddWithValue("@RoleID", 5);
					command.Parameters.AddWithValue("@PolicyBeneficiariesLineID", policyClaimant.PolicyBeneficiariesLineID);
					command.Parameters.AddWithValue("@Main", policyClaimant.MainCover);
					command.Parameters.AddWithValue("@Amount", policyClaimant.Amount);
					command.Parameters.AddWithValue("@PayAfter", policyClaimant.PayAfter);
					command.Parameters.AddWithValue("@AddedOn", policyClaimant.AddedOn ?? (object)DBNull.Value);
					command.Parameters.AddWithValue("@AddedBy", policyClaimant.AddedBy ?? (object)DBNull.Value);

					command.ExecuteNonQuery();
				}
			}
		}
		public void UpdateClaimant(PolicyClaimant policyClaimant)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM [dbo].[PolicyClaimaints] WHERE ([Archived]=0) AND [ClaimID]=@ClaimID AND [MemberID]=@MemberID; IF(@Count=0) BEGIN INSERT INTO PolicyClaimaints (ClaimID, MemberID, BankAccountID,AmountIsPercentage,Amount, AddedOn, AddedBy) VALUES (@ClaimID, @MemberID, @BankAccountID,@AmountIsPercentage,@Amount, @AddedOn, @AddedBy) END";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClaimID", policyClaimant.ClaimID);
                    command.Parameters.AddWithValue("@MemberID", policyClaimant.MemberID);
                    command.Parameters.AddWithValue("@BankAccountID", policyClaimant.BankAccountID);
                    command.Parameters.AddWithValue("@AmountIsPercentage", policyClaimant.AmountIsPercentage);
                    command.Parameters.AddWithValue("@Amount", policyClaimant.Amount);
                    command.Parameters.AddWithValue("@AddedOn", policyClaimant.AddedOn ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", policyClaimant.AddedBy ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateClaimantAmount(int ClaimantID, decimal Amount)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyClaimants_UpdateCover";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    command.Parameters.AddWithValue("@Amount", Amount);
                    command.Parameters.AddWithValue("@ID", ClaimantID); 
                    command.ExecuteNonQuery();
                }
            }
        }
        public void AddDeathRecord(DeathRecord deathRecord)
        {
            string insertQuery = @"UPDATE DeathRecords SET [Archived]=1,[ArchivedBy]=@AddedBy WHERE [PolicyClaimID]=@PolicyClaimID; INSERT INTO DeathRecords (PolicyClaimID,MemberID, DeathCertificateNo,BurialOrderNo,DateHealthAffected,DateOfDeath, EventCauseID, CauseDetails, Place, Hospital, PoliceStation, CaseReferenceNo,AddedBy) 
                               VALUES (@PolicyClaimID,@MemberID, @DeathCertificateNo,@BurialOrderNo,@DateHealthAffected,@DateOfDeath, @EventCauseID, @CauseDetails, @Place, @Hospital, @PoliceStation, @CaseReferenceNo,@AddedBy)";
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@PolicyClaimID", deathRecord.PolicyClaimID);
                    command.Parameters.AddWithValue("@DeathCertificateNo", (object)deathRecord.DeathCertificateNo ?? DBNull.Value);
                    var dateHealthAffectedParam = new SqlParameter("@DateHealthAffected", SqlDbType.DateTime);
                    if (deathRecord.DateHealthAffected.HasValue)
                    {
                        dateHealthAffectedParam.Value = deathRecord.DateHealthAffected.Value;
                    }
                    else
                    {
                        dateHealthAffectedParam.Value = DBNull.Value;
                    }
                    command.Parameters.Add(dateHealthAffectedParam); command.Parameters.AddWithValue("@DateOfDeath", deathRecord.DateOfDeath);
                    command.Parameters.AddWithValue("@EventCauseID", deathRecord.EventCauseID);
                    command.Parameters.AddWithValue("@MemberID", deathRecord.MemberID);
                    command.Parameters.AddWithValue("@CauseDetails", (object)deathRecord.CauseDetails ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Place", (object)deathRecord.Place ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Hospital", (object)deathRecord.Hospital ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PoliceStation", (object)deathRecord.PoliceStation ?? DBNull.Value);
                    command.Parameters.AddWithValue("@BurialOrderNo", (object)deathRecord.BurialOrderNo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CaseReferenceNo", (object)deathRecord.CaseReferenceNo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", deathRecord.AddedBy);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
		public int CheckDeathRecordExistence(int MemberID)
		{
			int count = 0;
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "DeathRecords_CheckExistence";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("@MemberID", MemberID); 
					count = Convert.ToInt32(command.ExecuteScalar());
				}
			}
			return count;
		}
		public void AddNewDeathRecord(DeathRecord deathRecord)
        {
            string insertQuery = @"INSERT INTO DeathRecords (RequestID, MemberID, DeathCertificateNo,BurialOrderNo,DateHealthAffected,DateOfDeath, EventCauseID, CauseDetails, Place, Hospital, PoliceStation, CaseReferenceNo,AddedBy) 
                               VALUES (@RequestID,@MemberID, @DeathCertificateNo,@BurialOrderNo,@DateHealthAffected,@DateOfDeath, @EventCauseID, @CauseDetails, @Place, @Hospital, @PoliceStation, @CaseReferenceNo,@AddedBy)";
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@PolicyClaimID", deathRecord.PolicyClaimID);
                    command.Parameters.AddWithValue("@DeathCertificateNo", (object)deathRecord.DeathCertificateNo ?? DBNull.Value);
                    var dateHealthAffectedParam = new SqlParameter("@DateHealthAffected", SqlDbType.DateTime);
                    if (deathRecord.DateHealthAffected.HasValue)
                    {
                        dateHealthAffectedParam.Value = deathRecord.DateHealthAffected.Value;
                    }
                    else
                    {
                        dateHealthAffectedParam.Value = DBNull.Value;
                    }
                    command.Parameters.Add(dateHealthAffectedParam); command.Parameters.AddWithValue("@DateOfDeath", deathRecord.DateOfDeath);
                    command.Parameters.AddWithValue("@RequestID", deathRecord.RequestID);
                    command.Parameters.AddWithValue("@EventCauseID", deathRecord.EventCauseID);
                    command.Parameters.AddWithValue("@MemberID", deathRecord.MemberID);
                    command.Parameters.AddWithValue("@CauseDetails", (object)deathRecord.CauseDetails ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Place", (object)deathRecord.Place ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Hospital", (object)deathRecord.Hospital ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PoliceStation", (object)deathRecord.PoliceStation ?? DBNull.Value);
                    command.Parameters.AddWithValue("@BurialOrderNo", (object)deathRecord.BurialOrderNo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CaseReferenceNo", (object)deathRecord.CaseReferenceNo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", deathRecord.AddedBy);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        public void AddDeaths(int MemberID, int PolicyClaimID)
        {
            string insertQuery = @"INSERT INTO PolicyClaimDeaths([PolicyClaimID],[MemberID]) VALUES (@PolicyClaimID,@MemberID)";
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@MemberID", MemberID);
                    command.Parameters.AddWithValue("@PolicyClaimID", PolicyClaimID); 
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
		public int CountDeathRecords(Guid PolicyID)
		{ 
			using (SqlConnection connection = new SqlConnection(Database))
			{
				using (SqlCommand command = new SqlCommand("Policy_GetDeceasedBeneficiariesCount", connection))
				{
                    command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("@PolicyID", PolicyID); 
					connection.Open();
					return Convert.ToInt32(command.ExecuteScalar());
				}
			}
		}
		public void DeathClaimUpdatePolicyStatus(Guid RequestID)
        {
            string insertQuery = "PolicyDeathClaim_UpdatePolicyStatus";
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RequestID", @RequestID); 
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        public List<DeathRecord> GetAllDeathRecords(int PolicyClaimID)
        {
            var deathRecords = new List<DeathRecord>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "DeathRecords_Get";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyClaimID", PolicyClaimID);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var deathRecord = new DeathRecord
                            { 
                                PolicyClaimID = (int)reader["PolicyClaimID"],
                                MemberName = (string)reader["MemberName"],
                                DateHealthAffected = reader["DateHealthAffected"] != DBNull.Value ? (DateTime?)reader["DateHealthAffected"] : null,
                                DateOfDeath = (DateTime)reader["DateOfDeath"],
                                EventCauseID = (int)reader["EventCauseID"],
                                EventCauseName = (string)reader["EventCauseName"],
                                CauseDetails = reader["CauseDetails"] as string,
                                Place = (string)reader["Place"],
                                Hospital = reader["Hospital"] as string,
                                PoliceStation = reader["PoliceStation"] as string,
                                CaseReferenceNo = reader["CaseReferenceNo"] as string,
                                BurialOrderNo = reader["BurialOrderNo"] as string,
                                DeathCertificateNo = reader["DeathCertificateNo"] as string,
                                AddedBy = reader["AddedBy"] as string,
                                AddedOn = (DateTime)reader["AddedOn"]
                            };
                            deathRecords.Add(deathRecord);
                        }
                    }
                }
            }
            return deathRecords;
        }
        public List<DeathRecord> GetAllDeathRecords(Guid RequestID)
        {
            var deathRecords = new List<DeathRecord>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "DeathRecords_GetByRequestID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var deathRecord = new DeathRecord
                            {
                                PolicyClaimID = (int)reader["PolicyClaimID"],
                                MemberName = (string)reader["MemberName"],
                                DateHealthAffected = reader["DateHealthAffected"] != DBNull.Value ? (DateTime?)reader["DateHealthAffected"] : null,
                                DateOfDeath = (DateTime)reader["DateOfDeath"],
                                EventCauseID = (int)reader["EventCauseID"],
                                EventCauseName = (string)reader["EventCauseName"],
                                CauseDetails = reader["CauseDetails"] as string,
                                Place = (string)reader["Place"],
                                Hospital = reader["Hospital"] as string,
                                PoliceStation = reader["PoliceStation"] as string,
                                CaseReferenceNo = reader["CaseReferenceNo"] as string,
                                BurialOrderNo = reader["BurialOrderNo"] as string,
                                DeathCertificateNo = reader["DeathCertificateNo"] as string,
                                AddedBy = reader["AddedBy"] as string,
                                AddedOn = (DateTime)reader["AddedOn"]
                            };
                            deathRecords.Add(deathRecord);
                        }
                    }
                }
            }
            return deathRecords;
        }
        public List<ClaimExpense> GetClaimExpensePriceList(Guid RequestID)
        {
            List<ClaimExpense> claimExpensePriceList = new List<ClaimExpense>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "PolicyClaimant_GetClaimTypeExpenses";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var claimExpense = new ClaimExpense
                            {
                                ID = (int)reader["ID"],
                                Item = (string)reader["Item"],
                                Price = (decimal)reader["Price"],
                                IsSelected = false,
                                ApplicationType= (byte)reader["ApplicationTypeID"]
							};
                           claimExpensePriceList.Add(claimExpense);
                        }
                    }
                }
            }
            return claimExpensePriceList;
        }
        public int ContractualPartyCheck(int MemberID, Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT Count(*) FROM [dbo].[PolicyBeneficiaries] WHERE [Archived]=0 AND [MemberID]=@MemberID AND [HeaderID]=@PolicyID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MemberID", MemberID);
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public string GetPolicyNo (Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @PolicyNo varchar(20); DECLARE @PolicyID uniqueidentifier; SELECT @PolicyID=[PolicyID] FROM [dbo].[PolicyClaims] WHERE [RequestID]=@ClaimRequestID; SELECT @PolicyNo=[PolicyNo] FROM [Policy] WHERE [ID]=@PolicyID;SELECT @PolicyNo";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ClaimRequestID", RequestID);
                    return  command.ExecuteScalar().ToString();
                }
            }
        }
        public Guid GetPolicyTypeID(Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @PolicyTypeID uniqueidentifier; DECLARE @PolicyID uniqueidentifier; SELECT @PolicyID=[PolicyID] FROM [dbo].[PolicyClaims] WHERE [RequestID]=@ClaimRequestID; SELECT @PolicyTypeID=[PolicyType] FROM [Policy] WHERE [ID]=@PolicyID;SELECT @PolicyTypeID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ClaimRequestID", RequestID);
                    return Guid.Parse(command.ExecuteScalar().ToString());
                }
            }
        }
        public int GetMemberID(Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyClaim_GetMemberID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public DataTable GetPolicyClaimants(int ClaimID, int MainCover)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyClaimants_Get";
            cmd.Parameters.AddWithValue("ClaimID", ClaimID);
            cmd.Parameters.AddWithValue("Main", MainCover);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetClaimCover(int ClaimID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyClaimant_GetClaimCover";
            cmd.Parameters.AddWithValue("ClaimID", ClaimID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetNonInvestmentSuppementaryCover(string PolicyNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Member_GetNonInvestmentSupplementaryCover";
            cmd.Parameters.AddWithValue("PolicyNo", PolicyNo);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetClaimTypes(Guid ProductID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ClaimTypes_GetConfigured";
            cmd.Parameters.AddWithValue("ProductID", ProductID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetUnpaidPremiums(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "BilledPremiums_GetUnpaidByPolicyID";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID); 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public decimal GetUnpaidPremiumsBalance(Guid PolicyID)
        {
            string insertQuery = "BilledPremiums_GetUnpaidBalanceByPolicyID";
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID); 
                    connection.Open();
                    return Convert.ToDecimal(command.ExecuteScalar()); 
                }
            }
        }
        public bool CheckProductWaitingPeriod(Guid PolicyID, Guid ProductID)
        {
            string insertQuery = "ProductClaim_CheckWaitingPeriod";
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@ProductID", ProductID);
                    connection.Open();
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public DataTable GetExpenses(Guid RequestID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyClaim_GetExpenses";
            cmd.Parameters.AddWithValue("@RequestID", RequestID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public decimal GetExpensesTotal(Guid RequestID)
        {
            string insertQuery = "PolicyClaim_GetExpensesTotal";
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    connection.Open();
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        public DataTable GetMemberDeaths()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "DeathRecords_GetLatest"; 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void AddServiceBreakdown(int ClaimID,int ServiceID, decimal Amount, string AddedBy)
        {
            string insertQuery = @"PolicyClaimServices_AddBreakdown";
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ClaimID", ClaimID);
                    command.Parameters.AddWithValue("@ServiceID", ServiceID);
                    command.Parameters.AddWithValue("@Amount", Amount);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        public int GetSystemDecision(Guid RequestID)
        {
            string insertQuery = "PolicyClaims_GetSystemDecision";
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RequestID", RequestID); 
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public DataTable GetServiceBreakdown(int ClaimID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyClaimant_GetServices";
            cmd.Parameters.AddWithValue("ClaimID", ClaimID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetDocuments(Guid MemberUID, Guid RequestID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "DECLARE @MemberID int=0; " +
                "SELECT @MemberID=ID FROM Members WHERE UID=@MemberUID; " +
                "SELECT [MediaUploads].[ID],[MemberID],[FilingNo],[Documents].[Document],[DocumentNo],[ContentType]," +
                "[MediaUploads].[DocumentsID],[FileName],[MediaUploads].[AddedOn],[MediaUploads].[AddedBy] " +
                "FROM [dbo].[PolicyClaimDocuments] LEFT JOIN [MediaUploads] ON" +
                " [PolicyClaimDocuments].[MediaUploadID]=[MediaUploads].[ID] LEFT JOIN [Documents]" +
                " ON [MediaUploads].[DocumentsID]=[Documents].[ID] " +
                "WHERE ([MediaUploads].[MemberID]=@MemberID) AND ([PolicyClaimDocuments].[ClaimRequestID]=@RequestID) ORDER BY [Documents].[Document] ASC";
            cmd.Parameters.AddWithValue("MemberUID", MemberUID);
            cmd.Parameters.AddWithValue("@RequestID", RequestID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetBeneficiariesByClaimType(Guid PolicyID, int ClaimTypeID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Policy_GetBeneficiariesByClaimType";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            cmd.Parameters.AddWithValue("ClaimTypeID",ClaimTypeID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
		public DataTable GetDeceasedBeneficiariesByClaimType(Guid PolicyID, int ClaimTypeID)
		{
			var Database = _configuration.GetConnectionString("DefaultConnection");
			DataTable DT = new DataTable();
			SqlConnection connection = new SqlConnection();
			connection.ConnectionString = Database;
			SqlCommand cmd = connection.CreateCommand();
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "Policy_GetDeceasedBeneficiariesByClaimType";
			cmd.Parameters.AddWithValue("PolicyID", PolicyID);
			cmd.Parameters.AddWithValue("ClaimTypeID", ClaimTypeID);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			da.Fill(DT);
			return DT;
		}
		public List<EventType> GetEventTypes()
        {
            List<EventType> eventTypes = new List<EventType>();
            string query = "SELECT EntryNo, ID, EventName, AddedOn, AddedBy FROM EventTypes";
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        EventType eventType = new EventType
                        {
                            EntryNo = Convert.ToInt32(reader["EntryNo"]),
                            ID = Guid.Parse(reader["ID"].ToString()),
                            EventName = reader["EventName"].ToString(),
                            AddedOn = reader["AddedOn"] != DBNull.Value ? Convert.ToDateTime(reader["AddedOn"]) : (DateTime?)null,
                            AddedBy = reader["AddedBy"] != DBNull.Value ? reader["AddedBy"].ToString() : null
                        };
                        eventTypes.Add(eventType);
                    }
                    reader.Close();
                }
            }
            return eventTypes;
        }
        public List<EventTypeCause> GetEventTypeCauses(Guid eventTypeID)
        {
            List<EventTypeCause> eventTypeCauses = new List<EventTypeCause>();

            string query = "SELECT ID, EventTypeID, Cause, Description FROM EventTypeCauses WHERE EventTypeID=@EventTypeID ORDER BY Cause ASC";

            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EventTypeID", eventTypeID);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        EventTypeCause eventTypeCause = new EventTypeCause
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            EventTypeID = Guid.Parse(reader["EventTypeID"].ToString()),
                            Cause = reader["Cause"].ToString(),
                            Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null
                        };
                        eventTypeCauses.Add(eventTypeCause);
                    }
                    reader.Close();
                }
            }
            return eventTypeCauses;
        }
        public List<Member> SearchServiceProviders(string SearchTerm)
        {
            List<Member> organisations = new List<Member>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Organisations_Search";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@SearchTerm", SearchTerm);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Member member = MapDataToMember(reader);
                            organisations.Add(member);
                        }
                    }
                }
            }
            return organisations;
        }
        public DataTable GetBeneficiaryShares(Guid PolicyID, decimal Balance)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PBLSplits_GetBalanceCalculation";
            cmd.Parameters.AddWithValue("PolicyID", PolicyID);
            cmd.Parameters.AddWithValue("Balance", Balance);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        private PolicyClaim MapData(SqlDataReader reader)
        {
            return new PolicyClaim
            {
                ID = Convert.ToInt32(reader["ID"]),
                PolicyID = Guid.Parse(reader["PolicyID"].ToString()),
                RequestID = Guid.Parse(reader["RequestID"].ToString()),
                EventID = Convert.ToInt32(reader["EventID"]),
                ClaimantID = Convert.ToInt32(reader["ClaimantID"]),
                ClaimDate = Convert.ToDateTime(reader["ClaimDate"]),
                TotalAmount = reader["TotalAmount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["TotalAmount"]),
                PaymentMethodID = reader["PaymentMethodID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["PaymentMethodID"]),
                CurrencyID = reader["CurrencyID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CurrencyID"]),
                Paid = reader["Paid"] == DBNull.Value ? (byte?)null : Convert.ToByte(reader["Paid"]),
                DatePaid = reader["DatePaid"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DatePaid"]),
                PaidComment = reader["PaidComment"].ToString(),
                StatusID = Convert.ToInt32(reader["StatusID"]),
                StatusDate = Convert.ToDateTime(reader["StatusDate"]),
                StatusComment = reader["StatusComment"].ToString(),
                StatusAddedBy = reader["StatusAddedBy"].ToString(),
                AddedOn = reader["AddedOn"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["AddedOn"]),
                AddedBy = reader["AddedBy"].ToString()
            };
        }
        public void ArchiveClaimLine(int ID, string ArchivedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Declare @ArchivedOn datetime2(7)=GetUTCDate(); Update [dbo].[PolicyClaimaints] SET [Archived]=1,[ArchivedBy]=@ArchivedBy,[ArchivedOn]=@ArchivedOn WHERE [ID]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text; 
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@ArchivedBy", ArchivedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        public decimal AllocationTotal(int ClaimID, int PolicyBeneficiariesLineID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT ISNULL(SUM([Amount]),0) AS [Allocated] FROM [dbo].[PolicyClaimaints] WHERE [Archived]=0 AND [PolicyBeneficiariesLineID]=@PolicyBeneficiariesLineID AND [ClaimID]=@ClaimID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClaimID",ClaimID); 
                    command.Parameters.AddWithValue("@PolicyBeneficiariesLineID", PolicyBeneficiariesLineID);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        public decimal CoverBalance(int ClaimID, int PolicyBeneficiariesLineID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Balance decimal(18,2)=0; DECLARE @Allocated decimal(18,2)=0; DECLARE @Cover decimal(18,2)=0; SELECT  @Allocated=ISNULL(SUM([Amount]),0) FROM [dbo].[PolicyClaimaints] WHERE [Archived]=0 AND [PolicyBeneficiariesLineID]=@PolicyBeneficiariesLineID AND [ClaimID]=@ClaimID; SELECT @Cover=Cover FROM [PolicyBeneficiariesLines] WHERE [ID]=@PolicyBeneficiariesLineID AND [Archived]=0; SET @Balance=@Cover-@Allocated; SELECT @Balance AS Balance";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClaimID", ClaimID);
                    command.Parameters.AddWithValue("@PolicyBeneficiariesLineID", PolicyBeneficiariesLineID);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        public decimal UpdateAllocationTotal(Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyClaim_AddTotal";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RequestID", RequestID); 
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        public decimal GetAllocationTotal(Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyClaim_GetTotal";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        public decimal GetDisbursementAmount(Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyClaim_GetDisbursementAmount";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        public int UpdateDisbursementAmount(Guid RequestID, decimal DisbursementAmount)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyClaim_UpdateDisbursementAmount";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.Parameters.AddWithValue("@DisbursementAmount", DisbursementAmount);
                    return command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetMySubmissions(string AddedBy)
        { 
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyClaims_GetMySubmissions";
            cmd.Parameters.AddWithValue("@AddedBy", AddedBy);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetUnReviewed()
        { 
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyClaims_GetUnReviewed"; 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable SearchUnReviewed(string SearchTerm)
        { 
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyClaims_SearchUnReviewed";
            cmd.Parameters.AddWithValue("@SearchTerm", SearchTerm);
			cmd.Parameters.AddWithValue("@NormalisedSearchTerm", (SearchTerm).ToUpper().Replace("-", "").Replace(" ", ""));
			SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetByPolicyID(Guid PolicyID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyClaims_GetByPolicyID";
            cmd.Parameters.AddWithValue("@PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable SearchMySubmissions(string AddedBy, string PolicyNo)
        { 
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyClaims_SearchMySubmissions";
            cmd.Parameters.AddWithValue("@AddedBy", AddedBy);
            cmd.Parameters.AddWithValue("@PolicyNo", PolicyNo);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetMyReviews(string AddedBy)
        { 
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyClaims_GetMyReviews";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AddedBy", AddedBy);
            da.Fill(DT);
            return DT;
        }
        public DataTable SearchMyReviews(string AddedBy, string PolicyNo)
        { 
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyClaims_SearchMyReviews";
            cmd.Parameters.AddWithValue("@AddedBy", AddedBy);
            cmd.Parameters.AddWithValue("@PolicyNo", PolicyNo);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetSubmittedBy(int ClaimID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [PolicyClaimaints].[ID],[Members].[Name3] + ' ' + ISNULL([Members].[Name2] + ' ','') + [Members].[Name1] AS [MemberName],[Members].[NationalID],Convert(varchar,[PolicyClaimaints].[AddedOn],103) AS [AddedOn] FROM [dbo].[PolicyClaimaints] LEFT JOIN [Members] ON [Members].[ID]=[PolicyClaimaints].[MemberID] WHERE [PolicyClaimaints].[Archived]=0 AND [RoleID]=5 AND [ClaimID]=@ClaimID ORDER BY [Name3] ASC, [Name2] ASC,[Name1] ASC";
             cmd.Parameters.AddWithValue("ClaimID", ClaimID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public bool UpdateStatus(Guid RequestID, int StatusID,string StatusAddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[PolicyClaims] SET [StatusID]=@StatusID,[StatusDate]=GetDate(),[StatusAddedBy]=@StatusAddedBy WHERE [RequestID]=@ClaimRequestID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text; 
                    command.Parameters.AddWithValue("@StatusID", StatusID);
                    command.Parameters.AddWithValue("@StatusAddedBy", StatusAddedBy);
                    command.Parameters.AddWithValue("@ClaimRequestID", RequestID);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
		public bool UpdatePolicyStatus(Guid RequestID, string StatusAddedBy)
		{
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "PolicyClaim_UpdatePolicyStatus";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.CommandType = CommandType.StoredProcedure; 
					command.Parameters.AddWithValue("@StatusAddedBy", StatusAddedBy);
					command.Parameters.AddWithValue("@ClaimRequestID", RequestID);
					return Convert.ToBoolean(command.ExecuteNonQuery());
				}
			}
		}
		public bool UpdateInvestmentPolicyStatus(Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyClaim_InvestmentUpdatePolicyStatus";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public DateTime GetSignedDate(Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @SignedOn date; SELECT @SignedOn=ISNULL([SignedOn],GETDATE()) FROM [dbo].[PolicyClaims] WHERE [RequestID]=@ClaimRequestID; SELECT @SignedOn";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ClaimRequestID", RequestID); 
                    return Convert.ToDateTime(command.ExecuteScalar());
                }
            }
        }
        public int GetSubmittedBankBranch(Guid RequestID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @SubmittedBankBranchID int=-1; SELECT @SubmittedBankBranchID=ISNULL([SubmittedBankBranchID],-1) FROM [dbo].[PolicyClaims] WHERE [RequestID]=@ClaimRequestID; SELECT @SubmittedBankBranchID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ClaimRequestID", RequestID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public bool UpdateSignedDate(Guid RequestID,DateTime SignedOn)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[PolicyClaims] SET [SignedOn]=@SignedOn WHERE [RequestID]=@ClaimRequestID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text; 
                    command.Parameters.AddWithValue("@ClaimRequestID", RequestID);
                    command.Parameters.AddWithValue("@SignedOn", SignedOn);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public bool UpdateSubmittedBankBranchID(Guid RequestID, int SubmittedBankBranchID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[PolicyClaims] SET [SubmittedBankBranchID]=@SubmittedBankBranchID WHERE [RequestID]=@ClaimRequestID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ClaimRequestID", RequestID);
                    command.Parameters.AddWithValue("@SubmittedBankBranchID", SubmittedBankBranchID);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public void ArchiveSubmittedByEntry(int ID, string AddedBy, DateTime AddedOn)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[PolicyClaimaints] Set [Archived]=1,[ArchivedBy]=@AddedBy,[ArchivedOn]=@AddedOn WHERE [ID]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", AddedOn);
                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetProposalDetails(int ClaimID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyUnits_GetProposalDetails";
            cmd.Parameters.AddWithValue("ClaimID", ClaimID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetDuePayments()
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyClaim_GetDuePayments";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetSettlements(Guid PolicyID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyClaim_GetPolicySettlements";
            cmd.Parameters.AddWithValue("@PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
		public DataTable GetLatestInvestmentsClaims()
		{
			DataTable DT = new DataTable();
			SqlConnection connection = new SqlConnection();
			connection.ConnectionString = Database;
			SqlCommand cmd = connection.CreateCommand();
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "PolicyClaims_GetLatestInvestmentClaims"; 
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			da.Fill(DT);
			return DT;
		}
		public DataTable SearchInvestmentsClaims(DateTime StartDate, DateTime EndDate)
		{
			DataTable DT = new DataTable();
			SqlConnection connection = new SqlConnection();
			connection.ConnectionString = Database;
			SqlCommand cmd = connection.CreateCommand();
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "PolicyClaims_SearchInvestmentClaimsByDate";
            cmd.Parameters.AddWithValue("StartDate", StartDate);
            cmd.Parameters.AddWithValue("EndDate", EndDate);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			da.Fill(DT);
			return DT;
		}
		public DataTable DownloadInvestmentsClaims(DateTime StartDate, DateTime EndDate)
		{
			DataTable DT = new DataTable();
			SqlConnection connection = new SqlConnection();
			connection.ConnectionString = Database;
			SqlCommand cmd = connection.CreateCommand();
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "PolicyClaims_DownloadInvestmentClaimsByDate";
			cmd.Parameters.AddWithValue("StartDate", StartDate);
			cmd.Parameters.AddWithValue("EndDate", EndDate);
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			da.Fill(DT);
			return DT;
		}
		private Member MapDataToMember(SqlDataReader reader)
        {
            return new Member
            {
                ID = Convert.ToInt32(reader["ID"]),
                UID = Guid.Parse(reader["UID"].ToString()),
                Name1 = Convert.ToString(reader["Name1"])
            };
        }
        public bool UpdateDeductions(Guid RequestID, decimal Deductions)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Update [PolicyClaims] SET [Deductions]=@Deductions WHERE [RequestID]=@RequestID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.Parameters.AddWithValue("@Deductions", Deductions);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        private void MapParameters(SqlCommand command, PolicyClaim claim)
        {
            command.Parameters.AddWithValue("@ID", claim.ID);
            command.Parameters.AddWithValue("@PolicyID", claim.PolicyID);
            command.Parameters.AddWithValue("@RequestID", claim.RequestID);
            command.Parameters.AddWithValue("@EventID", claim.EventID ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ClaimantID", claim.ClaimantID ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ClaimDate", claim.ClaimDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ClaimTypeID", claim.ClaimTypeID);
            command.Parameters.AddWithValue("@TotalAmount", claim.TotalAmount ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PaymentMethodID", claim.PaymentMethodID ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ValueMode", claim.ValueMode);
            command.Parameters.AddWithValue("@Paid", claim.Paid ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DatePaid", claim.DatePaid ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PaidComment", claim.PaidComment ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@StatusID", claim.StatusID);
            command.Parameters.AddWithValue("@StatusDate", claim.StatusDate);
            command.Parameters.AddWithValue("@StatusComment", claim.StatusComment ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@StatusAddedBy", claim.StatusAddedBy);
            command.Parameters.AddWithValue("@AddedOn", claim.AddedOn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@AddedBy", claim.AddedBy);
        }
    }
}
