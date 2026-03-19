using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LAIMS.Models.Questionnaires
{
    [Table("QuestionnaireResponses")]
    public class QuestionnaireResponse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntryNo { get; set; }

        [Required]
        public Guid ID { get; set; }
        [Required]
        public Guid PolicyID { get; set; } = Guid.Empty;

        [Required]
        public Guid MemberUID { get; set; }
        public string MemberName { get; set; }

        [Required]
        public Guid Questionnaire { get; set; }

        public string QuestionnaireTitle { get; set; }

        public byte Submitted { get; set; }
        public DateTime? SubmittedOn { get; set; }

        [Required] 
        public DateTime AddedOn { get; set; }

        [Required]
        [MaxLength(450)]
        public string AddedBy { get; set; }
    }
}
