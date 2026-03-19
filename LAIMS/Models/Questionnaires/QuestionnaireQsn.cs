using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Questionnaires
{
    public class QuestionnaireQsn
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntryNo { get; set; }

        [Required]
        public Guid ID { get; set; }

        [Required]
        public Guid Questionnaire { get; set; }

        [Required]
        public Guid Question { get; set; }

        [StringLength(450)]
        public string AddedBy { get; set; }

        public DateTime? AddedOn { get; set; }

        [Required]
        public byte Archived { get; set; }

        [StringLength(450)]
        public string ArchivedBy { get; set; }

        public DateTime? ArchivedOn { get; set; }
    }
}
