using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LAIMS.Models.Documents
{
    public class MediaUpload
    { 
        public int EntryNo { get; set; }

        [Key]
        public Guid ID { get; set; } 
        public int MemberID { get; set; }
        public Guid MemberUID { get; set; }

        [MaxLength(500)]
        public string FileName { get; set; }

        [MaxLength(50)]
        public string FilingNo { get; set; }

        [MaxLength(50)]
        public string DocumentNo { get; set; }

        [Required]
        public string ContentType { get; set; }

        [Required]
        public Guid DocumentsID { get; set; }

        [Required]
        public byte[] Data { get; set; }

        [Required]
        public DateTime AddedOn { get; set; }

        [Required]
        [MaxLength(450)]
        public string AddedBy { get; set; }
    }

}
