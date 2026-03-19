namespace LAIMS.Models.Utilities
{
    public class EventType
    {
        public int EntryNo { get; set; }
        public Guid ID { get; set; }
        public string EventName { get; set; }
        public DateTime? AddedOn { get; set; }
        public string AddedBy { get; set; }
    }
}
