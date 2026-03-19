namespace LAIMS.Models.Premiums
{
    public class PBLDocumentUpload
    {
       public Guid ID { get; set; }
       public int MemberID { get; set; }
       public Guid MemberUID { get; set; }
       public Guid DocumentID { get; set; }
       public Guid MediaUploadID { get; set; }
       public int ProductDocumentID { get; set; }
       public string FullName { get; set; }    
       public string DocumentName { get; set; }
       public string ValidationGroup { get; set; }
       public int ValidationGroupUploaded { get; set; }
       public bool Uploaded { get; set; }
       public string UploadedOn { get; set; }
       public string UploadStatus { get; set; }
    }
}
