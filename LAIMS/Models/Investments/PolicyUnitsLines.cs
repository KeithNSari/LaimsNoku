namespace LAIMS.Models.Investments
{
    public class PolicyUnitsLines
    {
        public int ID { get; set; }
        public int PolicyUnitsID { get; set; }
        public int UnitPricesListID { get; set; }
        public decimal Units { get; set; }
        public int TransactionTypeID { get; set; }
        public string AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
    }
}
