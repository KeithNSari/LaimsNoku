using LAIMS.Models.Membership;

namespace LAIMS.Interfaces.Membership
{
    public interface IRelationshipRepository
    {
        List<Relationship> GetRelationships();
        List<Relationship> GetRelationshipsInclusive();

	}
}
