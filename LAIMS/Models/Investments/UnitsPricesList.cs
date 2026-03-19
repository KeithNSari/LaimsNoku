namespace LAIMS.Models.Investments
{
    public class UnitsPricesList
    {
        public int ID { get; set; }
        public string UnitTrust {  get; set; }  
        public Guid UnitTrustID { get; set; }
        public int CurrencyID { get; set; }
        public decimal BidPrice { get; set; }
        public decimal OfferPrice { get; set; }
        public decimal Amount { get; set; } //amount purchased
        public decimal TotalQuantity { get; set; }// total quantity purchased
        public DateTime EffectiveDate { get; set; }
        public string AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
    }
}
