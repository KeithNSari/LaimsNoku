using System;
using System.ComponentModel.DataAnnotations;
namespace LAIMS.Models.LifeProducts
{  
    public class ExpenseType
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime? AddedOn { get; set; }

        [MaxLength(450)]
        public string AddedBy { get; set; }

        public bool? Archived { get; set; }

        [MaxLength(450)]
        public string ArchivedBy { get; set; }

        [MaxLength(500)]
        public string ArchivedComment { get; set; }

        public DateTime? ArchivedOn { get; set; }

        public bool? Deleted { get; set; }

        [MaxLength(450)]
        public string DeletedBy { get; set; }

        [MaxLength(500)]
        public string DeletedComment { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
