using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Membership
{
    public class Employment
    {
        [Key]
        public Guid ID { get; set; }

        [Required(ErrorMessage = "Employer name is required")]
        [StringLength(500, ErrorMessage = "Employer name must not exceed 500 characters")]
        public string Employer { get; set; }
        public Guid EmployerID { get; set; }
        public Guid MemberUID { get; set; }
        public int MemberID { get; set; }

        [Required(ErrorMessage = "Job title is required")]
        [StringLength(100, ErrorMessage = "Job title must not exceed 100 characters")]
        public string JobTitle { get; set; }

        [Required(ErrorMessage = "Employment number is required")]
        [StringLength(50, ErrorMessage = "Employment number must not exceed 50 characters")]
        public string EmploymentNo { get; set; }

        [Required(ErrorMessage = "Category ID is required")]
        public Guid CategoryID { get; set; }

        public int SalaryCurrencyID { get; set; }
        public decimal GrossSalary { get;set; }
        public decimal NetSalary { get; set; }
        public string AddedBy { get; set; }

    }
}
