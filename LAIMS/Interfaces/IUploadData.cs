namespace LAIMS.Interfaces
{
    using LAIMS.Models.Utilities;
    using System.Data; 

    public interface IUploadData
    {
        string Documentupload(IFormFile formFile);
        DataTable ExcelDataTable(string path);
        FileContents ExcelData(string path);
        DataTable CSVDataTable(string path);
        string ToCSV(DataTable SourceDataTable); 
    }
}
