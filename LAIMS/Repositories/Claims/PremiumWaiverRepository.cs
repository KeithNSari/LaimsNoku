using LAIMS.Interfaces.Claims;
using LAIMS.Models.Claims;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Claims
{
    public class PremiumWaiverRepository : IPremiumWaiverRepository
    {
        private readonly string _database;

        public PremiumWaiverRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _database = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection not configured.");
        }

        public DataTable GetClaimTypeLines(Guid productId)
        {
            using SqlConnection connection = new SqlConnection(_database);
            using SqlCommand cmd = new SqlCommand("ClaimTypeLines_GetByProduct", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ProductID", productId);
            using SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public DataTable GetClaimTypes()
        {
            using SqlConnection connection = new SqlConnection(_database);
            using SqlCommand cmd = new SqlCommand("ClaimTypes_GetMaster", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            using SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public void AddClaimTypeLine(ClaimTypeLine claimTypeLine)
        {
            using SqlConnection connection = new SqlConnection(_database);
            using SqlCommand cmd = new SqlCommand("ClaimTypeLines_Insert", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ProductID", claimTypeLine.ProductID);
            cmd.Parameters.AddWithValue("@ClaimTypeID", claimTypeLine.ClaimTypeID);
            cmd.Parameters.AddWithValue("@AddedBy", claimTypeLine.AddedBy);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public void ArchiveClaimTypeLine(int id, string archivedBy)
        {
            using SqlConnection connection = new SqlConnection(_database);
            using SqlCommand cmd = new SqlCommand("ClaimTypeLines_Archive", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@ArchivedBy", archivedBy);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public DataTable GetDisabilityPremiumWaivers()
        {
            using SqlConnection connection = new SqlConnection(_database);
            using SqlCommand cmd = new SqlCommand("DisabilityPremiumWaivers_GetAll", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            using SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public void AddDisabilityPremiumWaiver(DisabilityPremiumWaiver waiver)
        {
            using SqlConnection connection = new SqlConnection(_database);
            using SqlCommand cmd = new SqlCommand("DisabilityPremiumWaivers_Insert", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RequestID", waiver.RequestID);
            cmd.Parameters.AddWithValue("@PolicyID", waiver.PolicyID);
            cmd.Parameters.AddWithValue("@MemberID", waiver.MemberID);
            cmd.Parameters.AddWithValue("@NotificationDate", waiver.NotificationDate);
            cmd.Parameters.AddWithValue("@EffectiveDate", waiver.EffectiveDate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusID", waiver.StatusID);
            cmd.Parameters.AddWithValue("@Notes", waiver.Notes);
            cmd.Parameters.AddWithValue("@AddedBy", waiver.AddedBy);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public DataTable GetDeathPremiumWaivers()
        {
            using SqlConnection connection = new SqlConnection(_database);
            using SqlCommand cmd = new SqlCommand("DeathPremiumWaivers_GetAll", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            using SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public void AddDeathPremiumWaiver(DeathPremiumWaiver waiver)
        {
            using SqlConnection connection = new SqlConnection(_database);
            using SqlCommand cmd = new SqlCommand("DeathPremiumWaivers_Insert", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RequestID", waiver.RequestID);
            cmd.Parameters.AddWithValue("@PolicyID", waiver.PolicyID);
            cmd.Parameters.AddWithValue("@MemberID", waiver.MemberID);
            cmd.Parameters.AddWithValue("@DeathRecordID", waiver.DeathRecordID);
            cmd.Parameters.AddWithValue("@EffectiveDate", waiver.EffectiveDate);
            cmd.Parameters.AddWithValue("@StatusID", waiver.StatusID);
            cmd.Parameters.AddWithValue("@Notes", waiver.Notes);
            cmd.Parameters.AddWithValue("@AddedBy", waiver.AddedBy);
            connection.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
