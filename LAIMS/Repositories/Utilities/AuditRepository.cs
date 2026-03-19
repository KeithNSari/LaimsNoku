using LAIMS.Interfaces.Utilities;
using LAIMS.Models.Security;
using LAIMS.Models.Utilities;
using Microsoft.Data.SqlClient;
namespace LAIMS.Repositories.Utilities
{
    public class AuditRepository: IAuditRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private readonly string Database;
        public AuditRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public void SaveLoginAudit(LoginAudit audit)
        {
            using (SqlConnection conn = new SqlConnection(Database))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("INSERT INTO LoginAudit (UserID, ActionTime, Success, IPAddress, UserAgent, FailureReason, ActionSource, SessionID, Location,Status, TwoFactorEnabled, TwoFactorMethod) " +
                                                       "VALUES (@UserID, @ActionTime, @Success, @IPAddress, @UserAgent, @FailureReason, @ActionSource, @SessionID, @Location, @Status, @TwoFactorEnabled, @TwoFactorMethod)", conn))
                {
                    // Add parameters
                    cmd.Parameters.AddWithValue("@UserID", audit.UserID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionTime", audit.ActionTime);
                    cmd.Parameters.AddWithValue("@Success", audit.Success);
                    cmd.Parameters.AddWithValue("@IPAddress", audit.IPAddress ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserAgent", audit.UserAgent ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FailureReason", audit.FailureReason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionSource", audit.ActionSource ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SessionID", audit.SessionID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Location", audit.Location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", audit.Status ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TwoFactorEnabled", audit.TwoFactorEnabled.HasValue ? (object)audit.TwoFactorEnabled.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@TwoFactorMethod", audit.TwoFactorMethod ?? (object)DBNull.Value);

                    // Execute the query
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
