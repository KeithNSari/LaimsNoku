using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Membership;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text.RegularExpressions;

namespace LAIMS.Repositories.Premiums
{
    public class IntermediaryRepository: IIntermediaryRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public IntermediaryRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }
        public void AddIntermediary(Intermediary intermediary)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "INSERT [dbo].[Intermediaries]([BatchID],[IntermediaryTypeID],[MemberID],[ReportsToAgentCode],[Started],[Ended],[DesignationID],[AgentCode],[EmployeeNo])" +
                             "VALUES(@BatchID,@IntermediaryTypeID,@MemberID,@ReportsToAgentCode,@Started,@Ended,@DesignationID,@AgentCode,@EmployeeNo)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@BatchID",intermediary.BatchID);
                    command.Parameters.AddWithValue("@IntermediaryTypeID", intermediary.IntermediaryTypeID);
                    command.Parameters.AddWithValue("@ReportsToAgentCode", intermediary.ReportsToAgentCode);
                    command.Parameters.AddWithValue("@MemberID", intermediary.MemberID); 
                    command.Parameters.AddWithValue("@Started", intermediary.Started ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Ended", intermediary.Ended?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DesignationID", intermediary.DesignationID);
                    command.Parameters.AddWithValue("@AgentCode", intermediary.AgentCode); 
                    command.Parameters.AddWithValue("@EmployeeNo", intermediary.EmployeeNo);
                    command.ExecuteNonQuery();
                }
            }
        }
        public bool CheckIntermediaryCode(string Code)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM [dbo].[Intermediaries] WHERE [Archived]=0 AND [AgentCode]=@AgentCode; SELECT @Count";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AgentCode", Code);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public int GetIntermediaryID(string Code)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @ID int=0; SELECT @ID=[ID] FROM [dbo].[Intermediaries] WHERE [AgentCode]=@AgentCode; SELECT @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AgentCode", Code);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public DataTable Get()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT distinct [Intermediaries].[AgentCode] AS [Agent Code],[Intermediaries].[EmployeeNo] AS [Employee Number],[IntermediaryTypes].[Type] AS [Type of Agent],[Members].[Name1] AS [First Name],[Members].[Name3] AS [Surname],[Members].[NationalID] AS [National ID], I2.AgentCode + '-' + M2.[Name3] + ' ' + M2.[Name1] AS [Reports To], '077' AS [CellPhone]  FROM [dbo].[Intermediaries] LEFT JOIN [IntermediaryTypes] ON [IntermediaryTypes].[ID]=[Intermediaries].[IntermediaryTypeID] LEFT JOIN [Members] ON [Members].[ID]=[Intermediaries].[MemberID] LEFT JOIN [Intermediaries] I2 ON I2.[ID]=[Intermediaries].[ReportsToIntermediaryID] LEFT JOIN [Members] M2 ON [M2].[ID]=I2.[MemberID] ORDER BY [Agent Code] ASC"; 
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public List<IntermediaryImport> FormatIntermediaries(DataTable IntermediariesDT)
        {
            List<IntermediaryImport> intermediaryImports = new List<IntermediaryImport>();
            foreach (DataRow DR in IntermediariesDT.Rows)
            {
                //CheckNulls(DR["First Name"], "First Name");
                //CheckNulls(DR["Surname"], "Surname");
                //CheckNulls(DR["National ID"], "National ID");
                if ((DR["First Name"] == DBNull.Value) || (DR["First Name"] == null)) continue;
                IntermediaryImport intermediaryImport = new IntermediaryImport();
                foreach (DataColumn column in IntermediariesDT.Columns)
                {
                    string columnName = column.ColumnName;
                    object columnValue = DR[columnName];
                    switch (columnName)
                    {
                        case "Employee Number":
                            intermediaryImport.EmployeeNumber = ConvertToString(columnValue);
                            break;
                        case "Agent Code":
                            intermediaryImport.AgentCode = ConvertToString(columnValue);
                            break;
                        case "Designation":
                            intermediaryImport.Designation = ConvertToString(columnValue);
                            break;
                        case "Region":
                            intermediaryImport.Region = ConvertToString(columnValue);
                            break;
                        case "Location":
                            intermediaryImport.Location = ConvertToString(columnValue);
                            break;
                        case "Type of Agent":
                            intermediaryImport.TypeOfAgent = ConvertToString(columnValue);
                            break;
                        case "First Name":
                            intermediaryImport.FirstName = ConvertToString(columnValue);
                            break;
                        case "Middle Name":
                            intermediaryImport.MiddleName = ConvertToString(columnValue);
                            break;
                        case "Surname":
                            intermediaryImport.Surname = ConvertToString(columnValue);
                            break;
                        case "Date Of Appointment":
                            if ((columnValue != DBNull.Value) || (columnValue != null))
                            {
                                DateTime? dateOfAppointment = !string.IsNullOrEmpty(columnValue.ToString()) ? DateTime.Parse(columnValue.ToString()) : (DateTime?)null;
                                intermediaryImport.DateOfAppointment = dateOfAppointment;
                            }
                            break;
                        case "Date of Exit":
                            if ((columnValue != DBNull.Value) || (columnValue != null))
                            {
                                DateTime? dateOfExit = !string.IsNullOrEmpty(columnValue.ToString()) ? DateTime.Parse(columnValue.ToString()) : (DateTime?)null;
                                intermediaryImport.DateOfExit = dateOfExit;
                            }
                            break;
                        case "National ID":
                            intermediaryImport.NationalID = KeepOnlyFirstPattern(ConvertToString(columnValue));
                            break;
                        case "Date Of Birth":
                            if ((columnValue != DBNull.Value) || (columnValue != null))
                            {
                                DateTime? dateOfBirth = !string.IsNullOrEmpty(columnValue.ToString()) ? DateTime.Parse(columnValue.ToString()) : (DateTime?)null;
                                intermediaryImport.DateOfBirth = dateOfBirth;
                            }
                            else
                            {
                                intermediaryImport.DateOfBirth =null;
                            }
                            break;
                        case "Gender":
                            intermediaryImport.Gender = ConvertToString(columnValue);
                            break;
                        case "Title":
                            intermediaryImport.Title = ConvertToString(columnValue);
                            break;
                        case "Address Line 1":
                            intermediaryImport.AddressLine1 = ConvertToString(columnValue);
                            break;
                        case "Address Line 2":
                            intermediaryImport.AddressLine2 = ConvertToString(columnValue);
                            break;
                        case "Address Line 3":
                            intermediaryImport.AddressLine3 = ConvertToString(columnValue);
                            break;
                        case "Details":
                            intermediaryImport.Details = ConvertToString(columnValue);
                            break;
                        case "City":
                            intermediaryImport.City = ConvertToString(columnValue);
                            break;
                        case "Country":
                            intermediaryImport.Country = ConvertToString(columnValue);
                            break;
                        case "Cellphone":
                            intermediaryImport.Cellphone = ConvertToString(columnValue);
                            break;
                        case "Other Cellphone":
                            intermediaryImport.OtherCellphone = ConvertToString(columnValue);
                            break;
                        case "Work Email Address":
                            intermediaryImport.WorkEmailAddress = ConvertToString(columnValue);
                            break;
                        case "Other Email Address":
                            intermediaryImport.OtherEmailAddress = ConvertToString(columnValue);
                            break;
                        case "Branch":
                            intermediaryImport.Branch = ConvertToString(columnValue);
                            break;
                        case "Reports To":
                            intermediaryImport.ReportsTo = ConvertToString(columnValue);
                            break;
                        case "Archived":
                           if((columnValue!=DBNull.Value)|| (columnValue != null)) intermediaryImport.Archived = Convert.ToByte(columnValue);
                            break;
                        case "ArchivedOn":
                            if ((columnValue != DBNull.Value) || (columnValue != null))
                            {
                                DateTime? archivedOn = !string.IsNullOrEmpty(columnValue.ToString()) ? DateTime.Parse(columnValue.ToString()) : (DateTime?)null;
                                intermediaryImport.ArchivedOn = archivedOn;
                            }
                            break;
                        default:
                            // Handle any unexpected column names
                            break;
                    }
                }
                intermediaryImports.Add(intermediaryImport);
            }
            return intermediaryImports;
        }
        private string ConvertToString(object value)
        {
            return value?.ToString() ?? string.Empty;
        }
        private string KeepOnlyFirstPattern(string input)
        {
            // Use regular expression to match and replace all occurrences except the first one
            string pattern = "(P-|Z)";
            string replacement = "";

            return Regex.Replace(input, pattern, m =>
            {
                return m.Index == input.IndexOf(m.Value) ? m.Value : replacement;
            }, RegexOptions.IgnoreCase);
        }
        private void CheckNulls(object input, string fieldName)
        {
            if (input == null)
            {
                throw new Exception(fieldName + " cannot be null!");
            }
        }
        public void UpdateIntemediarySupervisors(Guid BatchID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [dbo].[Intermediaries] SET [Intermediaries].[ReportsToIntermediaryID] = A.[ID] FROM [dbo].[Intermediaries] INNER JOIN (SELECT [ID],[MemberID],[AgentCode] FROM [dbo].[Intermediaries] WHERE BatchID=@BatchID) A ON [Intermediaries].[ReportsToAgentCode]= A.[AgentCode] WHERE [Intermediaries].[BatchID]=@BatchID AND [Intermediaries].[ReportsToAgentCode] !='' AND [Intermediaries].[ReportsToAgentCode] IS NOT NULL";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public List<Intermediary> GetAllIntermediaries()
        {
            List<Intermediary> intermediaries = new List<Intermediary>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "SELECT * FROM Intermediaries";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Intermediary intermediary = MapDataReaderToIntermediary(reader);
                            intermediaries.Add(intermediary);
                        }
                    }
                }
            }

            return intermediaries;
        }
        public int GetDesignationID(string Designation)
        {
            int ID = 0;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @ID int =0; SELECT @ID=[ID] FROM [dbo].[Designations] WHERE [Designation]=@Designation; SELECT @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Designation", Designation);
                    ID = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return ID;
        }
        public void UpdateIntermediary(Intermediary intermediary)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "UPDATE Intermediaries SET IntermediaryTypeID = @IntermediaryTypeID, " +
                             "MemberID = @MemberID, Started = @Started, Ended = @Ended, Current = @Current " +
                             "WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", intermediary.ID);
                    command.Parameters.AddWithValue("@IntermediaryTypeID", intermediary.IntermediaryTypeID);
                    command.Parameters.AddWithValue("@MemberID", intermediary.MemberID);
                    command.Parameters.AddWithValue("@Started", intermediary.Started);
                    command.Parameters.AddWithValue("@Ended", intermediary.Ended);
                    command.Parameters.AddWithValue("@Current", intermediary.Current);

                    command.ExecuteNonQuery();
                }
            }
        }
         
        public void DeleteIntermediary(int intermediaryId)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string sql = "DELETE FROM Intermediaries WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", intermediaryId);

                    command.ExecuteNonQuery();
                }
            }
        } 
        private Intermediary MapDataReaderToIntermediary(SqlDataReader reader)
        {
            return new Intermediary
            {
                ID = (int)reader["ID"],
                IntermediaryTypeID = (int)reader["IntermediaryTypeID"],
                MemberID = (int)reader["MemberID"],
                Started = (DateTime)reader["Started"],
                Ended = (DateTime)reader["Ended"],
                Current = (byte)reader["Current"],
            };
        }
    }
}
