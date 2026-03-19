using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Premiums
{
    public class Intermediary
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public Guid BatchID { get; set; }   
        [Required]
        public int IntermediaryTypeID { get; set; }
        [Required]
        public int ReportsToIntermediaryID { get; set; }
        [Required ]
        public string ReportsToAgentCode { get; set; }
        [Required]
        public int DesignationID { get; set; }
        public string AgentCode { get; set; }
        public string EmployeeNo { get; set; }
        [Required]
        public int MemberID { get; set; }

        [Required] 
        public DateTime? Started { get; set; }

        [Required] 
        public DateTime? Ended { get; set; }

        [Required]
        public byte Current { get; set; }
    }
}
