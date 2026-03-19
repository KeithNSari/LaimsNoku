using LAIMS.Models.Membership;

namespace LAIMS.Interfaces.Membership
{
    public interface ITitleRepository
    {
        List<Title> GetAllTitles();
        Title GetTitleById(int titleId);
        void AddTitle(Title title);
        void UpdateTitle(Title title);
        void DeleteTitle(int titleId);
    }
}
