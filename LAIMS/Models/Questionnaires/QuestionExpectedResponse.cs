using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Questionnaires
{
    public class QuestionExpectedResponse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntryNo { get; set; }

        [Required]
        public Guid ID { get; set; }

        [Required]
        public int Sequence { get; set; }

        [StringLength(50)]
        public string Label { get; set; }

        [Required]
        public Guid QuestionID { get; set; }

        [Required]
        [StringLength(50)]
        public string ExpectedResponse { get; set; }

        public decimal? Weight { get; set; }

        [StringLength(450)]
        public string AddedBy { get; set; }

        public DateTime? AddedOn { get; set; }
    }
}
