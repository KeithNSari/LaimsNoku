using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Questionnaires
{
    public class Questionnaire
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntryNo { get; set; }

        [Required]
        public Guid ID { get; set; }

        [Required]
        [StringLength(500)]
        public string Title { get; set; }

        [Required]
        public byte Category { get; set; }

        [Required]
        public byte Weighted { get; set; }

        public decimal? TotalWeight { get; set; }

        [StringLength(450)]
        public string AddedBy { get; set; }

        public DateTime? AddedOn { get; set; }

        public byte? Archived { get; set; }

        [StringLength(450)]
        public string ArchivedBy { get; set; }

        public DateTime? ArchivedOn { get; set; }
    }
}
