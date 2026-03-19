using LAIMS.Models.Claims;
using LAIMS.Models.Membership;
using LAIMS.Models.Utilities;
using System.Data;

namespace LAIMS.Interfaces.Claims
{
    public interface IPolicyClaimRepository
    {
        List<PolicyClaim> GetAllClaims();
        PolicyClaim GetClaimById(int id);
        int GetClaimIDByRequestID(Guid RequestID);
        Guid GetPolicyIDByClaimRequestID(Guid RequestID);
		decimal GetProposedUnits(int ClaimID);
        Guid GetClaimTrust(int ClaimID);
        bool PurchaseMade(int ClaimID);
        decimal AllocationBalance(int ClaimID);// sell units
        decimal AllocationRequestBalance(int ClaimID); //request stage not sale
		int ContractualPartyCheck(int MemberID, Guid PolicyID);
        DataTable GetClaimCover(int ClaimID);
        DataTable GetNonInvestmentSuppementaryCover(string PolicyNo);
        DataTable GetUnpaidPremiums(Guid PolicyID);
        decimal GetUnpaidPremiumsBalance(Guid PolicyID);
        bool CheckProductWaitingPeriod(Guid PolicyID, Guid ProductID);
        DataTable GetExpenses(Guid RequestID);
        decimal GetExpensesTotal(Guid RequestID);
        void AddServiceBreakdown(int ClaimID, int ServiceID, decimal Amount, string AddedBy);
        DataTable GetServiceBreakdown(int ClaimID);
        DataTable GetPolicyClaimants(int ClaimID, int MainCover);
        DataTable GetBeneficiariesByClaimType(Guid PolicyID, int ClaimTypeID);
        DataTable GetDeceasedBeneficiariesByClaimType(Guid PolicyID, int ClaimTypeID);
		DataTable GetDocuments(Guid MemberUID, Guid RequestID);
        int GetSystemDecision(Guid RequestID);
        List<EventType> GetEventTypes();
        DataTable GetMySubmissions(string AddedBy);
        DataTable GetUnReviewed();
        DataTable GetByPolicyID(Guid PolicyID);
        DataTable SearchUnReviewed(string PolicyNo);
        DataTable SearchMySubmissions(string AddedBy, string PolicyNo);
        DataTable GetMyReviews(string AddedBy);
        DataTable SearchMyReviews(string AddedBy, string PolicyNo);
        List<EventTypeCause> GetEventTypeCauses(Guid eventTypeID);
        List<Member> SearchServiceProviders(string SearchTerm);
        List<DeathRecord> GetAllDeathRecords(int PolicyClaimID);
        List<DeathRecord> GetAllDeathRecords(Guid RequestID);
        DataTable GetSubmittedBy(int ClaimID);
        List<ClaimExpense> GetClaimExpensePriceList(Guid RequestID);
        void AddClaim(PolicyClaim claim);
        decimal AllocationTotal(int ClaimID, int PolicyBeneficiariesLineID);
        decimal GetAllocationTotal(Guid RequestID);
        decimal GetDisbursementAmount(Guid RequestID);
        decimal CoverBalance(int ClaimID, int PolicyBeneficiariesLineID);
        decimal UpdateAllocationTotal(Guid RequestID);
        DataTable GetBeneficiaryShares(Guid PolicyID, decimal Balance);
        int UpdateDisbursementAmount(Guid RequestID, decimal DisbursementAmount);
        int AddClaimDocument(Guid RequestID, Guid DocumentID, Guid MediaUploadID, string AddedBy);
        int AddClaimExpense(Guid RequestID, int ClaimTypeExpenseID, string AddedBy);
        int AddClaimUserExpense(Guid RequestID, int ClaimTypeExpenseID, decimal Amount, string AddedBy);
		int AddClaimSystemExpenses(Guid RequestID, string AddedBy);
        int AddClaimCalculatedExpenses(Guid RequestID, string AddedBy);
		void ArchiveExpense(Guid RequestID, string AddedBy);
        void AddClaimant(PolicyClaimant policyClaimant);
        void AddClaimSubmitter(PolicyClaimant policyClaimant);
        int CheckDeathRecordExistence(int MemberID);
		void AddDeathRecord(DeathRecord deathRecord);
        void AddNewDeathRecord(DeathRecord deathRecord);
        void AddDeaths(int MemberID, int PolicyClaimID);
        int CountDeathRecords(Guid PolicyID);
		void UpdateClaim(PolicyClaim claim);
        void UpdateClaimantAmount(int ClaimantID, decimal Amount);
        void UpdateClaimTotal(Guid RequestID, decimal Total);
        bool UpdateStatus(Guid RequestID, int StatusID, string StatusAddedBy);
        bool UpdatePolicyStatus(Guid RequestID, string StatusAddedBy);
		void DeathClaimUpdatePolicyStatus(Guid RequestID);
        void DeleteClaim(int id);
        void ArchiveClaimLine(int ID, string ArchivedBy);
        void ArchiveSubmittedByEntry(int ID, string AddedBy, DateTime AddedOn);
        bool UpdateSignedDate(Guid RequestID, DateTime SignedOn);
        bool UpdateSubmittedBankBranchID(Guid RequestID, int SubmittedBankBranchID);
        bool UpdateInvestmentPolicyStatus(Guid RequestID);
        DateTime GetSignedDate(Guid RequestID);
        int GetSubmittedBankBranch(Guid RequestID);
        DataTable GetProposalDetails(int ClaimID);
        string GetPolicyNo(Guid RequestID);
        Guid GetPolicyTypeID(Guid RequestID);
        int GetMemberID(Guid RequestID);
        DataTable GetMemberDeaths();
        DataTable GetDuePayments();
        DataTable GetSettlements(Guid PolicyID);
        DataTable GetLatestInvestmentsClaims();
        DataTable SearchInvestmentsClaims(DateTime StartDate, DateTime EndDate);
        DataTable DownloadInvestmentsClaims(DateTime StartDate, DateTime EndDate);
		bool UpdateDeductions(Guid RequestID, decimal Deductions);
    }
}
