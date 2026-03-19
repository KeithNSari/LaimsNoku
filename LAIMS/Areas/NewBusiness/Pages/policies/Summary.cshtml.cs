using LAIMS.Interfaces.Commissions;
using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.Commissions;
using LAIMS.Models.Documents;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Questionnaires;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
	[Authorize(Roles = "New Business Initiator")]
	public class SummaryModel : PageModel
    {
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IMemberRepository _memberRepository;
		private readonly IPolicyTypeRepository _policyTypeRepository;
		private readonly IPolicyTypesLinesRepository _policyTypesLinesRepository;
		private readonly IPolicyRepository _policyRepository;
		private readonly IPolicyBeneficiaryRepository _policyBeneficiaryRepository;
		private readonly IPolicyBeneficiaryLineRepository _policyBeneficiaryLineRepository;
		private readonly IPolicyPremiumRepository _policyPremiumRepository;
		private readonly IMediaUploadRepository _mediaUploadRepository;
		private readonly IQuestionnaireResponseRepository _questionnaireResponseRepository;
		private readonly IEmploymentRepository _employmentRepository;
		private readonly IIntermediaryRepository _intermediaryRepository;
		private readonly IPolicyPremiumLineRepository _policyPremiumLineRepository;
		private readonly IPolicyTypeCommissionRepository _policyTypeCommissionRepository;
		private readonly IPolicyCommissionRepository _policyCommissionRepository;
		public DataTable IntermediariesDT { get; set; }
		public DataTable StatiiHistoryDT { get; set; }
		public SummaryModel(UserManager<ApplicationUser> userManager,
			IMemberRepository memberRepository,
			IPolicyTypeRepository policyTypeRepository,
			IPolicyTypesLinesRepository policyTypesLinesRepository,
			IPolicyRepository policyRepository,
			IPolicyBeneficiaryRepository policyBeneficiaryRepository,
			IPolicyBeneficiaryLineRepository policyBeneficiaryLineRepository,
			IPolicyPremiumRepository policyPremiumRepository,
			IMediaUploadRepository mediaUploadRepository,
			IQuestionnaireResponseRepository questionnaireResponseRepository,
			IEmploymentRepository employmentRepository,
			IIntermediaryRepository intermediaryRepository,
			IPolicyPremiumLinesCommissionRepository policyPremiumLinesCommission,
			IPolicyPremiumLineRepository policyPremiumLineRepository,
			IPolicyTypeCommissionRepository policyTypeCommissionRepository,
			IPolicyCommissionRepository policyCommissionRepository)
		{
			_userManager = userManager;
			_memberRepository = memberRepository;
			_policyTypeRepository = policyTypeRepository;
			_policyTypesLinesRepository = policyTypesLinesRepository;
			_policyRepository = policyRepository;
			_policyBeneficiaryRepository = policyBeneficiaryRepository;
			_policyBeneficiaryLineRepository = policyBeneficiaryLineRepository;
			_policyPremiumRepository = policyPremiumRepository;
			_questionnaireResponseRepository = questionnaireResponseRepository;
			_mediaUploadRepository = mediaUploadRepository;
			_employmentRepository = employmentRepository;
			_intermediaryRepository = intermediaryRepository;
			_policyPremiumLineRepository = policyPremiumLineRepository;
			_policyTypeCommissionRepository = policyTypeCommissionRepository;
			_policyCommissionRepository = policyCommissionRepository;
		}
		[BindProperty]
		public List<PolicyBeneficiaryLineDetail> PolicyBeneficiaryLineDetails { get; set; }
		public List<PBLDocumentUpload> PBLDocumentUploads { get; set; }
		public class FileUploadModel
		{
			public Guid ID { get; set; }
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
		public List<FileUploadModel> FileUploadModelList { get; set; } = new List<FileUploadModel>();
		public List<QuestionnaireResponse> QuestionnaireResponses { get; set; }
		[BindProperty]
		public DateTime ApplicationDate { get; set; }
		[BindProperty]
		public int BillingDay { get; set; }
		[BindProperty]
		public string AgentCodes { get; set; }

		[BindProperty]
		public Guid MemberID { get; set; }
		[BindProperty]
		public Guid PolicyTypeID { get; set; }
		[BindProperty]
		public Member Member { get; set; }
		[BindProperty]
		public PolicyBeneficiaryLine PolicyBeneficiaryLine { get; set; }
		public DataTable MembersDT;
		public DataTable BeneficiariesDT;
		[BindProperty]
		public string? PolicyName { get; set; }

		[BindProperty]
		public Guid PolicyID { get; set; }
		[BindProperty]
		public PolicyPremium PolicyPremium { get; set; }
		public DataTable PremiumDetails { get; set; }
		public int PremiumPayerID { get; set; }
		[BindProperty]
		public Member PremiumPayer { get; set; }
		[BindProperty]
		public int PolicyPremiumID { get; set; }
		[BindProperty]
		public decimal TotalPremiums { get; set; }
		[BindProperty]
		public decimal TotalCover { get; set; }
		[BindProperty]
		public Guid ProposerID { get; set; }
		public DataTable EmploymentDT { get; set; }

		[BindProperty(SupportsGet = true)]
		public string ReturnUrl { get; set; }
		[BindProperty]
		public PolicyDates PolicyDates { get; set; }
		[BindProperty]
		public int StatusID { get; set; }
		[BindProperty]
		public string StatusComment { get; set; }
		public byte[] PdfData { get; set; }
		[BindProperty]
		public bool HasMainLifeAssured { get; set; }
		[BindProperty]
		public bool HasAdditionalLifeAssured { get; set; }
		public DataTable AdditionalLifeDT;
		public DataTable MainLifeDT;
		[BindProperty]
		public int PolicyTerm { get; set; }
		public IActionResult OnGet(Guid id, Guid policyTypeid, Guid policyid)
		{
			PdfData = new byte[0];
			ReturnUrl = Request.Path + Request.QueryString;
			PolicyID = policyid;
			MemberID = id;
			PolicyTypeID = policyTypeid;
			Member = _memberRepository.GetMemberById(id);
			PolicyDates = _policyRepository.GetPolicyDates(policyid);
			TotalCover = _policyRepository.GetTotalCover(PolicyID);
			TotalPremiums = _policyRepository.GetTotalPremiums(PolicyID);
			if (Member == null)
			{
				return NotFound();
			}
			PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
			PolicyTerm = _policyRepository.GetPolicyTerm(PolicyID);
			HasMainLifeAssured = _policyTypeRepository.CheckLIRole(policyTypeid, 1);
			HasAdditionalLifeAssured = _policyTypeRepository.CheckLIRole(policyTypeid, 7);
			BeneficiariesDT = _policyBeneficiaryRepository.GetBeneficiaryList(policyid);
			AdditionalLifeDT = _policyBeneficiaryRepository.GetAdditionalLifeAssured(policyid);
			MainLifeDT = _policyBeneficiaryRepository.GetMainLifeAssured(policyid);
			PolicyBeneficiaryLineDetails = _policyBeneficiaryLineRepository.GetPolicyBeneficiaryLineDetailsByPolicyID(policyid);
			LoadDocumentsData(id, policyTypeid, policyid);
			QuestionnaireResponses = _questionnaireResponseRepository.GetQuestionnaireResponseHeaders(policyid);
			PremiumPayerID = _policyPremiumRepository.GetPremiumPayer(policyid);
			if (PremiumPayerID > 0)
			{
				PremiumPayer = _memberRepository.GetMemberById(PremiumPayerID);
				PolicyPremium = _policyPremiumRepository.GetMainPolicyPremium(policyid);
				PolicyPremiumID = PolicyPremium.ID;
				PremiumDetails = _policyPremiumRepository.GetPremiumDetails(policyid, PolicyPremiumID);
			}
			EmploymentDT = _employmentRepository.GetPolicyEmploymentRecord(policyid);
			IntermediariesDT = _policyPremiumRepository.GetInitialPremiumAgents(policyid);
			StatiiHistoryDT = _policyRepository.GetPolicyStatusHistory(policyid);
			return Page();
		}
		private void LoadDocumentsData(Guid id, Guid policyTypeid, Guid policyid)
		{
			PolicyID = policyid;
			MemberID = id;
			PolicyTypeID = policyTypeid;
			PBLDocumentUploads = _policyBeneficiaryLineRepository.GetPolicyBeneficiaryLineDocuments(policyid);
			for (int i = 0; i < PBLDocumentUploads.Count; i++)
			{
				FileUploadModel fileUploadModel = new FileUploadModel();
				fileUploadModel.ID = PBLDocumentUploads[i].ID;
				fileUploadModel.MediaUploadID = PBLDocumentUploads[i].MediaUploadID;
				fileUploadModel.DocumentID = PBLDocumentUploads[i].DocumentID;
				fileUploadModel.ProductDocumentID = PBLDocumentUploads[i].ProductDocumentID;
				fileUploadModel.FullName = PBLDocumentUploads[i].FullName;
				fileUploadModel.DocumentName = PBLDocumentUploads[i].DocumentName;
				fileUploadModel.ValidationGroup = PBLDocumentUploads[i].ValidationGroup;
				fileUploadModel.Uploaded = PBLDocumentUploads[i].Uploaded;
				fileUploadModel.UploadStatus = PBLDocumentUploads[i].UploadStatus;
				fileUploadModel.UploadedOn = PBLDocumentUploads[i].UploadedOn;
				FileUploadModelList.Add(fileUploadModel);
			}
		}
		public IActionResult OnPostDownloadFile(Guid id, Guid policyTypeid, Guid policyid, Guid docId)
		{
			MediaUpload mediaUpload = _mediaUploadRepository.GetMediaUploadById(docId);
			if (mediaUpload.Data == null || mediaUpload.FileName == null)
			{
				return NotFound();
			}
			var stream = new MemoryStream(mediaUpload.Data);
			var fileStreamResult = new FileStreamResult(stream, "application/octet-stream")
			{
				FileDownloadName = mediaUpload.FileName
			};
			HttpContext.Response.RegisterForDispose(stream);
			return fileStreamResult;
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
	}
}
