namespace LAIMS.Models.Investments
{
    public class PolicyUnit
    {
        public int ID { get; set; }
        public Guid PolicyID { get; set; }
        public string UnitTrust { get; set; }
        public Guid UnitTrustID { get; set; }
        public decimal TotalUnits { get; set; }
        public int UnitPricesListID { get; set; }
        public decimal BidPrice { get; set; }
        public decimal OfferPrice { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
