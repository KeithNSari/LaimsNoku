using LAIMS.Models.Membership;
using System.Data;

namespace LAIMS.Interfaces.Membership
{
    public interface IEmploymentRepository
    {
        List<EmploymentCategory> GetEmploymentCategories();
        List<Designation> GetDesignations();
        List<Employment> GetEmploymentHistory(Guid MemberID);
        List<Member> SearchOrganisations(string searchKeyword);
        Employment GetEmploymentById(Guid id);
        DataTable GetPolicyEmploymentRecord(Guid PolicyID);
        int GetDesignationID(string DesignationName);
        void InsertEmployment(Employment employment);
        void InsertEmployment(Employment employment, Guid PolicyID);
        void InsertByPaymentProvider(Employment employment, int PaymentProviderID, Guid PolicyID);
        void ArchiveEmployment(Guid id);
    }
}
