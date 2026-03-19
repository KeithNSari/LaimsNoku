using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Commissions
{
    public class ProcessCommissions
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public ProcessCommissions(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        
    }
}
