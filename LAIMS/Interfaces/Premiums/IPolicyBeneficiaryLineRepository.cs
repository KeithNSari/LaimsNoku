using LAIMS.Models.Premiums;
using System.Data;

namespace LAIMS.Interfaces.Premiums
{
    public interface IPolicyBeneficiaryLineRepository
    {
        void InsertPolicyBeneficiaryLine(PolicyBeneficiaryLine beneficiaryLine);
        PolicyBeneficiaryLine GetPolicyBeneficiaryLine(int id);
        List<PolicyBeneficiaryLineDetail> GetPolicyBeneficiaryLineDetailsByPolicyID(Guid PolicyID);
        List<PolicyBeneficiaryLineDetail> GetPolicyBeneficiaryLineDetailsByRequestID(Guid PolicyID, Guid RequestID);
        List<PBLDocumentUpload> GetPolicyBeneficiaryLineDocuments(Guid PolicyID);
        void InsertPolicyBeneficiaryLineDocument(int PolicyBeneficiaryLineID, int ProductDocumentID, Guid DocumentID, string AddedBy);
        void ConfirmDocumentUpload(Guid MediaUploadID, Guid PolicyID, Guid MemberUID, Guid DocumentID);
        void UpdatePolicyBeneficiaryLine(PolicyBeneficiaryLine beneficiaryLine);
        void DeletePolicyBeneficiaryLine(int id);
        void ArchiveBeneficiaryRecords(Guid PolicyID, int ID, string ArchivedBy);
        void ArchiveBeneficiaryStagingRecords(Guid PolicyID, int ID, string ArchivedBy);
        void ProposeBeneficiaryLineArchive(int PolicyBeneficiaryLineID, Guid RequestID, string AddedBy);
        void PPDetailsApproveAdditionalComponents(Guid RequestID, string AddedBy);
        DataTable GetRiskPolices(Guid MemberUID);

    }
}
