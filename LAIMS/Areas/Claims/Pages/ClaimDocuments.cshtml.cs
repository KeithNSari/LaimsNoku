using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Documents;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Repositories.Policies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using System.Data;
using LAIMS.Interfaces.Claims;
using LAIMS.Models.BusinessRules;
using Microsoft.AspNetCore.Authorization;
using LAIMS.Models.Security;

namespace LAIMS.Areas.Claims.Pages
{
    [Authorize(Roles = "Claims Initiator")]
    public class ClaimDocumentsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMediaUploadRepository _mediaUploadRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDocumentsRepository _documentsRepository;
        private readonly IPolicyClaimRepository _policyClaimRepository; 
        [BindProperty]
        public Policy Policy { get; set; }

        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid ProposerID { get; set; }
        [BindProperty]
        public Guid RequestID { get; set; } 
        public ClaimDocumentsModel(UserManager<ApplicationUser> userManager, IMediaUploadRepository mediaUploadRepository, 
            IWebHostEnvironment webHostEnvironment, IPolicyClaimRepository policyClaimRepository,
            IDocumentsRepository documentsRepository)
        {
            _userManager = userManager;
            _mediaUploadRepository = mediaUploadRepository; 
            _webHostEnvironment = webHostEnvironment;
            _documentsRepository = documentsRepository;
            _policyClaimRepository = policyClaimRepository;
        }
        public class FileUploadModel
        {
            public Guid ID { get; set; } 
            public Guid DocumentID { get; set; }
            public string DocumentName { get; set; }
            public string FilingNo { get; set; }
            public DateTime? AddedOn { get; set; }
            public Guid MediaUploadID { get; set; } 
            public bool Uploaded { get; set; }
            public IFormFile UploadedFile { get; set; }
        }
        [BindProperty]
        public List<FileUploadModel> FileUploadModelList { get; set; } = new List<FileUploadModel>();

        public void OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            ProposerID = id;
            PolicyTypeID = policyTypeid;
            PolicyID = policyid;
            RequestID = reqid; 
            LoadFileUploadList(); 
        } 
        public async Task<IActionResult> OnPostUploadFile()
        {
            try
            {
                if (FileUploadModelList != null && FileUploadModelList.Count > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    foreach (var fileUpload in FileUploadModelList)
                    {
                        if (fileUpload.UploadedFile != null)
                        {                             
                            var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileUpload.UploadedFile.FileName;
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                            string contentType = fileUpload.UploadedFile.ContentType;
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await fileUpload.UploadedFile.CopyToAsync(fileStream);
                            } 
                            var mediaUpload = new MediaUpload
                            {
                                ID = Guid.NewGuid(),
                                MemberUID = ProposerID,
                                DocumentsID = fileUpload.DocumentID,
                                FileName = uniqueFileName,
                                ContentType = contentType,
                                Data = System.IO.File.ReadAllBytes(filePath),
                                AddedOn = DateTime.Now,
                                AddedBy = _userManager.GetUserId(User).ToString()
                            };
                            _mediaUploadRepository.SaveMediaUpload(mediaUpload);
                            _policyClaimRepository.AddClaimDocument(RequestID, fileUpload.DocumentID, mediaUpload.ID, mediaUpload.AddedBy);                          
                        }
                    }
                }
                return Redirect("ClaimDocuments?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return Page();
            }
        }
        public IActionResult OnGetDownloadPdfFromDatabase(Guid docId)
        {
            MediaUpload mediaUpload = _mediaUploadRepository.GetMediaUploadById(docId);
            if (mediaUpload.Data == null || mediaUpload.FileName == null)
            {
                return NotFound();
            }
            return File(mediaUpload.Data, "application/pdf", mediaUpload.FileName);
        }
        private void LoadFileUploadList()
        {
            foreach (Models.LifeProducts.Document document in _documentsRepository.GetClaimRequiredDocuments(RequestID))
            {
                FileUploadModel fileUploadModel = new()
                {
                    DocumentName = document.DocumentName,
                    DocumentID = document.ID,
                    AddedOn =document.AddedOn,
                    FilingNo= document.FilingNo,
                    Uploaded=document.Uploaded,
                    MediaUploadID = document.UploadID
                };
                FileUploadModelList.Add(fileUploadModel); 
            }
        }
        public IActionResult OnPostNext()
        {
            try
            {
                return Redirect("SubmitClaim?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = "" });
            }
        }
    }
}
