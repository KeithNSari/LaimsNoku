using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.LifeProducts
{
    public class PTQuestionnaire
    {  
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int EntryNo { get; set; }

            [Required]
            public Guid ID { get; set; }

            [Required]
            public Guid QuestionnaireID { get; set; }

            [Required]
            public Guid PolicyTypeID { get; set; }
            [Required]
            public byte TestedBusiness { get; set; }
            public decimal CoverRangeStart { get;set; }
            public decimal CoverRangeEnd { get; set; }
            public int StartAge { get; set; }
            public int EndAge { get; set; }
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
