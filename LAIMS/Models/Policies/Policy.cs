using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace LAIMS.Models.Policies
{
    public class Policy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntryNo { get; set; }

        [Required]
        public Guid ID { get; set; }

        [Required]
        public Guid MemberUID { get; set; }

        [MaxLength(20)]
        public string PolicyNo { get; set; }

        [Required]
        public Guid PolicyType { get; set; }
        [Required]
        public string PolicyName { get; set; }

        public DateTime? EffectiveDate { get; set; }
        public DateTime? CommencementDate { get; set; }

        public int PolicyStatus { get; set; }

        public int PolicyDurationYears { get; set; }

        public string? SummaryOfTCS { get; set; }
        public string? Declaration { get; set; }

        public DateTime ExpirationDate {  get; set; }
        public int CurrencyID {  get; set; }
        public string CurrencyName { get; set; }
        public decimal InvestmentContentBalance { get; set; }
        public decimal InvestmentContentTotalCredit { get; set; }
        public decimal InvestmentContentTotalDebit { get; set; }
        public DateTime? AddedOn { get; set; }

        [MaxLength(450)]
        public string AddedBy { get; set; }
    }
}
