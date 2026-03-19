using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.LifeProducts
{
    public class Currency
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string CurrencyName { get; set; }
        [Required]
        [MaxLength(10)]
        public string ShortCode { get; set; }
        [Required]
        public byte Default { get; set; }
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
