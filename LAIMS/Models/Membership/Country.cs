using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LAIMS.Models.Membership
{
    public class Country
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("CountryID")]
        public int CountryID { get; set; }

        [Required]
        [MaxLength(300)]
        [Column("Country")]
        public string CountryName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Code { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("NumericCode")]
        public string NumericCode { get; set; }

        [Required]
        public int Sequence { get; set; }

        [Required]
        [Range(0, 255)]
        public byte Visibility { get; set; }
    }
}
