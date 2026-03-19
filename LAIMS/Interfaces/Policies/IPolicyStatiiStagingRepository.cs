namespace LAIMS.Interfaces.Policies
{
    using LAIMS.Models.Policies;
    using System;
    using System.Collections.Generic;
    using System.Data;

    public interface IPolicyStatiiStagingRepository
    {
        PolicyStatiiStaging GetById(Guid requestId);
        List<PolicyStatiiStaging> GetAll();
        void Add(PolicyStatiiStaging policyStatiiStaging);
        void Update(PolicyStatiiStaging policyStatiiStaging);
        void Delete(Guid requestId);
        void ExecuteStoredProcedure(string procedureName);
        void SetApproved(Guid requestId, byte approved, string approvedBy);
        DataTable GetStatusHistory(Guid RequestID);
        bool UpdatePolicyStatus(Guid RequestID, string ApprovedBy);
    }
}
