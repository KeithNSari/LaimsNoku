using LAIMS.Interfaces.BatchJobs;
using LAIMS.Models.Policies;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.BatchJobs
{
    public class JobsRepository: IJobsRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public JobsRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public DataTable GetAllByDateRange(DateTime StartDate, DateTime EndDate)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new ();
            SqlConnection connection = new ();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Jobs_GetAllByDateRange";
            command.Parameters.AddWithValue("@StartDate", StartDate);
            command.Parameters.AddWithValue("@EndDate", EndDate);
            SqlDataAdapter da = new(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetLatest()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new();
            SqlConnection connection = new();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Jobs_GetLatest"; 
            SqlDataAdapter da = new(command);
            da.Fill(DT);
            return DT;
        }
    }
}
