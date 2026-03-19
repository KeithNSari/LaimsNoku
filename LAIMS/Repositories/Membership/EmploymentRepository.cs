using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Transactions;

namespace LAIMS.Repositories.Membership
{
    public class EmploymentRepository: IEmploymentRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public EmploymentRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public List<Designation> GetDesignations()
        {
            List<Designation> designations = new List<Designation>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM Designations ORDER By Designation ASC", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        
                        while (reader.Read())
                        {
                            Designation designation = new()
                            {
                                DesignationName = Convert.ToString(reader["Designation"]),
                                ID = Convert.ToInt32(reader["ID"])
                            };
                            designations.Add(designation);
                        }
                    }
                }
            }
            return designations;
        }
        public List<EmploymentCategory> GetEmploymentCategories()
        {
            List<EmploymentCategory> employmentCategories = new List<EmploymentCategory>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT [Category],[ID] FROM [dbo].[EmploymentCategories] ORDER BY [Category] ASC", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EmploymentCategory employmentCategory = MapDataToEmploymentCategory(reader);
                            employmentCategories.Add(employmentCategory);
                        }
                    }
                }
            }
            return employmentCategories;
        }
        public List<Employment> GetEmploymentHistory(Guid MemberID)
        {
            List<Employment> employments = new List<Employment>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM Employment", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Employment employment = MapToEmployment(reader);
                            employments.Add(employment);
                        }
                    }
                }
            }

            return employments;
        }
        public List<Member> SearchOrganisations(string SearchTerm)
        {
            List<Member> organisations = new List<Member>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open(); 
                string query = "Organisations_Search";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@SearchTerm",SearchTerm);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Member member = MapDataToMember(reader);
                            organisations.Add(member);
                        }
                    }
                }
            }
            return organisations;
        }
        public Employment GetEmploymentById(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT * FROM Employment WHERE ID = @ID", connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToEmployment(reader);
                        }
                    }
                }
            }
            return null;
        }
        public void InsertEmployment(Employment employment)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("INSERT INTO EmploymentRecords (ID, EmployerID, JobTitle, EmploymentNo, CategoryID,AddedBy) VALUES (@EmploymentRecordID, @EmployerID, @JobTitle, @EmploymentNo, @CategoryID,@AddedBy); INSERT [dbo].[EmploymentRecordSalaries]([EmploymentRecordID],[CurrencyID],[GrossSalary],[NetSalary],[AddedBy]) VALUES (@EmploymentRecordID,@CurrencyID,@GrossSalary,@NetSalary,@AddedBy)", connection))
                {
                    SetEmploymentParameters(command, employment);
                    command.ExecuteNonQuery();
                }
            }
        }
        public int GetDesignationID(string DesignationName)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("DECLARE @ID int=-1; SELECT @ID=[ID] FROM [dbo].[Designations] WHERE [Designation]=@Designation; SELECT @ID", connection))
                { 
                    command.Parameters.AddWithValue("@Designation", DesignationName);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public void InsertEmployment(Employment employment, Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("PolicyEmploymentRecord_Add", connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    SetEmploymentParameters(command, employment);
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void InsertByPaymentProvider(Employment employment,int PaymentProviderID, Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("PolicyEmploymentRecord_AddByPaymentProvider", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PaymentProviderID", PaymentProviderID);
                    command.Parameters.AddWithValue("@JobTitle", employment.JobTitle);
                    command.Parameters.AddWithValue("@EmploymentNo", employment.EmploymentNo.Trim());
                    command.Parameters.AddWithValue("@CategoryID", employment.CategoryID);
                    command.Parameters.AddWithValue("@CurrencyID", employment.SalaryCurrencyID);
                    command.Parameters.AddWithValue("@GrossSalary", employment.GrossSalary);
                    command.Parameters.AddWithValue("@NetSalary", employment.NetSalary);
                    command.Parameters.AddWithValue("@AddedBy", employment.AddedBy);
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateEmployment(Employment employment)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("UPDATE EmploymentRecords SET EmployerID = @EmployerID, JobTitle = @JobTitle, EmploymentNo = @EmploymentNo, CategoryID = @CategoryID WHERE ID = @ID", connection))
                {
                    SetEmploymentParameters(command, employment);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void ArchiveEmployment(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("DELETE FROM EmploymentRecords WHERE ID = @ID", connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetPolicyEmploymentRecord(Guid PolicyID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PolicyEmploymentRecord_Get";
            command.Parameters.AddWithValue("@PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        private static Employment MapToEmployment(SqlDataReader reader)
        {
            return new Employment
            {
                ID = (Guid)reader["ID"],
                Employer = reader["Employer"].ToString(),
                JobTitle = reader["JobTitle"].ToString(),
                EmploymentNo = reader["EmploymentNo"].ToString(),
                CategoryID = (Guid)reader["CategoryID"]
            };
        }

        private static void SetEmploymentParameters(SqlCommand command, Employment employment)
        {
            //command.Parameters.AddWithValue("@EmploymentRecordID", employment.ID);
            command.Parameters.AddWithValue("@EmployerID", employment.EmployerID);
            command.Parameters.AddWithValue("@JobTitle", employment.JobTitle);
            command.Parameters.AddWithValue("@EmploymentNo", employment.EmploymentNo.Trim());
            command.Parameters.AddWithValue("@CategoryID", employment.CategoryID);
            command.Parameters.AddWithValue("@CurrencyID", employment.SalaryCurrencyID);
            command.Parameters.AddWithValue("@GrossSalary", employment.GrossSalary);
            command.Parameters.AddWithValue("@NetSalary", employment.NetSalary);
            command.Parameters.AddWithValue("@AddedBy", employment.AddedBy);
        }
        private Member MapDataToMember(SqlDataReader reader)
        {
            return new Member
            {
                ID = Convert.ToInt32(reader["ID"]),
                UID = Guid.Parse(reader["UID"].ToString()), 
                Name1 = Convert.ToString(reader["Name1"]) 
            };
        }
        private EmploymentCategory MapDataToEmploymentCategory(SqlDataReader reader)
        {
            return new EmploymentCategory
            {
                ID = Guid.Parse(reader["ID"].ToString()), 
                Category = Convert.ToString(reader["Category"])
            };
        }
    }
}
