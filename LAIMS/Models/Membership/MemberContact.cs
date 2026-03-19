using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Membership
{
    [Table("MemberContacts")]
    public class MemberContact
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int ContactTypeID { get; set; }
        public string? ContactTypeName { get; set; }

        [Required]
        public int MemberID { get; set; } 
        public string ContactName { get; set; }
        [Required]
        public Guid MemberUID { get; set; }

        [MaxLength(500)]
        public string? MemberDetails { get; set; }
        public string? Designation { get; set; }
        [Required]
        [MaxLength(500)]
        public string Line1 { get; set; }

        [MaxLength(500)]
        public string Line2 { get; set; }

        [MaxLength(500)]
        public string Line3 { get; set; }

        public int CountryID { get; set; }
        public string? CountryName { get; set; }
        public int? City { get; set; }
        public string? CityName { get; set; }

        [Required]
        public byte Preferred { get; set; }
        public string? PreferredDesc { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? AddedOn { get; set; }

        [MaxLength(450)]
        public string AddedBy { get; set; }
    }
}
