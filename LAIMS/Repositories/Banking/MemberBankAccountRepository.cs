using LAIMS.Interfaces.Banking;
using LAIMS.Models.Banking;
using Microsoft.Data.SqlClient;
using System.Data; 

namespace LAIMS.Repositories.Banking
{
    public class MemberBankAccountRepository: IMemberBankAccountRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public MemberBankAccountRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        // Create a new MemberBankAccount record
        public int AddMemberBankAccount(MemberBankAccount memberBankAccount)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"DECLARE @AccountID int=0; SELECT @AccountID=ID FROM MemberBankAccounts WHERE MemberID=@MemberID AND NormalisedBankAccountNo=@NormalisedBankAccountNo AND BankID=@BankID; IF(@AccountID=0) BEGIN INSERT INTO MemberBankAccounts (MemberID,BankID,BranchCode,BankAccountNo,AccountName,NormalisedBankAccountNo,CurrencyID,[Internal]) VALUES (@MemberID,@BankID,@BranchCode,@BankAccountNo,@AccountName,@NormalisedBankAccountNo,@CurrencyID,@Internal);
                         SELECT @AccountID=SCOPE_IDENTITY(); END; SELECT @AccountID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MemberID", memberBankAccount.MemberID);
                    command.Parameters.AddWithValue("@BankID", memberBankAccount.BankID);
                    command.Parameters.AddWithValue("@BankAccountNo", memberBankAccount.BankAccountNo); 
                    command.Parameters.AddWithValue("@AccountName", memberBankAccount.AccountName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BranchCode", memberBankAccount.BranchCode); 
                    command.Parameters.AddWithValue("@NormalisedBankAccountNo", memberBankAccount.BankAccountNo.Replace("-","").Replace(" ","").ToUpper());
                    command.Parameters.AddWithValue("@CurrencyID", memberBankAccount.CurrencyID);
                    command.Parameters.AddWithValue("@Internal", memberBankAccount.IsInternalAccount);
                    command.Parameters.AddWithValue("@AddedBy",memberBankAccount.AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", memberBankAccount.AddedOn); 
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        // Read all MemberBankAccount records
        public List<MemberBankAccount> GetAllMemberBankAccounts()
        {
            List<MemberBankAccount> memberBankAccounts = new List<MemberBankAccount>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM MemberBankAccounts";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            memberBankAccounts.Add(MapReaderToMemberBankAccount(reader));
                        }
                    }
                }
            }

            return memberBankAccounts;
        }
        public DataTable GetInternalAccounts()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT ROW_NUMBER () OVER ( ORDER BY AccountName Asc, BankAccountNo Asc  ) RowNo, [AccountName],[BranchCode],[BankAccountNo],[MemberBankAccounts].[ID],[Currencies].[Name] AS [Currency]  FROM [dbo].[MemberBankAccounts] LEFT JOIN [Currencies] ON [Currencies].[ID]=[MemberBankAccounts].[CurrencyID] WHERE [Internal]=1 AND [MemberBankAccounts].[Archived]=0 ORDER By [AccountName] ASC,[BankAccountNo] ASC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        // Update an existing MemberBankAccount record
        public void UpdateMemberBankAccount(MemberBankAccount memberBankAccount)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"UPDATE MemberBankAccounts
                             SET MemberID = @MemberID, BankAccountNo = @BankAccountNo, Current = @Current
                             WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", memberBankAccount.ID);
                    command.Parameters.AddWithValue("@MemberID", memberBankAccount.MemberID);
                    command.Parameters.AddWithValue("@BankAccountNo", memberBankAccount.BankAccountNo);
                    command.Parameters.AddWithValue("@Current", memberBankAccount.Current);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Delete a MemberBankAccount record
        public void DeleteMemberBankAccount(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM MemberBankAccounts WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void ArchiveMemberAccount(int id, string ArchivedBy, DateTime ArchivedOn)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "Update [dbo].[MemberBankAccounts] SET [Archived]=1,[ArchivedBy]=@ArchivedBy,[ArchivedOn]=@ArchivedOn WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("ArchivedBy", ArchivedBy);
                    command.Parameters.AddWithValue("ArchivedOn", ArchivedOn);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Helper method to map SqlDataReader to MemberBankAccount object
        private MemberBankAccount MapReaderToMemberBankAccount(SqlDataReader reader)
        {
            return new MemberBankAccount
            {
                ID = (int)reader["ID"],
                MemberID = (int)reader["MemberID"],
                BankAccountNo = reader["BankAccountNo"].ToString(),
                Current = (bool)reader["Current"]
                // Add any additional properties as needed
            };
        }
    }
}
