using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Questionnaires
{
    [Table("QuestionnaireResponseLines")]
    public class QuestionnaireResponseLine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public Guid HeaderID { get; set; }

        [Required]
        public Guid QuestionID { get; set; }

        [Required]
        public Guid ResponseID { get; set; }

        [Required]
        [MaxLength(4000)]
        public string ResponseText { get; set; }

        [Required]
        [Column(TypeName = "datetime2(7)")]
        public DateTime AddedOn { get; set; }

        [Required]
        [MaxLength(450)]
        public string AddedBy { get; set; }
    }
}
