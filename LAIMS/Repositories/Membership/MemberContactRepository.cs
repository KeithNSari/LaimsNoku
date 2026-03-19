using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace LAIMS.Repositories.Membership
{
    public class MemberContactRepository: IMemberContactRepository
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment; 
        public MemberContactRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        } 
        public void InsertMemberContact(MemberContact memberContact)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"DECLARE @MemberID int; SELECT @MemberID=[ID] FROM [Members] WHERE UID=@MemberUID; 
                    IF(@Preferred=1) BEGIN UPDATE MemberContacts SET Preferred=0 WHERE Preferred=1 AND MemberID=@MemberID END;
                    INSERT INTO MemberContacts (ContactTypeID, MemberID,ContactName,Designation, Line1, Line2, Line3, City, Preferred, AddedBy)
                    VALUES (@ContactTypeID,@MemberID,@ContactName,@Designation, @Line1, @Line2, @Line3, @City, @Preferred, @AddedBy);
                    SELECT SCOPE_IDENTITY();";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ContactTypeID", memberContact.ContactTypeID);
                    command.Parameters.AddWithValue("@MemberUID", memberContact.MemberUID);
                    command.Parameters.AddWithValue("@ContactName", (object)memberContact.ContactName?? string.Empty);
                    command.Parameters.AddWithValue("@Designation", (object)memberContact.Designation ?? string.Empty);
                    command.Parameters.AddWithValue("@Line1", memberContact.Line1);
                    command.Parameters.AddWithValue("@Line2", (object)memberContact.Line2 ?? string.Empty);
                    command.Parameters.AddWithValue("@Line3", (object)memberContact.Line3 ?? string.Empty);
                    command.Parameters.AddWithValue("@City", (object)memberContact.City ?? string.Empty);
                    command.Parameters.AddWithValue("@Preferred", memberContact.Preferred);
                    command.Parameters.AddWithValue("@AddedBy", (object)memberContact.AddedBy ?? string.Empty);
                    connection.Open();
                    memberContact.ID = Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public DataTable Get(Guid UID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "DECLARE @MemberID int=0; SELECT @MemberID=ID FROM [dbo].[Members] WHERE [UID]=@UID; SELECT [MemberContacts].[ID],@UID AS [MemberUID],[ContactTypes].[Type] AS [ContactType],[ContactName],[Line1],[Line2],[Line3],[Cities].[City],[Countries].[Country],CASE [Preferred] WHEN 1 THEN 'Preferred' WHEN 0 THEN '' END AS [Preferred] FROM [dbo].[MemberContacts] LEFT JOIN [ContactTypes] ON [MemberContacts].[ContactTypeID]=[ContactTypes].[ID] LEFT JOIN [Cities] ON [Cities].[ID]=[MemberContacts].[City] LEFT JOIN [Countries] ON [Countries].[CountryID]=[Cities].[CountryID] WHERE [MemberID]=@MemberID And [MemberContacts].[Archived]=0";
            command.Parameters.AddWithValue("@UID", UID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public bool CheckMemberContactExistence(MemberContact memberContact)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @MemberID int=0; SELECT @MemberID=ID FROM [dbo].[Members] WHERE [UID]=@MemberUID; DECLARE @Count int=0; SELECT @Count=COUNT(*) FROM [dbo].[MemberContacts] WHERE [ContactTypeID]=@ContactTypeID AND [ContactName]=@ContactName AND [Line1]=@Line1 AND [Line2]=@Line2 AND [Line3]=@Line3  AND [MemberID]=@MemberID And [Archived]=0; SELECT @Count";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ContactTypeID", memberContact.ContactTypeID);
                    command.Parameters.AddWithValue("@ContactName", memberContact.ContactName);
                    command.Parameters.AddWithValue("@MemberUID", memberContact.MemberUID); 
                    command.Parameters.AddWithValue("@Line1", memberContact.Line1);
                    command.Parameters.AddWithValue("@Line2", (object)memberContact.Line2 ?? string.Empty);
                    command.Parameters.AddWithValue("@Line3", (object)memberContact.Line3 ?? string.Empty);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public List<MemberContact> GetAllMemberContacts(Guid MemberUID)
        {
            List<MemberContact> memberContacts = new List<MemberContact>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "DECLARE @MemberID int=0; SELECT @MemberID=ID FROM [dbo].[Members] WHERE [UID]=@UID; SELECT [MemberContacts].[ID],@UID AS [MemberUID],[ContactTypes].[Type] AS [ContactType],[ContactName],[Designation],[Line1],[Line2],[Line3],[Cities].[City],[Countries].[Country],CASE [Preferred] WHEN 1 THEN 'Preferred' WHEN 0 THEN '' END AS [Preferred] FROM [dbo].[MemberContacts] LEFT JOIN [ContactTypes] ON [MemberContacts].[ContactTypeID]=[ContactTypes].[ID] LEFT JOIN [Cities] ON [Cities].[ID]=[MemberContacts].[City] LEFT JOIN [Countries] ON [Countries].[CountryID]=[Cities].[CountryID] WHERE [MemberID]=@MemberID And [MemberContacts].[Archived]=0";
                using (SqlCommand command = new SqlCommand(query, connection))
                { 
                    command.Parameters.AddWithValue("@UID", MemberUID);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MemberContact memberContact = new MemberContact
                            {
                                ID = (int)reader["ID"],
                                //ContactTypeID = (int)reader["ContactTypeID"],
                                ContactTypeName = reader["ContactType"] as string,
                                //  MemberID = (int)reader["MemberID"],
                                Designation = reader["Designation"] as string,
                                ContactName = reader["ContactName"] as string,
                                Line1 = reader["Line1"].ToString(),
                                Line2 = reader["Line2"] as string,
                                Line3 = reader["Line3"] as string,
                                CityName = reader["City"] as string,
                                CountryName= reader["Country"] as string,
                                PreferredDesc =  reader["Preferred"] as string
                                //AddedOn = reader["AddedOn"] as DateTime?,
                                //AddedBy = reader["AddedBy"] as string
                            };

                            memberContacts.Add(memberContact);
                        }
                    }
                }
            }
            return memberContacts;
        }
        public MemberContact GetMemberContact(Guid MemberUID, int ContactID)
        {
            MemberContact memberContact = new MemberContact();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "DECLARE @MemberID int=0; SELECT @MemberID=ID FROM [dbo].[Members] WHERE [UID]=@MemberUID; SELECT ID, ContactTypeID,Preferred, MemberID,ContactName, Line1, Line2, Line3, City, Preferred, AddedOn, AddedBy FROM MemberContacts Where [ID]=@ID AND [MemberID]=@MemberID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("MemberUID", MemberUID);
                    command.Parameters.AddWithValue("ID", ContactID);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            memberContact.ID = (int)reader["ID"];
                            memberContact.ContactTypeID = (int)reader["ContactTypeID"];
                            memberContact.MemberID = (int)reader["MemberID"];
                            memberContact.ContactName = reader["ContactName"] as string;
                            memberContact.Line1 = reader["Line1"].ToString();
                            memberContact.Line2 = reader["Line2"] as string;
                            memberContact.Line3 = reader["Line3"] as string;
                            memberContact.City = reader["City"] as int?;
                            memberContact.Preferred = (byte)reader["Preferred"];
                            memberContact.AddedOn = reader["AddedOn"] as DateTime?;
                            memberContact.AddedBy = reader["AddedBy"] as string;
                        }
                    }
                }
            }
            return memberContact;
        }
        public void UpdateMemberContact(MemberContact memberContact)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"DECLARE @MemberID int=0; SELECT @MemberID=[ID] FROM [Members] WHERE [UID]=@MemberUID; IF(@Preferred=1) BEGIN UPDATE MemberContacts SET Preferred=0 WHERE Preferred=1 AND MemberID=@MemberID END; UPDATE MemberContacts SET ContactTypeID=@ContactTypeID,ContactName=@ContactName,Line1=@Line1, Line2=@Line2,Line3=@Line3, City=@City, Preferred=@Preferred, AddedBy=@AddedBy WHERE ID=@ID AND MemberID=@MemberID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", memberContact.ID);
                    command.Parameters.AddWithValue("@ContactTypeID", memberContact.ContactTypeID);
                    command.Parameters.AddWithValue("@MemberUID", memberContact.MemberUID);
                    command.Parameters.AddWithValue("@ContactName", (object)memberContact.ContactName ?? string.Empty);
                    command.Parameters.AddWithValue("@Line1", memberContact.Line1);
                    command.Parameters.AddWithValue("@Line2", (object)memberContact.Line2 ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Line3", (object)memberContact.Line3 ?? DBNull.Value);
                    command.Parameters.AddWithValue("@City", (object)memberContact.City ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Preferred", memberContact.Preferred);
                    command.Parameters.AddWithValue("@AddedBy", (object)memberContact.AddedBy ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        public void ArchiveMemberContact(Guid MemberUID, int ContactID, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = @"DECLARE @MemberID int=0; SELECT @MemberID=[ID] FROM [Members] WHERE [UID]=@MemberUID; UPDATE MemberContacts SET Archived=1,ArchivedBy=@AddedBy,ArchivedOn=@ArchivedOn WHERE ID=@ID AND MemberID=@MemberID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("MemberUID", MemberUID);
                    command.Parameters.AddWithValue("ID", ContactID);
                    command.Parameters.AddWithValue("AddedBy", AddedBy);
                    command.Parameters.AddWithValue("ArchivedOn", DateTime.Now);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeleteMemberContact(int memberContactId)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                string query = "DELETE FROM MemberContacts WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", memberContactId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
