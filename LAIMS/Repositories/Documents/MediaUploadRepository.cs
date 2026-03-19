
using Microsoft.Data.SqlClient;
using System.Configuration;
using LAIMS.Models.Documents;
using System;
using System.Collections.Generic;
using System.Data;
using LAIMS.Interfaces.Documents;
using Humanizer.Bytes;

namespace LAIMS.Repositories.Documents
{ 
    public class MediaUploadRepository: IMediaUploadRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public MediaUploadRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void SaveMediaUpload(MediaUpload mediaUpload)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"DECLARE @MemberID int=0; SELECT @MemberID=ID FROM Members WHERE UID=@MemberUID; INSERT INTO MediaUploads (ID, MemberID, FilingNo, DocumentNo,FileName,ContentType, DocumentsID, Data, AddedOn, AddedBy) VALUES (@ID, @MemberID, @FilingNo, @DocumentNo,@FileName, @ContentType, @DocumentsID, @Data, @AddedOn, @AddedBy)";

                    // Add parameters
                    command.Parameters.AddWithValue("@ID", mediaUpload.ID);
                    command.Parameters.AddWithValue("@MemberUID", mediaUpload.MemberUID);
                    command.Parameters.AddWithValue("@FilingNo", (object)mediaUpload.FilingNo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DocumentNo", (object)mediaUpload.DocumentNo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FileName", mediaUpload.FileName);
                    command.Parameters.AddWithValue("@ContentType", mediaUpload.ContentType);
                    command.Parameters.AddWithValue("@DocumentsID", mediaUpload.DocumentsID);
                    command.Parameters.AddWithValue("@Data", mediaUpload.Data);
                    command.Parameters.AddWithValue("@AddedOn", mediaUpload.AddedOn);
                    command.Parameters.AddWithValue("@AddedBy", mediaUpload.AddedBy);

                    // Execute the query
                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetDocuments(Guid MemberUID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "DECLARE @MemberID int=0; SELECT @MemberID=ID FROM Members WHERE UID=@MemberUID; SELECT [MediaUploads].[ID],[MemberID],[FilingNo],[Documents].[Document],[DocumentNo],[ContentType],[DocumentsID],[FileName],[MediaUploads].[AddedOn],[MediaUploads].[AddedBy] FROM [dbo].[MediaUploads] LEFT JOIN [Documents] ON [MediaUploads].[DocumentsID]=[Documents].[ID] WHERE [MediaUploads].[MemberID]=@MemberID ORDER BY [Documents].[Document] ASC";
            cmd.Parameters.AddWithValue("MemberUID", MemberUID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        } 
        public MediaUpload GetMediaUploadById(Guid id)
        { 
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT  [EntryNo],[ID],[MemberID],[FilingNo],[DocumentNo],[ContentType],ISNULL([DocumentsID],'00000000-0000-0000-0000-000000000000') AS [DocumentsID],[FileName],[Data],[AddedOn],[AddedBy] FROM [dbo].[MediaUploads] WHERE ID = @ID";
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToMediaUpload(reader);
                        }
                    }
                }
            }

            return null;
        }

        public List<MediaUpload> SearchMediaUploads(string filingNo, string documentNo)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            List<MediaUpload> results = new List<MediaUpload>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT * FROM MediaUploads WHERE FilingNo = @FilingNo OR DocumentNo = @DocumentNo";
                    command.Parameters.AddWithValue("@FilingNo", filingNo);
                    command.Parameters.AddWithValue("@DocumentNo", documentNo);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(MapToMediaUpload(reader));
                        }
                    }
                }
            }

            return results;
        }

        public void UpdateMediaUpload(MediaUpload mediaUpload)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"UPDATE MediaUploads
                                       SET MemberID = @MemberID, FilingNo = @FilingNo, DocumentNo = @DocumentNo,
                                           MimeType = @MimeType, DocumentsID = @DocumentsID, Data = @Data,
                                           AddedOn = @AddedOn, AddedBy = @AddedBy
                                       WHERE ID = @ID";

                    // Add parameters
                    command.Parameters.AddWithValue("@ID", mediaUpload.ID);
                    command.Parameters.AddWithValue("@MemberID", mediaUpload.MemberID);
                    command.Parameters.AddWithValue("@FilingNo", (object)mediaUpload.FilingNo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DocumentNo", (object)mediaUpload.DocumentNo ?? DBNull.Value);
                    //command.Parameters.AddWithValue("@MimeType", mediaUpload.MimeType);
                    command.Parameters.AddWithValue("@DocumentsID", mediaUpload.DocumentsID);
                    command.Parameters.AddWithValue("@Data", mediaUpload.Data);
                    command.Parameters.AddWithValue("@AddedOn", mediaUpload.AddedOn);
                    command.Parameters.AddWithValue("@AddedBy", mediaUpload.AddedBy);

                    // Execute the query
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteMediaUpload(Guid id)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "DELETE FROM MediaUploads WHERE ID = @ID";
                    command.Parameters.AddWithValue("@ID", id);

                    // Execute the query
                    command.ExecuteNonQuery();
                }
            }
        }

        private MediaUpload MapToMediaUpload(SqlDataReader reader)
        {
            return new MediaUpload
            {
                EntryNo = (int)reader["EntryNo"],
                ID = (Guid)reader["ID"],
                MemberID = (int)reader["MemberID"],
                FilingNo = reader["FilingNo"] is DBNull ? null : (string)reader["FilingNo"],
                DocumentNo = reader["DocumentNo"] is DBNull ? null : (string)reader["DocumentNo"],
                ContentType = reader["ContentType"].ToString(),
                DocumentsID = Guid.Parse(reader["DocumentsID"].ToString()),
                FileName = reader["FileName"].ToString(),
                Data = (byte[])reader["Data"],
                AddedOn = (DateTime)reader["AddedOn"],
                AddedBy = (string)reader["AddedBy"]
            };
        }
    }

}
