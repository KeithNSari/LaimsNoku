using LAIMS.Models.Premiums;

namespace LAIMS.Interfaces.Premiums
{
    public interface IReversalLineRepository
    {
        void InsertReversalLine(ReversalLine reversalLine);
        ReversalLine GetReversalLineById(int id);
        List<ReversalLine> GetAllReversalLines();
        void UpdateReversalLine(ReversalLine reversalLine);
        void DeleteReversalLine(int id);
    }
}
