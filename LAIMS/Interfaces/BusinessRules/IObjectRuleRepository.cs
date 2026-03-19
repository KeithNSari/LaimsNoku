using LAIMS.Models.BusinessRules;
using System.Data;

namespace LAIMS.Interfaces.BusinessRules
{
    public interface IObjectRuleRepository
    {
        void CreateObjectRule(ObjectRule objectRule);

        ObjectRule GetObjectRuleById(int entryNo);

        List<ObjectRule> GetAllObjectRules();
        DataTable Get(Guid ObjectID);
        void UpdateObjectRule(ObjectRule objectRule);

        void DeleteObjectRule(int entryNo);
    }
}
