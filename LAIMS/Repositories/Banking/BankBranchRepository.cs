using LAIMS.Interfaces.Banking;
using LAIMS.Models.Banking;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Banking
{
    public class BankBranchRepository: IBankBranchRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public BankBranchRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        // Create a new BankBranch record
        public void AddBankBranch(BankBranch bankBranch)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"INSERT INTO BankBranches (BankID, MemberID, Code, AddedBy, AddedOn, Archived, ArchivedBy, ArchivedOn)
                             VALUES (@BankID, @MemberID, @Code, @AddedBy, @AddedOn, @Archived, @ArchivedBy, @ArchivedOn)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BankID", bankBranch.BankID);
                    command.Parameters.AddWithValue("@MemberID", bankBranch.MemberID);
                    command.Parameters.AddWithValue("@Code", bankBranch.Code);
                    command.Parameters.AddWithValue("@AddedBy", bankBranch.AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", bankBranch.AddedOn);
                    command.Parameters.AddWithValue("@Archived", bankBranch.Archived);
                    command.Parameters.AddWithValue("@ArchivedBy", bankBranch.ArchivedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ArchivedOn", bankBranch.ArchivedOn ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Read all BankBranch records
        public List<BankBranch> GetAllBankBranches()
        {
            List<BankBranch> bankBranches = new List<BankBranch>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT [EntryNo],[BankID],[MemberID],[Members].[Name1] AS [BranchName],[Code] FROM [dbo].[BankBranches] " +
                    "LEFT JOIN [Members] ON [BankBranches].[MemberID]=[Members].[ID] ORDER BY [BranchName] ASC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bankBranches.Add(MapReaderToBankBranch(reader));
                        }
                    }
                }
            }

            return bankBranches;
        }

        // Update an existing BankBranch record
        public void UpdateBankBranch(BankBranch bankBranch)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"UPDATE BankBranches
                             SET BankID = @BankID, Code = @Code, AddedBy = @AddedBy, AddedOn = @AddedOn,
                                 Archived = @Archived, ArchivedBy = @ArchivedBy, ArchivedOn = @ArchivedOn
                             WHERE MemberID = @MemberID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BankID", bankBranch.BankID);
                    command.Parameters.AddWithValue("@MemberID", bankBranch.MemberID);
                    command.Parameters.AddWithValue("@Code", bankBranch.Code);
                    command.Parameters.AddWithValue("@AddedBy", bankBranch.AddedBy);
                    command.Parameters.AddWithValue("@AddedOn", bankBranch.AddedOn);
                    command.Parameters.AddWithValue("@Archived", bankBranch.Archived);
                    command.Parameters.AddWithValue("@ArchivedBy", bankBranch.ArchivedBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ArchivedOn", bankBranch.ArchivedOn ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Delete a BankBranch record
        public void DeleteBankBranch(int memberID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM BankBranches WHERE MemberID = @MemberID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MemberID", memberID);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Helper method to map SqlDataReader to BankBranch object
        private BankBranch MapReaderToBankBranch(SqlDataReader reader)
        {
            return new BankBranch
            {
                EntryNo = (int)reader["EntryNo"],
                BankID = (int)reader["BankID"],
                MemberID = (int)reader["MemberID"],
                BranchName = reader["BranchName"].ToString(),
                Code = reader["Code"].ToString() 
            };
        }
    }
}
