namespace LAIMS.Models.Utilities
{
    public class Holiday
    {
        public int HolidayID { get; set; }
        public string HolidayName { get; set; }
        public DateTime HolidayDate { get; set; }
        public int Year { get; set; }
        public DateTime NextBillingDate { get; set; }
    }
}
