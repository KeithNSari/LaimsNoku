using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Questionnaires
{
    public class AnswerOption
    {
        [Required]
        public Guid ID { get; set; }
        [StringLength(50)]
        public string Label { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ExpectedResponse { get; set; }
    }
}
