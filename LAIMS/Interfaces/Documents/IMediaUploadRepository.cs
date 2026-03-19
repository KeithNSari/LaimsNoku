using LAIMS.Models.Documents;
using System.Data;

namespace LAIMS.Interfaces.Documents
{
    public interface IMediaUploadRepository
    {
        void SaveMediaUpload(MediaUpload mediaUpload);
        MediaUpload GetMediaUploadById(Guid id);
        List<MediaUpload> SearchMediaUploads(string filingNo, string documentNo);
        DataTable GetDocuments(Guid MemberUID);
        void UpdateMediaUpload(MediaUpload mediaUpload);
        void DeleteMediaUpload(Guid id);
    }
}
