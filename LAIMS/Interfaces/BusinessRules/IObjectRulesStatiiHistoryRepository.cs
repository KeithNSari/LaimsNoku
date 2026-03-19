using LAIMS.Models.BusinessRules;
using System.Data;

namespace LAIMS.Interfaces.BusinessRules
{
    public interface IObjectRulesStatiiHistoryRepository
    {
        List<ObjectRulesStatiiHistory> GetAll();
        ObjectRulesStatiiHistory GetById(long id);
        void Add(ObjectRulesStatiiHistory entity);
        void Update(ObjectRulesStatiiHistory entity);
        DataTable GetHistory(Guid RequestID);
        void Delete(long id);
    }
}
