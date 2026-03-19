using LAIMS.Models.Investments;
using LAIMS.Models.Premiums;
using System.Data;

namespace LAIMS.Interfaces.Premiums
{
    public interface IPolicyBeneficiaryRepository
    {
        int CountPrincipalMembers(Guid PolicyID);
        int InsertPolicyBeneficiary(PolicyBeneficiary beneficiary);
        int InsertPolicyBeneficiaryStaging(PolicyBeneficiary beneficiary, Guid RequestID);
        int ProposeAdditionalLifeAssured(PolicyBeneficiary beneficiary, Guid RequestID);
        int GetRiskGroup(List<int> Parameters);
        int GetPolicyBeneficiaryRiskGroup(int PolicyBeneficiaryID);
		void UpdateRiskGroup(int PolicyBeneficiaryID, int RiskGroup);
        void AddPBLSplit(PBLSplit pblSplit);
        void ArchivePBLSplit(Guid PolicyID, string ArchivedBy);
        void AddPBLSplitStaging(PBLSplit pblSplit, Guid RequestID);
        void UpdatePBLSplitFromStaging(Guid RequestID);
        void ArchivePBLSplitStaging(Guid PolicyID, string ArchivedBy);
        void ProposeBeneficiaryArchive(int PolicyBeneficiaryID, Guid RequestID, string AddedBy);
        decimal GetSplitTotal(Guid PolicyID);
        int CountRelationship(int RelationshipID, Guid PolicyID);
        int GetBeneficiaryID(int MemberID, Guid PolicyID);
        void UpdateBeneficiaryStatus(int PolicyBeneficiaryID, int Beneficiary);
        PolicyBeneficiary GetPolicyBeneficiary(int id);
        DataTable GetBeneficiaryFullDetails(Guid PolicyID);
        DataTable GetBeneficiaryList(Guid PolicyID);
        DataTable GetStagingBeneficiaryList(Guid PolicyID, Guid RequestID);
        DataTable GetAdditionalLifeAssured(Guid PolicyID);
        DataTable GetAdditionalLifeAssured(Guid PolicyID, Guid RequestID);
        void CopyBeneficiaryList(Guid PolicyID, Guid RequestID);
        DataTable GetMainLifeAssured(Guid PolicyID);
        DataTable GetBeneficiaryShares(Guid PolicyID);
        DataTable GetStagingBeneficiaryShares(Guid PolicyID, Guid RequestID);
        DataTable GetRequiredPolicyDocumentsList(Guid PolicyID, int TestedBusiness);
        int GetMemberID(int PolicyBeneficiaryID);
        void UpdatePolicyBeneficiary(PolicyBeneficiary beneficiary);
        void UpdateBeneficiaryListFromCopy(Guid PolicyID, Guid RequestID);
        void DeletePolicyBeneficiary(int id);
    }
}
