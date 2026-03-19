namespace LAIMS.Models.Membership
{
    public class MemberStatus
    {
        public int ID { get; set; }
        public Guid MemberUID { get; set; }
        public int Status { get; set; }
        public int? StatusReason { get; set; }
        public DateTime StatusDate { get; set; }
        public string StatusComment { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
    }
}
