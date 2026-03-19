using LAIMS.Models.Premiums;
using System.Data;

namespace LAIMS.Interfaces.Premiums
{
    public interface IReversalHeaderRepository
    {
        int InsertReversalHeader(ReversalHeader reversalHeader);
        ReversalHeader GetReversalHeaderById(int id);
        List<ReversalHeader> GetAllReversalHeaders();
        DataTable GetReversalHistory();
        void UpdateReversalHeader(ReversalHeader reversalHeader);
        void DeleteReversalHeader(int id);
        void UpdatePolicyStatus(Guid PolicyID);
    }
}
