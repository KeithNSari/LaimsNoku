using System.ComponentModel.DataAnnotations;

namespace LAIMS.Models.Premiums
{
    public class PremiumCollectionConfigHeader
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int PaymentMethodID { get; set; }

        [Required]
        public int PaymentProviderID { get; set; }
        public int CurrencyID { get; set; }
        public int? InternalBankAccountID { get; set; }
        public string? StopOrderName { get; set; }
        public string? StopOrderCode { get; set; }
        public int? SalaryDisbursementdate { get;set; }
        public int? Billingdate { get; set; }
        public decimal? CollectionCommissionRate { get; set; }
        public byte? Net { get; set; }
        [Required]
        [MaxLength(500)]
        public string StoredProcedureName { get; set; }       

        [Required]
        public byte Aggregated { get; set; }

        public DateTime? AddedOn { get; set; }

        [MaxLength(450)]
        public string AddedBy { get; set; }

        public byte? Archived { get; set; }

        [MaxLength(450)]
        public string ArchivedBy { get; set; }

        [MaxLength(500)]
        public string ArchivedComment { get; set; }

        public DateTime? ArchivedOn { get; set; }
    }
}
