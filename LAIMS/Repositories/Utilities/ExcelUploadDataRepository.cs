using LAIMS.Interfaces.Utilities;
using LAIMS.Models.Utilities;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Utilities
{
    public class ExcelUploadDataRepository: IExcelUploadDataRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private readonly string Database;

        public ExcelUploadDataRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public void Insert(ExcelUploadData excelUploadData)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"INSERT INTO ExcelUploadData (ID, MediaUploadID, Column1, Column2, Column3, Column4, Column5,
                            Column6, Column7, Column8, Column9, Column10, Column11, Column12, Column13, Column14, Column15)
                            VALUES (@ID, @MediaUploadID, @Column1, @Column2, @Column3, @Column4, @Column5,
                            @Column6, @Column7, @Column8, @Column9, @Column10, @Column11, @Column12, @Column13, @Column14, @Column15)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, excelUploadData);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(ExcelUploadData excelUploadData)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"UPDATE ExcelUploadData 
                             SET MediaUploadID = @MediaUploadID, Column1 = @Column1, 
                                 Column2 = @Column2, Column3 = @Column3, Column4 = @Column4, 
                                 Column5 = @Column5, Column6 = @Column6, Column7 = @Column7, 
                                 Column8 = @Column8, Column9 = @Column9, Column10 = @Column10, 
                                 Column11 = @Column11, Column12 = @Column12, Column13 = @Column13, 
                                 Column14 = @Column14, Column15 = @Column15
                             WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, excelUploadData);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM ExcelUploadData WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        public ExcelUploadData Read(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM ExcelUploadData WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDataReaderToModel(reader);
                        }
                    }
                }

                return null;
            }
        }

        public List<ExcelUploadData> GetAll()
        {
            List<ExcelUploadData> excelUploadDataList = new List<ExcelUploadData>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM ExcelUploadData";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ExcelUploadData excelUploadData = MapDataReaderToModel(reader);
                            excelUploadDataList.Add(excelUploadData);
                        }
                    }
                }
            }

            return excelUploadDataList;
        }

        private void SetParameters(SqlCommand command, ExcelUploadData excelUploadData)
        {
            command.Parameters.AddWithValue("@ID", excelUploadData.ID);
            command.Parameters.AddWithValue("@MediaUploadID", excelUploadData.MediaUploadID);
            command.Parameters.AddWithValue("@Column1", excelUploadData.Column1 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column2", excelUploadData.Column2 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column3", excelUploadData.Column3 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column4", excelUploadData.Column4 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column5", excelUploadData.Column5 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column6", excelUploadData.Column6 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column7", excelUploadData.Column7 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column8", excelUploadData.Column8 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column9", excelUploadData.Column9 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column10", excelUploadData.Column10 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column11", excelUploadData.Column11 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column12", excelUploadData.Column12 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column13", excelUploadData.Column13 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column14", excelUploadData.Column14 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Column15", excelUploadData.Column15 ?? (object)DBNull.Value);
        }

        private ExcelUploadData MapDataReaderToModel(SqlDataReader reader)
        {
            return new ExcelUploadData
            {
                ID = (Guid)reader["ID"],
                MediaUploadID = (Guid)reader["MediaUploadID"],
                Column1 = reader["Column1"] as string,
                Column2 = reader["Column2"] as string,
                Column3 = reader["Column3"] as string,
                Column4 = reader["Column4"] as string,
                Column5 = reader["Column5"] as string,
                Column6 = reader["Column6"] as string,
                Column7 = reader["Column7"] as string,
                Column8 = reader["Column8"] as string,
                Column9 = reader["Column9"] as string,
                Column10 = reader["Column10"] as string,
                Column11 = reader["Column11"] as string,
                Column12 = reader["Column12"] as string,
                Column13 = reader["Column13"] as string,
                Column14 = reader["Column14"] as string,
                Column15 = reader["Column15"] as string,
            };
        }
    }
} 
