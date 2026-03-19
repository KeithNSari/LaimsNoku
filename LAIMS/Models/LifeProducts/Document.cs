using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.LifeProducts
{
    public class Document
    {
        [Key]
        public Guid ID { get; set; }
        public int EntryNo { get; set; }
        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; }
        public string FilingNo { get; set; }
        public bool Uploaded { get; set; }
        public Guid UploadID { get; set; }
        public DateTime? AddedOn { get; set; }
        public string AddedBy { get; set; }
        public byte? Archived { get; set; }
        public string ArchivedBy { get; set; }
        public string ArchivedComment { get; set; }
        public DateTime? ArchivedOn { get; set; }
        public byte? Deleted { get; set; }
        public string DeletedBy { get; set; }
        public string DeletedComment { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
