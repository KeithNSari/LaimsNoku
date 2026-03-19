using LAIMS.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.IO;
using LAIMS.Models.Utilities;
namespace LAIMS.Utilities
{
    public class UploadData : IUploadData
    {

        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public UploadData(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
        public DataTable ExcelDataTable(string path)
        {
            var constr = _configuration.GetConnectionString("ExcelConString");
            DataTable datatable = new();
            constr = string.Format(constr, path);
            using (OleDbConnection excelconn = new(constr))
            {
                using OleDbCommand cmd = new();
                using OleDbDataAdapter ODA = new();
                excelconn.Open();
                cmd.Connection = excelconn;
                DataTable excelschema;
                excelschema = excelconn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                var sheetname = excelschema.Rows[0]["Table_Name"].ToString();
                cmd.CommandText = "SELECT * From [" + sheetname + "]";
                ODA.SelectCommand = cmd;
                ODA.Fill(datatable);
                excelconn.Close();
            }
            File.Delete(path);
            return datatable;
        }       
        public FileContents ExcelData(string path)
        {
            FileContents fileContents = new FileContents();
            var connectionString = _configuration.GetConnectionString("ExcelConString");
            connectionString = string.Format(connectionString, path); 
            fileContents.ColumnsList = new List<string>(); // Initialize OriginalColumnsList  
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();

                DataTable schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                foreach (DataRow row in schemaTable.Rows)
                {
                    string sheetName = row["TABLE_NAME"].ToString();
                    if (sheetName.EndsWith("$"))
                    {
                        // Remove the dollar sign at the end to get the actual sheet name
                        sheetName = sheetName.Substring(0, sheetName.Length - 1);
                    }

                    // Get column names for the current sheet
                    DataTable sheetSchema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, sheetName, null });

                    // Extract the first few rows of data to determine the original column names
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter($"SELECT TOP 10 * FROM [{sheetName}$]", connection))
                    {
                        DataTable tempData = new DataTable();
                        adapter.Fill(tempData);
                        // Assuming the first row contains the original column names
                        foreach (DataColumn column in tempData.Columns)
                        { 
                            fileContents.ColumnsList.Add(column.ColumnName);
                        }
                    }

                    // Create DataTable to hold data
                    DataTable dataDT = new DataTable();
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter($"SELECT * FROM [{sheetName}$]", connection))
                    {
                        adapter.Fill(dataDT);
                    }

                    // Add data to FileContents model 
                    fileContents.DataDT = dataDT;
                    break;
                }
            }
            File.Delete(path);
            return fileContents;
        }
        //this assumes a simple CSV file without commas in the data
        public DataTable CSVDataTable(string path)
        {
            string csvData = File.ReadAllText(path);
            DataTable DT = new();
            int headerColumnCount = 0;
            if (!string.IsNullOrEmpty(csvData))
            {
                string[] rows = csvData.Split('\n');
                int rowsCount = rows.Length;
                for (int i = 0; i < rowsCount; i++)
                {
                    string row = rows[i];
                    string[] rowData = row.Split(',');
                    if (i == 0)
                    {
                        headerColumnCount = rowData.Length;
                        for (int j = 0; j < headerColumnCount; j++)
                        {
                            DataColumn DC = new(rowData[j]);
                            DT.Columns.Add(DC);
                        }
                    }
                    else
                    {
                        int currentRowColumnCount = rowData.Length;
                        if (currentRowColumnCount != headerColumnCount) throw new Exception("Unexpected number of columns! Please remove commas.");
                        for (int j = 0; j < currentRowColumnCount; j++)
                        {
                            DataRow newRow = DT.NewRow();
                            newRow[j] = rowData[j];
                            DT.Rows.Add(newRow);
                        }
                    }
                }
            }
            return DT;
        }

        public string Documentupload(IFormFile UploadFile)
        {
            string webRootFolder = _environment.WebRootPath;
            string uploadsFolder = Path.Combine(webRootFolder, "Uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            string sourcefile = Path.GetFileName(UploadFile.FileName);
            string path = Path.Combine(uploadsFolder, sourcefile);
            using (FileStream filestream = new FileStream(path, FileMode.Create))
            {
                UploadFile.CopyTo(filestream);
            }
            return path;
        }
        public void DocumentUploadToDB(IFormFile UploadFile, string FileName,string ContentType,string AddedOn)
        {
            var constr = _configuration.GetConnectionString("DefaultConnection");
            using (MemoryStream ms = new MemoryStream())
            {
                UploadFile.CopyTo(ms);
                using (SqlConnection con = new(constr))
                {
                    string query = "INSERT INTO [dbo].[MediaUploads]([ID],[MemberID],[FilingNo],[MimeType],[DocumentsID],[Data],[AddedOn],[AddedBy]) VALUES(@ID,@MemberID,@FilingNo,@MimeType,@DocumentsID,@Data,@AddedOn,@AddedBy)";
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        cmd.Parameters.AddWithValue("@Name", FileName);
                        cmd.Parameters.AddWithValue("@ContentType", ContentType);
                        cmd.Parameters.AddWithValue("@Data", ms.ToArray());
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
            }
        }
        public string ToCSV(DataTable SourceDataTable)
        {
            string webRootFolder = _environment.WebRootPath;
            string uploadsFolder = Path.Combine(webRootFolder, "Uploads");
            string filePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + ".csv");
            StreamWriter sw = new(filePath, false);
            for (int i = 0; i < SourceDataTable.Columns.Count; i++)
            {
                sw.Write(SourceDataTable.Columns[i]);
                if (i < SourceDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in SourceDataTable.Rows)
            {
                for (int i = 0; i < SourceDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < SourceDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
            return filePath;
        }
    }
}
