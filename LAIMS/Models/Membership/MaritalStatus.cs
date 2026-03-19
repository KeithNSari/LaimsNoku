using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Membership
{
    public class MaritalStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("MaritalStatusID")]
        public int MaritalStatusID { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("MaritalStatus")]
        public string MaritalStatusName { get; set; }
    }
}
