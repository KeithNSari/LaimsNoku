using LAIMS.Models.Security;

namespace LAIMS.Interfaces.Utilities
{
    public interface IAuditRepository
    {
        void SaveLoginAudit(LoginAudit audit);
    }
}
