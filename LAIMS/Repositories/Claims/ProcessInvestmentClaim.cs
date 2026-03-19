namespace LAIMS.Repositories.Claims
{
    public class ProcessInvestmentClaim
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private readonly string Database;

        public ProcessInvestmentClaim(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

    }
}
