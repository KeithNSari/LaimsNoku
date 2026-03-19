using LAIMS.Models.Utilities;

namespace LAIMS.Interfaces.Utilities
{
    public interface IExcelUploadDataRepository
    {
        void Insert(ExcelUploadData excelUploadData);
        void Update(ExcelUploadData excelUploadData);
        void Delete(Guid id);
        ExcelUploadData Read(Guid id);
       List<ExcelUploadData> GetAll();
    } 
}
