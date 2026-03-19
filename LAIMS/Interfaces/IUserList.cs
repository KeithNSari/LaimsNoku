using System.Data;
using LAIMS.Models.Utilities;

namespace LAIMS.Interfaces
{
    public interface IUserList
    {
        DataTable GetUsers();
        DataTable GetRoles();
        DataTable GetUserRoles(string UserID);
        DataTable GetUserRoleList(string UserID);
        void UpdateUserPersonalNames(string FirstNames, string Surname, string UserID,int DesignationID, string AddedBy);
        void ClearUserRoles(string UserID);
        void InsertUserRoles(string FullRolesList, string UserID);
        void UpdateUserDesignationRoles(int DesignationID, string UserID);
        UserDetails GetUserDetailsById(string userId);
    }
}
