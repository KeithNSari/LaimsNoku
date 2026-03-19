using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Membership
{
    public class Relationship
    {
        [Key]
        public int ID { get; set; }
         
        [Required]
        [MaxLength(500)]         
        public string RelationshipName { get; set; }         
        public DateTime? AddedOn { get; set; }

        [MaxLength(450)]
        public string AddedBy { get; set; }
    }
}
