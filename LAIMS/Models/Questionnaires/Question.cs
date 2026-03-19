using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Questionnaires
{
    public class Question
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntryNo { get; set; }

        [Required]
        public Guid ID { get; set; } 

        [Required]
        public int QuestionNo { get; set; }
         
        [StringLength(50)]
        public string? QuestionLabel { get; set; }

        [Required]
        [StringLength(500)]
        public string QuestionText { get; set; }

        [Required]
        public byte QuestionTypeID { get; set; }

        public decimal? Weight { get; set; }

        public int? Position { get; set; }

        [Required]
        [StringLength(450)]
        public string AddedBy { get; set; }

        [Required]
        public DateTime AddedOn { get; set; }
        public List<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();   
    }
}
