using LAIMS.Models.Policies;
using System.Data;

namespace LAIMS.Interfaces.Policies
{
    public interface IPolicyRepository
    {
        void InsertPolicy(Policy policy);
        Policy GetPolicyById(Guid policyId);
        PolicyDates GetPolicyDates(Guid policyId);
        List<Policy> GetAllPolicies();
        DataTable GetMemberPolicies(Guid MemberUID);
        DataTable GetMemberAssociatedPolicies(Guid MemberUID);
        DataTable GetMemberPolicies();
        DataTable GetPoliciesByStatusID(int StatusID);
        DataTable GetPoliciesAwaitingApproval();
        DataTable PoliciesSearchPostApproved(string SearchTerm);
        DataTable PoliciesSearchRiskPolicies(string SearchTerm);
		DataTable PolicyInvestmentSummary(string PolicyNo);
        decimal GetInvestmentContentBalance(string PolicyNo);
        int CountAllPoliciesAwaitingApproval();
        int CountAllMyPoliciesAwaitingApproval(string AddedBy);
        DataTable GetPoliciesAwaitingBeneficiaryUpdatesApproval();
        DataTable PoliciesSearch(string SearchTerm, int StatusID);
        DataTable PoliciesSearchByUser(string SearchTerm, string AddedBy);
        DataTable Search(string SearchTerm);

		DataTable PoliciesSearchByStatusUpdateUser(string SearchTerm, string AddedBy);
        int GetPolicyStatus(Guid ID);
        Guid GetPolicyID(string PolicyNo);
        Guid GetPolicyTypeID(string PolicyNo);
        ApplicationStats GetApplicationStats(string AddedBy);
        DataTable GetLatestApplications(string AddedBy);
        DataTable GetMySubmissions(string AddedBy);
        public DataTable GetMyReviews(string AddedBy);
        public DataTable GetMyWorkQueue(string AddedBy);
        public DataTable SearchMyWorkQueue(string AddedBy, string SearchTerm);
        DataTable GetByLatestStatii();
        DataTable GetRequiredQuestionnaires(Guid PolicyID);
        decimal GetInvestmentContentBalance(Guid PolicyID);
        decimal GetTotalPremiums(Guid PolicyID);
        decimal GetTotalCover(Guid PolicyID);
        decimal GetTotalCover(int CurrencyID,Guid MemberUID);
        void UpdatePolicyTerm(Guid ID, int Term);
        int GetPolicyTerm(Guid ID);
        bool UpdatePolicyStage(Guid ID, int Stage);
        int GetPolicyStage(Guid ID);
        DataTable GetBy(string PolicyNo);
        int GetPolicyCurrency(Guid PolicyID);
        int GetPolicyCurrency(string PolicyNo);
        bool UpdatePolicyStatus(Guid ID, int StatusID);
        bool UpdatePolicyStatus(Guid ID, int Stage, Guid PolicyStatusRuleID, int StatusID, int? StatusReason, string StatusMessage, string AddedBy);
        bool UpdatePolicyStatus(Guid ID, int StatusID, int? StatusReason, string StatusMessage, string AddedBy);
        bool CheckPolicyStatusOwner(Guid ID, int StatusID, string UserID);
        void UpdatePolicy(Policy policy);
        bool UpdatePolicyStatus(Guid ID, int StatusID, string Comment, string AddedBy);
        bool UpdatePolicyStatus(bool OverrideSequence, Guid ID, int Stage, int StatusID, string Comment, string AddedBy);
        bool UpdatePolicyDates(Guid policyId, PolicyDates policyDates);
        DataTable GetPolicyStatusHistory(Guid PolicyID);
        int GetProposerID(string PolicyNo);
        int GetProposerID(Guid PolicyID);
        Guid GetProposerUID(string PolicyNo);
        DataTable GetRequiredQuestionnaires(Guid PolicyID, Guid MemberUID, decimal TotalCover);
        bool CheckExistence(string PolicyNo);
        bool SubmitPolicyApplication(Guid ID, int StatusID, DateTime ApplicationDate, DateTime ProposedStartDate);
        void DebitInvestmentBalance(Guid policyId, decimal TransactionTotal);
        void DeletePolicy(Guid policyId);
        //Policy Servicing
        void PolicyServicingMessagesAdd(Guid PolicyID, Guid MemberUID, int ChangeTypeID, string Message, string AddedBy);
        DataTable PolicyServicingMessagesGet();
        DataTable GetPolicyServicingRequests();
        DataTable SearchPolicyServicingRequests(string SearchTerm);
		DataTable GetEmploymentRecord(Guid PolicyID);
        string GetEmploymentNo(Guid PolicyID);
        void InsertPolicyServicingRequests(Guid PolicyID, Guid RequestID, int StatusID, int ChangeTypeID, string AddedBy, DateTime AddedOn, int Archived);  
        void UpdatePolicyServicingRequests(Guid RequestID, int StatusID);
        int GetPaymentMethod(Guid PolicyID);
    }
}
