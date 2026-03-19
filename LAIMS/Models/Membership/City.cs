using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Membership
{
    public class City
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int CountryID { get; set; }

        [Required]
        [MaxLength(50)]
        public string CityName { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? AddedOn { get; set; }

        [MaxLength(450)]
        public string AddedBy { get; set; }
    }
}
