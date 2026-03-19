using System.ComponentModel.DataAnnotations; 
namespace LAIMS.Models.LifeProducts
{ 
    public class PolicyTypesExpenseLine
    {
        public int ID { get; set; }

        [Display(Name = "Expense Type ID")]
        public int? ExpenseTypeID { get; set; }

        [Display(Name = "Is Percentage")]
        public bool? IsPercentage { get; set; }

        [Display(Name = "Currency ID")]
        [Required(ErrorMessage = "Currency ID is required.")]
        public int CurrencyID { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? Amount { get; set; }

        public bool? Current { get; set; }

        [Display(Name = "Added On")]
        [DataType(DataType.DateTime)]
        public DateTime? AddedOn { get; set; }

        [Display(Name = "Added By")]
        public string AddedBy { get; set; }

        public bool? Archived { get; set; }

        [Display(Name = "Archived By")]
        public string ArchivedBy { get; set; }

        [Display(Name = "Archived Comment")]
        [StringLength(500, ErrorMessage = "Archived Comment must be at most 500 characters long.")]
        public string ArchivedComment { get; set; }

        [Display(Name = "Archived On")]
        [DataType(DataType.DateTime)]
        public DateTime? ArchivedOn { get; set; }

        public bool? Deleted { get; set; }

        [Display(Name = "Deleted By")]
        public string DeletedBy { get; set; }

        [Display(Name = "Deleted Comment")]
        [StringLength(500, ErrorMessage = "Deleted Comment must be at most 500 characters long.")]
        public string DeletedComment { get; set; }

        [Display(Name = "Deleted On")]
        [DataType(DataType.DateTime)]
        public DateTime? DeletedOn { get; set; }
    }
}
