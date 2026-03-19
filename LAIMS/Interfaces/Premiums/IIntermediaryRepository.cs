using LAIMS.Models.Premiums;
using System.Data;

namespace LAIMS.Interfaces.Premiums
{
    public interface IIntermediaryRepository
    {
        // CREATE
        void AddIntermediary(Intermediary intermediary);
        List<IntermediaryImport> FormatIntermediaries(DataTable IntermediariesDT);
        // READ
        bool CheckIntermediaryCode(string Code);
        int GetIntermediaryID(string Code);
        DataTable Get();
        List<Intermediary> GetAllIntermediaries();
        int GetDesignationID(string Designation);
        // UPDATE
        void UpdateIntermediary(Intermediary intermediary);
        void UpdateIntemediarySupervisors(Guid BatchID);
        // DELETE
        void DeleteIntermediary(int intermediaryId);
    }
}
