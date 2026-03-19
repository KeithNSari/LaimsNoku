using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.LifeProducts
{ 
    public class Product
    {       
        public int EntryNo { get; set; }
      
        [Key]
        public Guid ID { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [Range(1, 4, ErrorMessage = "Invalid category.")]
        public int CategoryID { get; set; }
        public string Category { get; set; }

        [Required(ErrorMessage = "Term is required.")]
        [Range(0, 2, ErrorMessage = "Invalid Term.")]
        public int TermID { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(250, ErrorMessage = "Product name must not exceed 250 characters.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string Description { get; set; }
        public decimal Cover { get; set; } = 0;
        public decimal Premium { get; set; } = 0;

        [DataType(DataType.DateTime)]
        public DateTime? AddedOn { get; set; }

        [StringLength(450, ErrorMessage = "AddedBy must not exceed 450 characters.")]
        public string AddedBy { get; set; }

        public bool? Archived { get; set; }

        [StringLength(450, ErrorMessage = "ArchivedBy must not exceed 450 characters.")]
        public string ArchivedBy { get; set; }

        [StringLength(500, ErrorMessage = "ArchivedComment must not exceed 500 characters.")]
        public string ArchivedComment { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ArchivedOn { get; set; }

        public bool? Deleted { get; set; }

        [StringLength(450, ErrorMessage = "DeletedBy must not exceed 450 characters.")]
        public string DeletedBy { get; set; }

        [StringLength(500, ErrorMessage = "DeletedComment must not exceed 500 characters.")]
        public string DeletedComment { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DeletedOn { get; set; }
    }
}
