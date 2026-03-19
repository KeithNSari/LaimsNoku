namespace LAIMS.Models.Membership
{
    public class StatiiReason
    {
        public int StatusID { get; set; }
        public int ReasonID { get; set; }
        public string Reason { get; set; }
        public DateTime? AddedOn { get; set; }
        public string AddedBy { get; set; }
    }
}
