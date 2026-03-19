using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Premiums
{
    public class PolicyPremiumLine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int PolicyPremiumsID { get; set; }
        [Required]
        public bool IsMainProduct {  get; set; }

        [Required]
        public Guid PolicyProductID { get; set; }

        public Guid PolicyID;

        public Guid PolicyType;

        [Required] 
        public decimal Premium { get; set; }

        [Required]
        public int StatusID { get; set; }

        [Required] 
        public DateTime StatusDate { get; set; }
    }
}
