using Microsoft.CodeAnalysis.Elfie.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LAIMS.Models.Membership
{
    public class Member
    {
        public Guid BatchID { get; set; } = Guid.NewGuid();

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { get; set; }

        [Column("UID")]
        public Guid UID { get; set; }        

        [Required]
        [Column("IsOrganisation")]
        public byte IsOrganisation { get; set; }

        [Required]
        [MaxLength(300)]
        [Column("Name1")]
        public string Name1 { get; set; }

        [MaxLength(300)]
        [Column("Name2")]
        public string? Name2 { get; set; }

        [MaxLength(300)]
        [Column("Name3")]
        public string Name3 { get; set; }

        [Column("GenderID")]
        public int? GenderID { get; set; }

        [Column("Gender")]
        public string? Gender { get; set; }

        [Column("TitleID")]
        public int? TitleID { get; set; }

        [Column("Title")]
        public string? Title { get; set; }

        [Column("MaritalStatusID")]
        public int? MaritalStatusID { get; set; }

        [Column("MaritalStatus")]
        public string? MaritalStatus { get; set; }

        [Column("CountryID")]
        public int? CountryID { get; set; }

        [Column("Country")]
        public string? Country { get; set; }

        [Column("BirthCountryID")]
        public int? BirthCountryID { get; set; }

        [Column("BirthCountry")]
        public string? BirthCountry { get; set; }

        [Column("DOB")]
        public DateTime? DOB { get; set; }

        [MaxLength(50)]
        [Column("PlaceOfBirth")]
        public string? PlaceOfBirth { get; set; }

        [MaxLength(50)]
        [Column("NationalID")]
        public string? NationalID { get; set; }

        [MaxLength(50)] 
        public string? NationalIDConfirm { get; set; }

        [MaxLength(50)]
        [Column("BirthCertificate")]
        public string? BirthCertificate { get; set; }

        [MaxLength(50)] 
        public string? BirthCertificateConfirm { get; set; }

        [MaxLength(50)]
        [Column("Passport")]
        public string? Passport { get; set; }

        [MaxLength(50)] 
        public string? PassportConfirm { get; set; }

        [MaxLength(50)]
        [Column("MemberNo")]
        public string? MemberNo { get; set; }

        [Column("Confirmed")]
        public byte Confirmed { get; set; } = 0;

        [Column("AddedOn")]
        public DateTime? AddedOn { get; set; }

        [MaxLength(450)]
        [Column("AddedBy")]
        public string AddedBy { get; set; }
    }
}
