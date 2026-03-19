using LAIMS.Interfaces;
using LAIMS.Models.Utilities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Utilities
{
    public class UserLists : IUserList
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public UserLists(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
        public DataTable GetUsers()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT TOP 10 A.[UserName],A.[FirstNames],A.[Surname],[Roles],A.[AddedOn]," +
                "B.[UserName] AS [AddedBy] FROM [dbo].[AspNetUsers] A " +
                "LEFT JOIN [AspNetUsers] B ON [A].[AddedBy]=B.[Id] LEFT JOIN (SELECT STRING_AGG([Name],',') AS [Roles],[UserId] " +
                "FROM [AspNetUserRoles] " +
                "LEFT JOIN [AspNetRoles] ON [AspNetRoles].[Id]=[AspNetUserRoles].[RoleId] " +
                "GROUP BY [UserId]) C ON C.UserId=A.ID ORDER BY A.[EntryNo] DESC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetRoles()
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [Id],[Name] FROM  [dbo].[AspNetRoles] ORDER BY [Name] ASC";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetUserRoles(string UserID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [AspNetRoles].[Id] AS [RoleID],[AspNetRoles].[Name] AS [RoleName],CASE WHEN [A].[RoleId] IS NOT NULL THEN 1 ELSE 0 END AS RoleExists FROM [dbo].[AspNetRoles] LEFT JOIN (SELECT [RoleID] FROM [AspNetUserRoles] WHERE [UserId]=@UserID) A ON [AspNetRoles].[Id]=A.[RoleId] ORDER BY [Name] ASC";
            cmd.Parameters.AddWithValue("@UserID", UserID); 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public DataTable GetUserRoleList(string UserID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT [AspNetRoles].[Id] AS [RoleID],[AspNetRoles].[Name] AS [RoleName] FROM [dbo].[AspNetUserRoles] LEFT JOIN [AspNetRoles] ON [AspNetRoles].[Id]=[RoleId] WHERE [UserId]=@UserID ORDER BY [Name] ASC";
            cmd.Parameters.AddWithValue("@UserID", UserID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public void UpdateUserPersonalNames(string FirstNames, string Surname, string UserID, int DesignationID, string AddedBy)
        {
            if (string.IsNullOrEmpty(FirstNames)) FirstNames = string.Empty;
            if (string.IsNullOrEmpty(Surname)) Surname = string.Empty;

            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [dbo].[AspNetUsers] SET [FirstNames]=@FirstNames,[Surname]=@Surname,[DesignationID]=@DesignationID,[AddedBy]=@AddedBy WHERE [Id]=@ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FirstNames", FirstNames);
                    command.Parameters.AddWithValue("@Surname", Surname);
                    command.Parameters.AddWithValue("@ID", UserID);
                    command.Parameters.AddWithValue("@DesignationID", DesignationID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void ClearUserRoles(string UserID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM [AspNetUserRoles] WHERE [UserID]=@UserID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", UserID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void InsertUserRoles(string FullRolesList, string UserID)
        { 
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "INSERT INTO [AspNetUserRoles] ([UserID], [RoleID]) SELECT @UserID, r.[ID] FROM [AspNetRoles] r LEFT JOIN [AspNetUserRoles] ur ON ur.[RoleID] = r.[ID] AND ur.[UserID] = @UserID WHERE r.[ID] IN (" + FullRolesList + ") AND ur.[RoleID] IS NULL;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", UserID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateUserDesignationRoles(int DesignationID, string UserID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE [AspNetUsers] SET [DesignationID]=@DesignationID WHERE [Id]=@UserID; INSERT INTO [AspNetUserRoles]([UserId],[RoleId]) SELECT @UserID,[DesignationRoles].[AspNetRoleID] FROM [dbo].[DesignationRoles] " +
                    "LEFT JOIN (SELECT [RoleID] FROM [AspNetUserRoles] WHERE [AspNetUserRoles].[UserID]=@UserID) A " +
                    "ON [DesignationRoles].[AspNetRoleID]=A.[RoleId] WHERE [DesignationID]=@DesignationID AND A.[RoleID] IS NULL";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", UserID);
                    command.Parameters.AddWithValue("@DesignationID", DesignationID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public UserDetails GetUserDetailsById(string userId)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            string query = @"SELECT A.[EntryNo],A.[Id],A.[UserName],A.[FirstNames],A.[Surname],A.[Email],A.[PhoneNumber],A.[AddedOn]," +
                            "B.[UserName] AS [AddedBy],ISNULL([Designations].[Designation],'') AS [Designation] " +
                            "FROM [dbo].[AspNetUsers] A LEFT JOIN [AspNetUsers] B ON A.AddedBy=B.Id " +
                            "LEFT JOIN [Designations] ON [A].[DesignationID]=[Designations].[ID] WHERE A.[Id] = @UserID"; 
            UserDetails userDetails = null; 
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                { 
                    command.Parameters.AddWithValue("@UserID", userId); 
                    connection.Open(); 
                    using (SqlDataReader reader = command.ExecuteReader())
                    { 
                        if (reader.Read())
                        { 
                            userDetails = new UserDetails
                            {
                                Username = reader["UserName"].ToString(),
                                UserID = reader["Id"].ToString(),
                                Designation = reader["Designation"].ToString(),
                                FirstNames = reader["FirstNames"].ToString(),
                                Surname = reader["Surname"].ToString(),
                                Email = reader["Email"].ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString(),
                                AddedOn = Convert.ToDateTime(reader["AddedOn"]),
                                AddedBy = reader["AddedBy"].ToString()
                            };
                        }
                    }
                }
            }
            return userDetails;
        }
    }
}
