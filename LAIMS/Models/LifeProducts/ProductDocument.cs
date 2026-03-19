namespace LAIMS.Models.LifeProducts
{
    public class ProductDocument
    {
        public int ID { get; set; }
        public Guid ProductID { get; set; }
        public Guid DocumentID { get; set; }
        public int LIRoleID { get ; set; }
        public int Tested { get; set; }
        public byte Optional { get; set; }
        public string? ValidationGroup { get; set; }
        public byte Current { get; set; }
        public DateTime? AddedOn { get; set; }
        public string AddedBy { get; set; }
        public byte? Archived { get; set; }
        public string ArchivedBy { get; set; }
        public string ArchivedComment { get; set; }
        public DateTime? ArchivedOn { get; set; }
        public byte? Deleted { get; set; }
        public string DeletedBy { get; set; }
        public string DeletedComment { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
