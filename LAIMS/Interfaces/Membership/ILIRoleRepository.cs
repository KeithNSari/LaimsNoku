using LAIMS.Models.Membership;

namespace LAIMS.Interfaces.Membership
{
    public interface ILIRoleRepository
    {
        List<LIRole> GetAllLIRoles();
    }
}
