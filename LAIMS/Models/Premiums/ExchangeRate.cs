using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LAIMS.Models.Premiums
{
	public class ExchangeRate
	{
		[Key]
		public int EntryNo { get; set; }

		[Required]
		public int BaseCurrency { get; set; }

		[Required]
		public int OtherCurrency { get; set; }

		[Required]
		[Column(TypeName = "decimal(20,10)")]
		public decimal Value { get; set; }
		[Required]
		public DateTime EffectiveDate { get; set; }

		[Required]
		[StringLength(256)]
		public string AddedBy { get; set; }

		[Required]
		public DateTime AddedOn { get; set; }
	}

}
