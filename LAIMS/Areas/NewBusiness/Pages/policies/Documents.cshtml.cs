using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Documents;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Repositories.Membership;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LAIMS.Interfaces.Questionnaires;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using LAIMS.Models.Security;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
	[Authorize(Roles = "New Business Initiator")]
	public class DocumentsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager; 
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IPolicyTypesLinesRepository _policyTypesLinesRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IPolicyBeneficiaryRepository _policyBeneficiaryRepository;
        private readonly IPolicyBeneficiaryLineRepository _policyBeneficiaryLineRepository;
        private readonly IQuestionnaireResponseRepository _questionnaireResponseRepository;
        private readonly IMediaUploadRepository _mediaUploadRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        public List<PBLDocumentUpload> PBLDocumentUploads { get; set; }
        [BindProperty]
        public Guid MemberID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public List<IFormFile> UploadedFiles { get; set; }

        [BindProperty]
        public List<int> DocumentIds { get; set; } = new List<int>();
        [BindProperty(SupportsGet = true)]
        public string SourceUrl { get; set; }

        public class FileUploadModel
        {
            public Guid ID { get; set; }
            public Guid MemberUID { get; set; }
            public Guid DocumentID { get; set; }
            public Guid MediaUploadID { get; set; }
            public int ProductDocumentID { get; set; }
            public string FullName { get; set; }
            public string DocumentName { get; set; }
            public string ValidationGroup { get; set; }
            public bool Uploaded { get; set; }
            public string UploadStatus { get; set; }
            public string UploadedOn { get; set; }
            public IFormFile UploadedFile { get; set; }
        }
        [BindProperty]
        public List<FileUploadModel> FileUploadModelList { get; set; } = new List<FileUploadModel>();

        public DocumentsModel(UserManager<ApplicationUser> userManager, 
            IPolicyTypeRepository policyTypeRepository,
            IPolicyTypesLinesRepository policyTypesLinesRepository,
            IPolicyRepository policyRepository,
            IPolicyBeneficiaryRepository policyBeneficiaryRepository,
            IPolicyBeneficiaryLineRepository policyBeneficiaryLineRepository,
            IQuestionnaireResponseRepository questionnaireResponseRepository,
            IMediaUploadRepository mediaUploadRepository,
            IWebHostEnvironment webHostEnvironment,
            IBusinessRuleRepository businessRuleRepository)
        {
            _userManager = userManager; 
            _policyTypeRepository = policyTypeRepository;
            _policyTypesLinesRepository = policyTypesLinesRepository;
            _policyRepository = policyRepository;
            _policyBeneficiaryRepository = policyBeneficiaryRepository;
            _policyBeneficiaryLineRepository = policyBeneficiaryLineRepository;
            _questionnaireResponseRepository = questionnaireResponseRepository;
            _mediaUploadRepository = mediaUploadRepository;
            _webHostEnvironment = webHostEnvironment;
            _businessRuleRepository = businessRuleRepository;
        }
        public IActionResult OnGet(Guid id, Guid policyTypeid, Guid policyid)
        {
            try
            {
                SourceUrl = Request.Path + Request.QueryString; 
                LoadData(id, policyTypeid, policyid);
				_policyRepository.UpdatePolicyStage(PolicyID, 4);
				return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = SourceUrl });
            }
        }
        private void LoadData(Guid id, Guid policyTypeid, Guid policyid)
        {
            PolicyID = policyid;
            MemberID = id;
            PolicyTypeID = policyTypeid;
            PBLDocumentUploads = _policyBeneficiaryLineRepository.GetPolicyBeneficiaryLineDocuments(policyid);
            for (int i = 0; i < PBLDocumentUploads.Count; i++)
            {
                FileUploadModel fileUploadModel = new FileUploadModel();
               // fileUploadModel.ID = PBLDocumentUploads[i].ID;
                fileUploadModel.MemberUID = PBLDocumentUploads[i].MemberUID;
                fileUploadModel.MediaUploadID = PBLDocumentUploads[i].MediaUploadID;
                fileUploadModel.DocumentID = PBLDocumentUploads[i].DocumentID;
               // fileUploadModel.ProductDocumentID = PBLDocumentUploads[i].ProductDocumentID;
                fileUploadModel.FullName = PBLDocumentUploads[i].FullName;
                fileUploadModel.DocumentName = PBLDocumentUploads[i].DocumentName;
                fileUploadModel.ValidationGroup = PBLDocumentUploads[i].ValidationGroup;
                fileUploadModel.Uploaded = PBLDocumentUploads[i].Uploaded;
                fileUploadModel.UploadStatus = PBLDocumentUploads[i].UploadStatus;
                fileUploadModel.UploadedOn=PBLDocumentUploads[i].UploadedOn;
                FileUploadModelList.Add(fileUploadModel);
            }
        }
       public async Task<IActionResult> OnPostUploadFile(Guid id, Guid policyTypeid, Guid policyid)
        {
            try
            {
                SourceUrl = Request.Path + Request.QueryString;
                if (FileUploadModelList != null && FileUploadModelList.Count > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    foreach (var fileUpload in FileUploadModelList)
                    {
                        if (fileUpload.UploadedFile != null)
                        {
							var extension = Path.GetExtension(fileUpload.UploadedFile.FileName).ToLower();
							if (!extension.Contains(".pdf"))
							{
								throw new Exception($"Only pdf files are allowed!");
							}
							BusinessRulesParameters newBusinessParameters = new()
                            {
                                DocumentFormat = fileUpload.UploadedFile.ContentType,
                                DocumentID = fileUpload.DocumentID,
                                PolicyID=PolicyID,
                                PolicyTypeID=PolicyTypeID,
                                MemberUID= fileUpload.MemberUID,
                                ProposerUID =id
                            };
                            StatusReport statusReport = _businessRuleRepository.CheckRules(PolicyTypeID, "Documents", newBusinessParameters);
                            if (statusReport.SuccessStatus == 0)
                            {
                                throw new Exception(statusReport.StatusMessage);
                            }
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
                                MemberUID = id,
                                DocumentsID = fileUpload.DocumentID,
                                FileName = uniqueFileName,
                                ContentType = contentType,
                                Data = System.IO.File.ReadAllBytes(filePath),
                                AddedOn = DateTime.Now,
                                AddedBy = _userManager.GetUserId(User).ToString()
                            };
                            _mediaUploadRepository.SaveMediaUpload(mediaUpload);
                            _policyBeneficiaryLineRepository.ConfirmDocumentUpload(mediaUpload.ID, policyid,fileUpload.MemberUID,fileUpload.DocumentID);
                        }
                    }
                }                     
                return Redirect("Documents?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = SourceUrl });
            }
        }
        //public IActionResult OnPostDownloadFile(Guid id, Guid policyTypeid, Guid policyid, Guid docId)
        //{
        //    MediaUpload mediaUpload = _mediaUploadRepository.GetMediaUploadById(docId);
        //    if (mediaUpload.Data == null || mediaUpload.FileName == null)
        //    {
        //        return NotFound();
        //    }
        //    var stream = new MemoryStream(mediaUpload.Data);
        //    var fileStreamResult = new FileStreamResult(stream, "application/octet-stream")
        //    {
        //        FileDownloadName = mediaUpload.FileName
        //    };
        //    HttpContext.Response.RegisterForDispose(stream);
        //    return fileStreamResult;
        //}
        public IActionResult OnGetDownloadPdfFromDatabase(Guid docId)
        {
            try
            {
                SourceUrl = Request.Path + Request.QueryString;
                MediaUpload mediaUpload = _mediaUploadRepository.GetMediaUploadById(docId);
                if (mediaUpload.Data == null || mediaUpload.FileName == null)
                {
                    return NotFound();
                }
                return File(mediaUpload.Data, "application/pdf", mediaUpload.FileName);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = SourceUrl });
            }           
        }
        public IActionResult OnPostNext(Guid id, Guid policyTypeid, Guid policyid)
        {
            try
            {  
                string addedBy = _userManager.GetUserId(User).ToString();
                SourceUrl = Request.Path + Request.QueryString;
                //DataTable DT = _policyBeneficiaryRepository.GetRequiredPolicyDocumentsList(policyid, testedBusiness);
                //foreach (DataRow DR in DT.Rows)
                //{
                //    int policyBeneficiaryLineID = Convert.ToInt32(DR["PolicyBeneficiaryLineID"]);
                //    int productDocumentID = Convert.ToInt32(DR["ProductDocumentID"]);
                //    _policyBeneficiaryLineRepository.InsertPolicyBeneficiaryLineDocument(policyBeneficiaryLineID, productDocumentID, addedBy);
                //}              
                //_policyRepository.UpdatePolicyStatus(policyid, 3, string.Empty, addedBy);
                BusinessRulesParameters newBusinessParameters = new BusinessRulesParameters();
                newBusinessParameters.PolicyTypeID = PolicyTypeID;
                newBusinessParameters.PolicyID = PolicyID;
                newBusinessParameters.ProposerUID = id;
                StatusReport statusReport = _businessRuleRepository.CheckRules(PolicyTypeID, "Documents", newBusinessParameters);
                if (statusReport.SuccessStatus == 0)
                {
                    throw new Exception(statusReport.StatusMessage);
                }
                int questionnaireCount = _questionnaireResponseRepository.CountQuestionnaireResponses(policyid);
                if (questionnaireCount > 0)
                {
                    return Redirect("PBQuestionnaires?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
                }
                else
                {
                    return Redirect("Submission?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = SourceUrl });
            }           
        }
    }
}
