namespace LAIMS.Models.Utilities
{
    public class ExcelUploadColumn
    {
        public int EntryNo { get; set; }
        public Guid ID { get; set; }
        public Guid MediaUploadID { get; set; }
        public string ColumnName { get; set; }
        public int? ColumnID { get; set; }
        public string DataType { get; set; }
        public string AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }

    }
}
