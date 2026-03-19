using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace LAIMS.Models.Premiums
{
    public class PolicyTypesExpense
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public Guid PolicyTypeID { get; set; }

        public int? PaymentFrequencyID { get; set; }

        [Required]
        public int ExpenseTypeID { get; set; }

        [Required]
        public byte Ispercentage { get; set; }

        [Required]
        public byte AppliesTo { get; set; }
        [Required]
        public int IntermediaryTypeID { get; set; }
        [Required]
        public int CurrencyID { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Qty/Amount should be 0 or greater.")]
        public decimal Amount { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Start month should be 0 or greater.")] 
        public int StartMonth { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "End month should be 0 or greater.")]
        public int EndMonth { get; set; } 
		public int StageID { get; set; } 
		public int ApplicationTypeID { get; set; }

		public byte? Current { get; set; }
         
        public DateTime? AddedOn { get; set; }

        [StringLength(450)]
        public string AddedBy { get; set; }

        public byte? Deleted { get; set; }

        [StringLength(450)]
        public string DeletedBy { get; set; }

        [StringLength(500)]
        public string DeletedComment { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DeletedOn { get; set; }
    }
}
