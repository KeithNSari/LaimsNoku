using System.Data;

namespace LAIMS.Interfaces.BatchJobs
{
    public interface IJobsRepository
    {
        DataTable GetAllByDateRange(DateTime StartDate, DateTime EndDate);
        DataTable GetLatest();
    }
}
