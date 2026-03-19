using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.LifeProducts
{
    public class DurationUnit
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string UnitName { get; set; }
    }
}
