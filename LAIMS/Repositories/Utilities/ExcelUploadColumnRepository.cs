using LAIMS.Interfaces.Utilities;
using LAIMS.Models.Utilities;
using Microsoft.Data.SqlClient;

namespace LAIMS.Repositories.Utilities
{
    public class ExcelUploadColumnRepository: IExcelUploadColumnRepository
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private readonly string Database;
        public ExcelUploadColumnRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }

        public void Insert(ExcelUploadColumn excelUploadColumn)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = @"INSERT INTO ExcelUploadColumns (ID, MediaUploadID, ColumnName, ColumnID, DataType)
                             VALUES (@ID, @MediaUploadID, @ColumnName, @ColumnID, @DataType)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, excelUploadColumn);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(ExcelUploadColumn excelUploadColumn)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = @"UPDATE ExcelUploadColumns 
                             SET MediaUploadID = @MediaUploadID, ColumnName = @ColumnName, 
                                 ColumnID = @ColumnID, DataType = @DataType, 
                                 AddedBy = @AddedBy, AddedOn = @AddedOn
                             WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SetParameters(command, excelUploadColumn);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "DELETE FROM ExcelUploadColumns WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        public ExcelUploadColumn Read(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM ExcelUploadColumns WHERE ID = @ID";

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

        public List<ExcelUploadColumn> GetAll()
        {
            List<ExcelUploadColumn> excelUploadColumns = new List<ExcelUploadColumn>();

            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();

                string query = "SELECT * FROM ExcelUploadColumns";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ExcelUploadColumn excelUploadColumn = MapDataReaderToModel(reader);
                            excelUploadColumns.Add(excelUploadColumn);
                        }
                    }
                }
            }

            return excelUploadColumns;
        }

        private void SetParameters(SqlCommand command, ExcelUploadColumn excelUploadColumn)
        {
            command.Parameters.AddWithValue("@ID", excelUploadColumn.ID);
            command.Parameters.AddWithValue("@MediaUploadID", excelUploadColumn.MediaUploadID);
            command.Parameters.AddWithValue("@ColumnName", excelUploadColumn.ColumnName);
            command.Parameters.AddWithValue("@ColumnID", excelUploadColumn.ColumnID ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DataType", excelUploadColumn.DataType); 
        }

        private ExcelUploadColumn MapDataReaderToModel(SqlDataReader reader)
        {
            return new ExcelUploadColumn
            {
                EntryNo = (int)reader["EntryNo"],
                ID = (Guid)reader["ID"],
                MediaUploadID = (Guid)reader["MediaUploadID"],
                ColumnName = reader["ColumnName"] as string,
                ColumnID = reader["ColumnID"] as int?,
                DataType = reader["DataType"] as string,
                AddedBy = reader["AddedBy"] as string,
                AddedOn = reader["AddedOn"] as DateTime?
            };
        }
    }
} 
