using LAIMS.Models.Utilities;

namespace LAIMS.Interfaces.Utilities
{
    public interface IExcelUploadColumnRepository
    {
        void Insert(ExcelUploadColumn excelUploadColumn);
        void Update(ExcelUploadColumn excelUploadColumn);
        void Delete(Guid id);
        ExcelUploadColumn Read(Guid id);
        List<ExcelUploadColumn> GetAll();
    }
}
