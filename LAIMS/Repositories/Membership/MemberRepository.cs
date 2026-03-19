using LAIMS.Interfaces.Membership;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Membership;
using LAIMS.Models.Premiums;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace LAIMS.Repositories.Membership
{
    public class MemberRepository: IMemberRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public MemberRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public List<Member> GetAllMembers()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<Member> members = new List<Member>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM Members", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Member member = MapDataToMember(reader);
                            members.Add(member);
                        }
                    }
                }
            }

            return members;
        }

        public Member GetMemberById(Guid memberId)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT [Members].[ID],[Members].[UID],[MemberNo],[IsOrganisation],CASE [IsOrganisation] WHEN 1 Then 'Organisation' WHEN 0 THEN 'Person' END  AS [Type],[Name1],[Name2],[Name3],[GenderID],[Genders].[Name] AS [Gender],[Members].[TitleID],[Titles].[Title],[Members].[MaritalStatusID],[MaritalStatus],[Members].[CountryID],A.[Country],[DOB],[BirthCountryID],B.[Country] AS [BirthCountry],[PlaceOfBirth],[NationalID],[BirthCertificate],[Passport],[AddedOn],[AddedBy] FROM [dbo].[Members] LEFT JOIN [Genders] ON [Genders].[Id]=[Members].[GenderID] LEFT JOIN [Titles] ON [Titles].[TitleID]=[Members].[TitleID] LEFT JOIN [Countries] A ON A.[CountryID]=[Members].[CountryID] LEFT JOIN [Countries] B On B.[CountryID]=[Members].[BirthCountryID] LEFT JOIN [MaritalStatii] On [MaritalStatii].[MaritalStatusID]=[Members].[MaritalStatusID] WHERE [Members].[UID]=@ID", connection))
                {
                    command.Parameters.AddWithValue("@ID", memberId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataToMember(reader);
                        }
                    }
                }
            }
            return null;
        }
        public Member GetStagingMemberById(Guid memberId)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("DECLARE @LastRecord int; SELECT @LastRecord=MAX([ID]) FROM MembersStaging WHERE [UID]=@UID; SELECT [MembersStaging].[ID],[MembersStaging].[UID],[MemberNo],[IsOrganisation],CASE [IsOrganisation] WHEN 1 Then 'Organisation' WHEN 0 THEN 'Person' END  AS [Type],[Name1],[Name2],[Name3],[GenderID],[Genders].[Name] AS [Gender],[MembersStaging].[TitleID],[Titles].[Title],[MembersStaging].[MaritalStatusID],[MaritalStatus],[MembersStaging].[CountryID],A.[Country],[DOB],[BirthCountryID],B.[Country] AS [BirthCountry],[PlaceOfBirth],[NationalID],[BirthCertificate],[Passport],[AddedOn],[AddedBy] FROM [dbo].[MembersStaging] LEFT JOIN [Genders] ON [Genders].[Id]=[MembersStaging].[GenderID] LEFT JOIN [Titles] ON [Titles].[TitleID]=[MembersStaging].[TitleID] LEFT JOIN [Countries] A ON A.[CountryID]=[MembersStaging].[CountryID] LEFT JOIN [Countries] B On B.[CountryID]=[MembersStaging].[BirthCountryID] LEFT JOIN [MaritalStatii] On [MaritalStatii].[MaritalStatusID]=[MembersStaging].[MaritalStatusID] WHERE [MembersStaging].[ID]=@LastRecord", connection))
                {
                    command.Parameters.AddWithValue("@UID", memberId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataToMember(reader);
                        }
                    }
                }
            }
            return null;
        }
        public Member GetMemberById(string ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT [Members].[ID],[Members].[UID],[MemberNo],[IsOrganisation],CASE [IsOrganisation] WHEN 1 Then 'Organisation' WHEN 0 THEN 'Person' END  AS [Type],[Name1],[Name2],[Name3],[GenderID],[Genders].[Name] AS [Gender],[Members].[TitleID],[Titles].[Title],[Members].[MaritalStatusID],[MaritalStatus],[Members].[CountryID],A.[Country],[DOB],[BirthCountryID],B.[Country] AS [BirthCountry],[PlaceOfBirth],[NationalID],[BirthCertificate],[Passport],[AddedOn],[AddedBy] FROM [dbo].[Members] LEFT JOIN [Genders] ON [Genders].[Id]=[Members].[GenderID] LEFT JOIN [Titles] ON [Titles].[TitleID]=[Members].[TitleID] LEFT JOIN [Countries] A ON A.[CountryID]=[Members].[CountryID] LEFT JOIN [Countries] B On B.[CountryID]=[Members].[BirthCountryID] LEFT JOIN [MaritalStatii] On [MaritalStatii].[MaritalStatusID]=[Members].[MaritalStatusID] WHERE ([Members].[NationalID]=@ID) OR ([Members].[Passport]=@ID) OR ([Members].[BirthCertificate]=@ID)", connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataToMember(reader);
                        }
                    }
                }
            }
            return null;
        }
        public Guid GetOrganisationIDByName(string OrganisationName)
        {
            Guid OrgID= Guid.Empty;
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("DECLARE @OrgID uniqueidentifier='00000000-0000-0000-0000-000000000000'; SELECT @OrgID=[Members].[UID] FROM [dbo].[Members] WHERE ([Members].[Name1]=@Name1) OR ([Members].[NormalisedName1Name3]=@NormalisedName1Name3) AND ([IsOrganisation]=1); SELECT @OrgID", connection))
                {
                    command.Parameters.AddWithValue("@Name1", OrganisationName);
                    command.Parameters.AddWithValue("@NormalisedName1Name3", OrganisationName.ToUpper().Replace("-", "").Replace(" ", ""));
                    string response = command.ExecuteScalar().ToString();
                    OrgID = Guid.Parse(response); 
                }
            }
            return OrgID;
        }
        public Member GetMemberById(int ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT [Members].[ID],[Members].[UID],[MemberNo],[IsOrganisation],CASE [IsOrganisation] WHEN 1 Then 'Organisation' WHEN 0 THEN 'Person' END  AS [Type],[Name1],[Name2],[Name3],[GenderID],[Genders].[Name] AS [Gender],[Members].[TitleID],[Titles].[Title],[Members].[MaritalStatusID],[MaritalStatus],[Members].[CountryID],A.[Country],[DOB],[BirthCountryID],B.[Country] AS [BirthCountry],[PlaceOfBirth],[NationalID],[BirthCertificate],[Passport],[AddedOn],[AddedBy] FROM [dbo].[Members] LEFT JOIN [Genders] ON [Genders].[Id]=[Members].[GenderID] LEFT JOIN [Titles] ON [Titles].[TitleID]=[Members].[TitleID] LEFT JOIN [Countries] A ON A.[CountryID]=[Members].[CountryID] LEFT JOIN [Countries] B On B.[CountryID]=[Members].[BirthCountryID] LEFT JOIN [MaritalStatii] On [MaritalStatii].[MaritalStatusID]=[Members].[MaritalStatusID] WHERE [Members].[ID]=@ID", connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataToMember(reader);
                        }
                    }
                }
            }
            return null;
        }
        public Member GetStagingMemberById(int ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT [Members].[ID],[Members].[UID],[MemberNo],[IsOrganisation],CASE [IsOrganisation] WHEN 1 Then 'Organisation' WHEN 0 THEN 'Person' END  AS [Type],[Name1],[Name2],[Name3],[GenderID],[Genders].[Name] AS [Gender],[Members].[TitleID],[Titles].[Title],[Members].[MaritalStatusID],[MaritalStatus],[Members].[CountryID],A.[Country],[DOB],[BirthCountryID],B.[Country] AS [BirthCountry],[PlaceOfBirth],[NationalID],[BirthCertificate],[Passport],[AddedOn],[AddedBy] FROM [dbo].[MembersStaging] LEFT JOIN [Genders] ON [Genders].[Id]=[Members].[GenderID] LEFT JOIN [Titles] ON [Titles].[TitleID]=[Members].[TitleID] LEFT JOIN [Countries] A ON A.[CountryID]=[Members].[CountryID] LEFT JOIN [Countries] B On B.[CountryID]=[Members].[BirthCountryID] LEFT JOIN [MaritalStatii] On [MaritalStatii].[MaritalStatusID]=[Members].[MaritalStatusID] WHERE [Members].[ID]=@ID", connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataToMember(reader);
                        }
                    }
                }
            }
            return null;
        }
        public int GetID(Guid UID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            int ID;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @ID int=-1; SELECT @ID=[ID] FROM [Members] WHERE [UID]=@UID; SELECT @ID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UID", UID); 
                    ID = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return ID;
        }
        public Guid GetUID(int ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            Guid UID;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "DECLARE @UID uniqueidentifier; SELECT @UID=[UID] FROM [Members] WHERE [ID]=@ID; SELECT @UID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    UID = Guid.Parse(command.ExecuteScalar().ToString());
                }
            }
            return UID;
        }
        public bool ConfirmID(Guid ID,string NationalID,string PassPort, string BirthCertificate)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int =0; SELECT @Count=Count(*) FROM [dbo].[Members] WHERE [Members].[NationalID]=@NationalID AND [BirthCertificate]=@BirthCertificate AND [Passport]=@Passport AND [UID]=@ID; IF(@COUNT>0) BEGIN UPDATE [dbo].[Members] SET [Confirmed]=1 WHERE [UID]=@ID; END; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NationalID", NationalID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BirthCertificate", BirthCertificate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Passport", PassPort ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ID", ID);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public bool CheckIDConfirm(Guid ID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Confirmed int =0; SELECT @Confirmed=Confirmed FROM [dbo].[Members] WHERE [UID]=@ID; SELECT @Confirmed";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public bool CheckOther(Guid ID, string NationalID, string PassPort, string BirthCertificate)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int =0; SELECT @Count=Count(*) FROM [dbo].[Members] WHERE ([Members].[NationalID]=@NationalID OR [BirthCertificate]=@BirthCertificate OR [Passport]=@Passport) AND [UID]!=@ID; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NationalID", NationalID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BirthCertificate", BirthCertificate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Passport", PassPort ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ID", ID);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public bool CheckOtherNationalID(Guid ID, string NationalID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int =0; SELECT @Count=Count(*) FROM [dbo].[Members] WHERE ([Members].[NationalID]=@NationalID) AND [UID]!=@ID; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NationalID", NationalID); 
                    command.Parameters.AddWithValue("@ID", ID);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public bool CheckOtherBirthCertificate(Guid ID, string BirthCertificate)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int =0; SELECT @Count=Count(*) FROM [dbo].[Members] WHERE ([Members].[BirthCertificate]=@BirthCertificate) AND [UID]!=@ID; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BirthCertificate", BirthCertificate);
                    command.Parameters.AddWithValue("@ID", ID);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public bool CheckOtherPassport(Guid ID, string Passport)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @Count int =0; SELECT @Count=Count(*) FROM [dbo].[Members] WHERE ([Members].[Passport]=@Passport) AND [UID]!=@ID; SELECT @COUNT";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Passport", Passport);
                    command.Parameters.AddWithValue("@ID", ID);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public bool CheckNationalIDExistence(string NationalID)
		{
			var Database = _configuration.GetConnectionString("DefaultConnection");
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM [dbo].[Members] WHERE ([NormalisedNationalID]=@NormalisedNationalID); SELECT @COUNT";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@NormalisedNationalID", NationalID.ToUpper().Replace("-", "").Replace(" ", ""));
				  	return Convert.ToBoolean(command.ExecuteScalar());
				}
			}
		}
		public bool CheckPassportExistence(string Passport)
		{
			var Database = _configuration.GetConnectionString("DefaultConnection");
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM [dbo].[Members] WHERE ([NormalisedPassport]=@NormalisedPassport); SELECT @COUNT";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@NormalisedPassport", Passport.ToUpper().Replace("-", "").Replace(" ", ""));
					return Convert.ToBoolean(command.ExecuteScalar());
				}
			}
		}
		public bool CheckBirthCertificateExistence(string BirthCertificate)
		{
			var Database = _configuration.GetConnectionString("DefaultConnection");
			using (SqlConnection connection = new SqlConnection(Database))
			{
				connection.Open();
				string query = "DECLARE @Count int=0; SELECT @Count=Count(*) FROM [dbo].[Members] WHERE ([NormalisedBirthCertificate]=@NormalisedBirthCertificate); SELECT @COUNT";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@NormalisedBirthCertificate", BirthCertificate.ToUpper().Replace("-", "").Replace(" ", ""));
				 	return Convert.ToBoolean(command.ExecuteScalar());
				}
			}
		} 
        public int CheckMemberNameExistence(string MemberName)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @ID int=-1; SELECT @ID=[ID] FROM [dbo].[Members] WHERE ([NormalisedName1Name3]=@NormalisedName1Name3); SELECT @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NormalisedName1Name3", (MemberName).ToUpper().Replace("-", "").Replace(" ", ""));
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public bool SetConfirmation(Guid ID, int Status)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Members] SET [Confirmed]=@Confirmed WHERE [UID]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                { 
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@Confirmed", Status);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        } 
        public void AddMember(Member member)
        {
            string NormalisedNationalID = string.Empty;
            if (member.NationalID != null)
            {
                NormalisedNationalID = member.NationalID.ToUpper().Replace("-", "").Replace(" ", "");
            }
            string NormalisedBirthCertificate = string.Empty;
            if(member.BirthCertificate != null)
            {
                NormalisedBirthCertificate = member.BirthCertificate.ToUpper().Replace("-", "").Replace(" ", "");
            }
            string NormalisedPassport=string.Empty;
            if(member.Passport != null)
            {
                NormalisedPassport = member.Passport.ToUpper().Replace("-", "").Replace(" ", "");
            }
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(
					"INSERT INTO Members (BatchID,UID,IsOrganisation, Name1, Name2, Name3,NormalisedName1Name3, GenderID, TitleID, MaritalStatusID, CountryID,BirthCountryID,DOB, PlaceOfBirth, NationalID,NormalisedNationalID, BirthCertificate,NormalisedBirthCertificate,Passport,NormalisedPassport,MemberNo, AddedOn, AddedBy) " +
					"VALUES (@BatchID,@UID,@IsOrganisation, @Name1, @Name2, @Name3,@NormalisedName1Name3, @GenderID, @TitleID, @MaritalStatusID, @CountryID,@BirthCountryID, @DOB, @PlaceOfBirth, @NationalID,@NormalisedNationalID,@BirthCertificate,@NormalisedBirthCertificate,@Passport,@NormalisedPassport,@MemberNo, @AddedOn, @AddedBy); " +
                    "SELECT SCOPE_IDENTITY();", connection))
                {
                    command.Parameters.AddWithValue("@BatchID", member.BatchID);
                    command.Parameters.AddWithValue("@UID", member.UID); 
                    command.Parameters.AddWithValue("@IsOrganisation", member.IsOrganisation);
                    command.Parameters.AddWithValue("@Name1", member.Name1);
                    command.Parameters.AddWithValue("@Name2", member.Name2 ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Name3", member.Name3 ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@GenderID", member.GenderID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@NormalisedName1Name3", (member.Name1 + member.Name3).ToUpper().Replace("-", "").Replace(" ", ""));
					command.Parameters.AddWithValue("@TitleID", member.TitleID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MaritalStatusID", member.MaritalStatusID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CountryID", member.CountryID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BirthCountryID", member.BirthCountryID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DOB", member.DOB ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PlaceOfBirth", member.PlaceOfBirth ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@NationalID", member.NationalID ?? string.Empty);
                    command.Parameters.AddWithValue("@NormalisedNationalID", NormalisedNationalID);
                    command.Parameters.AddWithValue("@BirthCertificate", member.BirthCertificate ?? string.Empty);
                    command.Parameters.AddWithValue("@NormalisedBirthCertificate", NormalisedBirthCertificate);
                    command.Parameters.AddWithValue("@Passport", member.Passport ?? string.Empty);
                    command.Parameters.AddWithValue("@NormalisedPassport", NormalisedPassport);
                    command.Parameters.AddWithValue("@MemberNo", member.MemberNo ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedOn", member.AddedOn ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", member.AddedBy ?? (object)DBNull.Value);

                    int newMemberId = Convert.ToInt32(command.ExecuteScalar());
                    member.ID = newMemberId;
                }
            }
        }
        public int GetGender(int MemberID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            Guid UID;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "SELECT [GenderID] FROM [dbo].[Members] WHERE [ID]=@MemberID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MemberID", MemberID);
                    return Convert.ToInt32(command.ExecuteScalar().ToString());
                }
            } 
        }
        public void UpdateMember(Member member)
        {
            string NormalisedNationalID = string.Empty;
            if (member.NationalID != null)
            {
                NormalisedNationalID = member.NationalID.ToUpper().Replace("-", "").Replace(" ", "");
            }
            string NormalisedBirthCertificate = string.Empty;
            if (member.BirthCertificate != null)
            {
                NormalisedBirthCertificate = member.BirthCertificate.ToUpper().Replace("-", "").Replace(" ", "");
            }
            string NormalisedPassport = string.Empty;
            if (member.Passport != null)
            {
                NormalisedPassport = member.Passport.ToUpper().Replace("-", "").Replace(" ", "");
            }
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(
                    "UPDATE Members SET Name1 = @Name1, Name2 = @Name2, Name3 = @Name3, GenderID = @GenderID, TitleID = @TitleID, MaritalStatusID = @MaritalStatusID, CountryID = @CountryID,BirthCountryID=@BirthCountryID,MemberNo=@MemberNo, " +
                    "DOB = @DOB, PlaceOfBirth = @PlaceOfBirth, NationalID = @NationalID,NormalisedNationalID=@NormalisedNationalID,BirthCertificate=@BirthCertificate,NormalisedBirthCertificate=@NormalisedBirthCertificate,Passport = @Passport,NormalisedPassport=@NormalisedPassport, AddedOn = @AddedOn, AddedBy = @AddedBy " +
                    "WHERE UID = @UID", connection))
                {
                    command.Parameters.AddWithValue("@ID", member.ID);
                    command.Parameters.AddWithValue("@UID", member.UID); 
                    command.Parameters.AddWithValue("@Name1", member.Name1);
                    command.Parameters.AddWithValue("@Name2", member.Name2 ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Name3", member.Name3 ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@GenderID", member.GenderID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TitleID", member.TitleID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MaritalStatusID", member.MaritalStatusID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CountryID", member.CountryID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BirthCountryID", member.BirthCountryID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DOB", member.DOB ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PlaceOfBirth", member.PlaceOfBirth ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@NationalID", member.NationalID ?? string.Empty);
                    command.Parameters.AddWithValue("@NormalisedNationalID", NormalisedNationalID);
                    command.Parameters.AddWithValue("@BirthCertificate", member.BirthCertificate ?? string.Empty);
                    command.Parameters.AddWithValue("@NormalisedBirthCertificate", NormalisedBirthCertificate);
                    command.Parameters.AddWithValue("@Passport", member.Passport ?? string.Empty);
                    command.Parameters.AddWithValue("@NormalisedPassport", NormalisedPassport);
                    command.Parameters.AddWithValue("@MemberNo", member.MemberNo ?? (object)DBNull.Value); 
                    command.Parameters.AddWithValue("@AddedOn", member.AddedOn ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", member.AddedBy ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateMemberStaging(Member member)
        {
            string NormalisedNationalID = string.Empty;
            if (member.NationalID != null)
            {
                NormalisedNationalID = member.NationalID.ToUpper().Replace("-", "").Replace(" ", "");
            }
            string NormalisedBirthCertificate = string.Empty;
            if (member.BirthCertificate != null)
            {
                NormalisedBirthCertificate = member.BirthCertificate.ToUpper().Replace("-", "").Replace(" ", "");
            }
            string NormalisedPassport = string.Empty;
            if (member.Passport != null)
            {
                NormalisedPassport = member.Passport.ToUpper().Replace("-", "").Replace(" ", "");
            }
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(
                    "DECLARE @LastRecord int; SELECT @LastRecord=MAX([ID]) FROM MembersStaging WHERE [UID]=@UID; UPDATE MembersStaging SET Name1 = @Name1, Name2 = @Name2, Name3 = @Name3, GenderID = @GenderID, TitleID = @TitleID, MaritalStatusID = @MaritalStatusID, CountryID = @CountryID,BirthCountryID=@BirthCountryID,MemberNo=@MemberNo, " +
                    "DOB = @DOB, PlaceOfBirth = @PlaceOfBirth, NationalID = @NationalID,NormalisedNationalID=@NormalisedNationalID,BirthCertificate=@BirthCertificate,NormalisedBirthCertificate=@NormalisedBirthCertificate,Passport = @Passport,NormalisedPassport=@NormalisedPassport " +
                    "WHERE ID=@LastRecord", connection))
                { 
                    command.Parameters.AddWithValue("@UID", member.UID);
                    command.Parameters.AddWithValue("@Name1", member.Name1);
                    command.Parameters.AddWithValue("@Name2", member.Name2 ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Name3", member.Name3 ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@GenderID", member.GenderID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@TitleID", member.TitleID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MaritalStatusID", member.MaritalStatusID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CountryID", member.CountryID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BirthCountryID", member.BirthCountryID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DOB", member.DOB ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PlaceOfBirth", member.PlaceOfBirth ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@NationalID", member.NationalID ?? string.Empty);
                    command.Parameters.AddWithValue("@NormalisedNationalID", NormalisedNationalID);
                    command.Parameters.AddWithValue("@BirthCertificate", member.BirthCertificate ?? string.Empty);
                    command.Parameters.AddWithValue("@NormalisedBirthCertificate", NormalisedBirthCertificate);
                    command.Parameters.AddWithValue("@Passport", member.Passport ?? string.Empty);
                    command.Parameters.AddWithValue("@NormalisedPassport", NormalisedPassport);
                    command.Parameters.AddWithValue("@MemberNo", member.MemberNo ?? (object)DBNull.Value);  
                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeleteMember(int memberId)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("DELETE FROM Members WHERE ID = @ID", connection))
                {
                    command.Parameters.AddWithValue("@ID", memberId);

                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetMyEntries(string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT TOP 50 [Members].[ID],[Members].[UID],CASE [IsOrganisation] WHEN 0 Then 'Organisation' WHEN 1 THEN 'Person' END  AS [Type],[Name1],[Name2],[Name3],[GenderID],[Genders].[Name] AS [Gender],[Members].[TitleID],[Titles].[Title],[Members].[MaritalStatusID],[MaritalStatus],[Members].[CountryID],[Country],[DOB],[PlaceOfBirth],[NationalID],[BirthCertificate],[Passport],[AddedOn],[AddedBy] FROM [dbo].[Members] LEFT JOIN [Genders] ON [Genders].[Id]=[Members].[GenderID] LEFT JOIN [Titles] ON [Titles].[TitleID]=[Members].[TitleID] LEFT JOIN [Countries] ON [Countries].[CountryID]=[Members].[CountryID] LEFT JOIN [MaritalStatii] On [MaritalStatii].[MaritalStatusID]=[Members].[MaritalStatusID] WHERE [AddedBy]=@AddedBy AND [IsOrganisation]=0 ORDER BY [Members].[ID] DESC";
            command.Parameters.AddWithValue("AddedBy", AddedBy);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public List<Member> SearchOrganisations(string SearchTerm)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<Member> organisations = new List<Member>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Organisations_Search";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@SearchTerm", SearchTerm);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Member member = MapDataToOrganisation(reader);
                            organisations.Add(member);
                        }
                    }
                }
            }
            return organisations;
        }
        public DataTable Search(string SearchTerm)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Members_Search";
            command.Parameters.AddWithValue("SearchTerm", SearchTerm);  
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable SearchOrganisations2(string SearchTerm)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Organisations_Search2";
            command.Parameters.AddWithValue("SearchTerm", SearchTerm);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetOrganisation(Guid UID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Organisations_Get";
            command.Parameters.AddWithValue("UID", UID);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetLatestOrganisations()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Organisations_GetLatest"; 
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        private Member MapDataToMember(SqlDataReader reader)
        {
            return new Member
            {
                ID = Convert.ToInt32(reader["ID"]),
                UID=Guid.Parse(reader["UID"].ToString()),
                IsOrganisation = Convert.ToByte(reader["IsOrganisation"]),
                Name1 = Convert.ToString(reader["Name1"]),
                Name2 = Convert.IsDBNull(reader["Name2"]) ? null : Convert.ToString(reader["Name2"]),
                Name3 = Convert.IsDBNull(reader["Name3"]) ? null : Convert.ToString(reader["Name3"]),
                GenderID = Convert.IsDBNull(reader["GenderID"]) ? (int?)null : Convert.ToInt32(reader["GenderID"]),
                Gender = Convert.IsDBNull(reader["Gender"]) ? null : Convert.ToString(reader["Gender"]),
                TitleID = Convert.IsDBNull(reader["TitleID"]) ? (int?)null : Convert.ToInt32(reader["TitleID"]),
                Title = Convert.IsDBNull(reader["Title"]) ? null : Convert.ToString(reader["Title"]),
                MaritalStatusID = Convert.IsDBNull(reader["MaritalStatusID"]) ? (int?)null : Convert.ToInt32(reader["MaritalStatusID"]),
                MaritalStatus = Convert.IsDBNull(reader["MaritalStatus"]) ? null : Convert.ToString(reader["MaritalStatus"]),
                CountryID = Convert.IsDBNull(reader["CountryID"]) ? (int?)null : Convert.ToInt32(reader["CountryID"]),
                Country = Convert.IsDBNull(reader["Country"]) ? null : Convert.ToString(reader["Country"]),
                BirthCountryID = Convert.IsDBNull(reader["BirthCountryID"]) ? (int?)null : Convert.ToInt32(reader["BirthCountryID"]),
                BirthCountry = Convert.IsDBNull(reader["BirthCountry"]) ? null : Convert.ToString(reader["BirthCountry"]),
                DOB = Convert.IsDBNull(reader["DOB"]) ? (DateTime?)null : Convert.ToDateTime(reader["DOB"]),
                PlaceOfBirth = Convert.IsDBNull(reader["PlaceOfBirth"]) ? null : Convert.ToString(reader["PlaceOfBirth"]),
                NationalID = Convert.IsDBNull(reader["NationalID"]) ? null : Convert.ToString(reader["NationalID"]),
                BirthCertificate = Convert.IsDBNull(reader["BirthCertificate"]) ? null : Convert.ToString(reader["BirthCertificate"]),
                Passport = Convert.IsDBNull(reader["Passport"]) ? null : Convert.ToString(reader["Passport"]),
                MemberNo = Convert.IsDBNull(reader["MemberNo"]) ? null : Convert.ToString(reader["MemberNo"]),
                AddedOn = Convert.IsDBNull(reader["AddedOn"]) ? (DateTime?)null : Convert.ToDateTime(reader["AddedOn"]),
                AddedBy = Convert.IsDBNull(reader["AddedBy"]) ? null : Convert.ToString(reader["AddedBy"]),
            };
        }
        private Member MapDataToOrganisation(SqlDataReader reader)
        {
            return new Member
            {
                ID = Convert.ToInt32(reader["ID"]),
                UID = Guid.Parse(reader["UID"].ToString()),
                Name1 = Convert.ToString(reader["Name1"])
            };
        }
        public bool CopyMember(Guid UID, string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Members_Copy";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    command.Parameters.AddWithValue("@UID", UID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy); 
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public bool UpdateMemberFromCopy(Guid UID, Guid RequestID,string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Members_UpdateFromCopy";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UID", UID);
                    command.Parameters.AddWithValue("RequestID", RequestID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public bool UpdateCopyStatus(Guid UID, Guid RequestID,int StatusID,string StatusComment,string AddedBy)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [MembersStaging] SET [StatusID]=@StatusID,[StatusComment]=@StatusComment WHERE [RequestID]=@RequestID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@StatusID", StatusID);
                    command.Parameters.AddWithValue("@StatusComment", StatusComment);
                    command.Parameters.AddWithValue("RequestID", RequestID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public DataTable GetStaging()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "MembersStaging_Get"; 
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
        public DataTable SearchStaging(string SearchTerm)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "MembersStaging_Search";
            command.Parameters.AddWithValue("SearchTerm", SearchTerm);
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(DT);
            return DT;
        }
    }
}
