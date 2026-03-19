namespace LAIMS.Models.Utilities
{
    public class EventTypeCause
    {
        public int ID { get; set; }
        public Guid EventTypeID { get; set; }
        public string Cause { get; set; }
        public string Description { get; set; }
    }
}
