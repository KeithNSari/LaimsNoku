using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Lifeproducts
{
    public class PolicyTypeLinesBenefitsRepository: IPolicyTypeLinesBenefitsRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public PolicyTypeLinesBenefitsRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
        public void InsertBenefit( PolicyTypeLinesBenefit policyTypeLinesBenefit)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "INSERT INTO [dbo].[PTLBenefits]([PTLID],[TestedBusiness],[WaitingPeriod],[WPDurationUnit],[Benefit],[MaximumBenefit],[Contribution],[AddedBy],[AddedOn]) VALUES(@PTLID,@TestedBusiness,@WaitingPeriod,@WPDurationUnit,@Benefit,@MaximumBenefit,@Contribution,@AddedBy,@AddedOn)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                   AddParameters(command, policyTypeLinesBenefit);
                   command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetBenefits(Guid PolicyDefinitionID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [Products].[Product],[PTLBenefits].[ID],[PTLID],CASE [TestedBusiness] WHEN 0 Then 'Untested' WHEN 1 THEN 'Tested' WHEN 2 THEN 'All' ELSE 'Undefined' END AS [TestedBusiness],[WaitingPeriod],Convert(varchar,[WaitingPeriod]) + ' ' + [Unit] As [DurationUnit],[Benefit],[MaximumBenefit],[Contribution],[PTLBenefits].[AddedOn],[PTLBenefits].[AddedBy] FROM [dbo].[PTLBenefits] LEFT JOIN [DurationUnits] ON [DurationUnits].[ID]= [PTLBenefits].[WPDurationUnit] LEFT JOIN [PolicyTypesLines] ON [PolicyTypesLines].[ID]=[PTLBenefits].[PTLID] LEFT JOIN [Products] On [Products].[ID]=[PolicyTypesLines].[ProductID] WHERE [PTLBenefits].[Deleted]=0 AND [PTLBenefits].[Archived]=0 AND [PolicyTypesLines].[HeaderID]=@PolicyDefinitionID Order By [PTLBenefits].[ID] Asc";
            cmd.Parameters.AddWithValue("PolicyDefinitionID", PolicyDefinitionID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void ArchiveBenefit(int id, string ArchivedBy, DateTime ArchivedOn)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "UPDATE [dbo].[PTLBenefits] SET [Archived]=1,[ArchivedBy]=@ArchivedBy,[ArchivedOn]=@ArchivedOn WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("ArchivedBy", ArchivedBy);
                    command.Parameters.AddWithValue("ArchivedOn", ArchivedOn);
                    command.ExecuteNonQuery();
                }
            }
        }
        private void AddParameters(SqlCommand command,PolicyTypeLinesBenefit policyTypeLinesBenefit)
        {
            command.Parameters.AddWithValue("@PTLID", policyTypeLinesBenefit.PTLID);
            command.Parameters.AddWithValue("@WaitingPeriod", policyTypeLinesBenefit.WaitingPeriod);
            command.Parameters.AddWithValue("@WPDurationUnit", policyTypeLinesBenefit.WPDurationUnit);
            command.Parameters.AddWithValue("@TestedBusiness", policyTypeLinesBenefit.TestedBusiness);
            command.Parameters.AddWithValue("@Benefit", policyTypeLinesBenefit.Benefit);
            command.Parameters.AddWithValue("@MaximumBenefit", policyTypeLinesBenefit.MaximumBenefit);
            command.Parameters.AddWithValue("@Contribution", policyTypeLinesBenefit.Contribution);
            command.Parameters.AddWithValue("@Current", policyTypeLinesBenefit.Current);
            command.Parameters.AddWithValue("@AddedOn", policyTypeLinesBenefit.AddedOn);
            command.Parameters.AddWithValue("@AddedBy", policyTypeLinesBenefit.AddedBy);
        }
    }
}
