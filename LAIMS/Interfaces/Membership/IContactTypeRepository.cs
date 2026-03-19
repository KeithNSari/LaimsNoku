using LAIMS.Models.Membership;

namespace LAIMS.Interfaces.Membership
{
    public interface IContactTypeRepository
    {
        void InsertContactType(ContactType contactType);
        List<ContactType> GetAllContactTypes();
        void UpdateContactType(ContactType contactType);
        void DeleteContactType(int contactTypeId);
    }
}
