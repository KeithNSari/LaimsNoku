using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LAIMS.Models.Membership
{
    public class Title
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("TitleID")]
        public int TitleID { get; set; }

        [Required]
        [MaxLength(50)]
        public string TitleName { get; set; }
    }
}
