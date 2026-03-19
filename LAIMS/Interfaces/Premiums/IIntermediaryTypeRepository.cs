using LAIMS.Models.Premiums;

namespace LAIMS.Interfaces.Premiums
{
    public interface IIntermediaryTypeRepository
    {
        // CREATE
        void AddIntermediaryType(IntermediaryType intermediaryType);

        // READ
        List<IntermediaryType> GetAllIntermediaryTypes();
        int GetTypeID(string TypeName);

        // UPDATE
        void UpdateIntermediaryType(IntermediaryType intermediaryType);

        // DELETE
        void DeleteIntermediaryType(int intermediaryTypeId);
    }
} 
