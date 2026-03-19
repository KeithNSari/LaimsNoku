using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Membership
{
    public class LIRole
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; }
        public DateTime? AddedOn { get; set; }

        [MaxLength(450)]
        public string AddedBy { get; set; }
    }
}
