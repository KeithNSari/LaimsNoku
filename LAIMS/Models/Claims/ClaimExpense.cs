namespace LAIMS.Models.Claims
{
    public class ClaimExpense
    {
        public int ID { get; set; }
        public string Item { get; set; }
        public decimal Price { get; set; }
        public bool IsSelected { get; set; }
        public byte ApplicationType { get; set; }

	}
}
