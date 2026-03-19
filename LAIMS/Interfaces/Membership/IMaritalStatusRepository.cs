using LAIMS.Models.Membership;

namespace LAIMS.Interfaces.Membership
{
    public interface IMaritalStatusRepository
    {
        List<MaritalStatus> GetAllMaritalStatuses();
        MaritalStatus GetMaritalStatusById(int maritalStatusId);
        void AddMaritalStatus(MaritalStatus maritalStatus);
        void UpdateMaritalStatus(MaritalStatus maritalStatus);
        void DeleteMaritalStatus(int maritalStatusId);
    }
}
