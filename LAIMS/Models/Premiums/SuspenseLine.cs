namespace LAIMS.Models.Premiums
{
    public class SuspenseLine
    {
        public int ID { get; set; }
        public int HeaderID { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public DateTime AddedOn { get; set; }
        public string AddedBy { get; set; }
    }
}
