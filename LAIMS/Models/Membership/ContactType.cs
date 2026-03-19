using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Membership
{
    [Table("ContactTypes")]
    public class ContactType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Type { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? AddedOn { get; set; }

        [MaxLength(450)]
        public string AddedBy { get; set; }
    }
}
