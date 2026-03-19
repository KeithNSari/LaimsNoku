using LAIMS.Models.Membership;

namespace LAIMS.Interfaces.Membership
{
    public interface IGenderRepository
    {
        List<Gender> GetAllGenders();
        Gender GetGenderById(int id);
        void AddGender(Gender gender);
        void UpdateGender(Gender gender);
        void DeleteGender(int id);
    }
}
