using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.BusinessRules;
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
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
	[Authorize(Roles = "New Business Initiator")]
	public class SubmissionModel : PageModel
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
        private readonly IBusinessRuleRepository _businessRuleRepository;
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
        [BindProperty ]
        public DateTime ApplicationDate { get; set; }
        [BindProperty]
        public DateTime ProposedStartDate { get; set; }
        [BindProperty]
        public int BillingDay { get; set; }
        [BindProperty]
        public string AgentCodes { get; set; }
        public DataTable AdditionalLifeDT;
        public DataTable MainLifeDT;
        [BindProperty]
        public bool HasInvestmentContent { get; set; }
        [BindProperty]
        public bool HasMainLifeAssured { get; set; }
        [BindProperty]
        public bool HasAdditionalLifeAssured { get; set; }
        [BindProperty]
        public bool HasRiskProduct { get; set; }
		[BindProperty]
		public int PolicyTerm { get; set; }
		public SubmissionModel(UserManager<ApplicationUser> userManager,
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
            IBusinessRuleRepository businessRuleRepository)
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
            _employmentRepository=employmentRepository;
            _intermediaryRepository = intermediaryRepository;
            _businessRuleRepository = businessRuleRepository;
        }
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
        public PolicyDates PolicyDates { get; set; }
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
        public Guid ProposerID { get; set; }
        [BindProperty]
        public decimal TotalPremiums { get; set; }
        [BindProperty]
        public decimal TotalCover { get; set; }
        public DataTable EmploymentDT { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public IActionResult OnGet(Guid id, Guid policyTypeid, Guid policyid)
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                PolicyID = policyid;
                MemberID = id;
                PolicyTypeID = policyTypeid;
                _policyRepository.UpdatePolicyStage(PolicyID, 6);
                Member = _memberRepository.GetMemberById(id);
                ApplicationDate = DateTime.Today;
                ProposedStartDate = DateTime.Today;
                TotalCover = _policyRepository.GetTotalCover(PolicyID);
                TotalPremiums = _policyRepository.GetTotalPremiums(PolicyID);
                if (Member == null)
                {
                    return NotFound();
                }
                PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
                PolicyDates = _policyRepository.GetPolicyDates(policyid);
				PolicyTerm = _policyRepository.GetPolicyTerm(PolicyID);
				HasInvestmentContent = _policyTypeRepository.HasInvestmentProduct(policyTypeid);
                HasMainLifeAssured = _policyTypeRepository.CheckLIRole(policyTypeid, 1);
                HasAdditionalLifeAssured = _policyTypeRepository.CheckLIRole(policyTypeid, 7);
                HasRiskProduct = _policyTypeRepository.HasRiskProduct(policyTypeid);
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
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
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
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
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
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostNext(Guid id, Guid policyTypeid, Guid policyid)
        {
			try
			{
				using (TransactionScope TS = new TransactionScope())
				{
					string AddedBy = _userManager.GetUserId(User).ToString();
					ReturnUrl = Request.Path + Request.QueryString;
					int policyPremiumID = _policyPremiumRepository.UpdatePolicyPremium(policyid, AgentCodes, PolicyDates.PreferredBillingDay);
					if (policyPremiumID > 0)
					{
						string[] agentCodes = AgentCodes.Split(',');
						foreach (string agentCode in agentCodes)
						{
							if (!_intermediaryRepository.CheckIntermediaryCode(agentCode))
							{
								throw new Exception("Invalid Agent Code: " + agentCode);
							}
							_policyPremiumRepository.AddPolicyPremiumIntermediaries(policyPremiumID, agentCode);
						}
						_policyPremiumRepository.AddSupervisors(policyPremiumID);
					}
					CheckSubmissionRules(policyTypeid, policyid, id, AddedBy);
					bool overrideSequence = true;
					int StatusID = 6;//Awaiting Approval
					int stageID = 7; //Under Review
					_policyRepository.UpdatePolicyStatus(overrideSequence, policyid, stageID, StatusID, string.Empty, AddedBy);
					TS.Complete();
				}
				return Redirect("Summary?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
			}
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new
				{
					errorMessage = "An error occurred during processing. " + ex.Message,
					returnUrl = ReturnUrl
				});
			}
		}
        private void CheckSubmissionRules(Guid PolicyTypeID, Guid PolicyID, Guid ProposerUID, string AddedBy)
        {
            BusinessRulesParameters newBusinessParameters = new BusinessRulesParameters();
            newBusinessParameters.PolicyTypeID = PolicyTypeID;
            newBusinessParameters.ProposerUID = ProposerUID;
            newBusinessParameters.PolicyID = PolicyID;
            List<StatusReport> statusReports = _businessRuleRepository.GetAllChecks(PolicyTypeID, "Submission", newBusinessParameters);
            foreach (StatusReport statusReport in statusReports)
            {
                _policyRepository.UpdatePolicyStatus(PolicyID, 6, statusReport.RuleID, statusReport.StatusID, statusReport.StatusReasonID, statusReport.StatusMessage, AddedBy);
            } 
        }
        public IActionResult OnGetDownloadPdfFromDatabase(Guid docId)
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                MediaUpload mediaUpload = _mediaUploadRepository.GetMediaUploadById(docId);
                if (mediaUpload.Data == null || mediaUpload.FileName == null)
                {
                    return NotFound();
                }
                return File(mediaUpload.Data, "application/pdf", mediaUpload.FileName);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new
                {
                    errorMessage = "An error occurred during processing. " + ex.Message,
                    returnUrl = ReturnUrl
                });
            }
        }
    }
}
