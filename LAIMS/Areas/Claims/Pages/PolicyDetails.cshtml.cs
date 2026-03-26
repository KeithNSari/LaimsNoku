using Azure.Core;
using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Commissions;
using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Claims;
using LAIMS.Models.Documents;
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

namespace LAIMS.Areas.Claims.Pages
{
    [Authorize(Roles = "Claims Initiator")]
    public class PolicyDetailsModel : PageModel
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
        private readonly IPolicyClaimRepository _policyClaimRepository;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        public DataTable IntermediariesDT { get; set; }
        public DataTable StatiiHistoryDT { get; set; }
        public PolicyDetailsModel(UserManager<ApplicationUser> userManager,
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
            IPolicyClaimRepository policyClaimRepository,
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
            _employmentRepository = employmentRepository;
            _policyClaimRepository = policyClaimRepository;
            _businessRuleRepository = businessRuleRepository;
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
        public Policy Policy { get; set; }

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
        public DataTable EmploymentDT { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public PolicyDates PolicyDates { get; set; }
        [BindProperty]
        public int StatusID { get; set; }
        [BindProperty]
        public string StatusComment { get; set; }
        [BindProperty]
        public int ClaimTypeID { get; set; }
        public List<SelectListItem> ClaimTypesList { get; set; } = new List<SelectListItem>();
        [BindProperty] 
        public CoverDetails CoverDetails {get;set;}
        public IActionResult OnGet(Guid id, Guid policyTypeid, Guid policyid)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            try
            {
				PolicyID = policyid;
				MemberID = id;
				PolicyTypeID = policyTypeid;
				Member = _memberRepository.GetMemberById(id);
				PolicyDates = _policyRepository.GetPolicyDates(policyid);
				if (Member == null)
				{
                    throw new Exception("Member cannot be empty!");
				}
				Policy = _policyRepository.GetPolicyById(policyid);
				BeneficiariesDT = _policyBeneficiaryRepository.GetBeneficiaryList(policyid);
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
				CoverDetails = _policyPremiumRepository.GetCoverDetails(policyid);
                LoadClaimTypesSelectList(policyTypeid);
			}
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
			}			
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
        public IActionResult OnGetDownloadPdfFromDatabase(Guid docId)
        {
            MediaUpload mediaUpload = _mediaUploadRepository.GetMediaUploadById(docId);
            if (mediaUpload.Data == null || mediaUpload.FileName == null)
            {
                return NotFound();
            }
            return File(mediaUpload.Data, "application/pdf", mediaUpload.FileName);
        }
        public IActionResult OnPostNext(Guid id, Guid policyTypeid, Guid policyid)
        {
            try
            {
                LoadClaimTypesSelectList(policyTypeid);
                if (ClaimTypeID <= 0 || !ClaimTypesList.Any(item => item.Value == ClaimTypeID.ToString()))
                {
                    ModelState.AddModelError(nameof(ClaimTypeID), "The selected claim type is not configured for this policy.");
                    OnGet(id, policyTypeid, policyid);
                    return Page();
                }

                string addedBy = _userManager.GetUserId(User).ToString();
                Guid requestID = Guid.NewGuid();
                PolicyClaim policyClaim = new()
                {
                    RequestID = requestID,
                    PolicyID = policyid,
                    ClaimTypeID = ClaimTypeID,
                    AddedOn = DateTime.Now,
                    AddedBy = addedBy,
                    StatusID = 3000,
                    StatusAddedBy = addedBy,
                    StatusDate = DateTime.Now
                };
                _policyClaimRepository.AddClaim(policyClaim);
                CheckInitiationRules(policyTypeid,requestID,addedBy);
                switch (ClaimTypeID)
                { 
                    case 1:
                        return Redirect("DeathClaims?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid + "&reqid=" + requestID);

                    default:
                        break;
                }
                return Redirect("PolicyDetails?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid + "&reqid=" + requestID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        private void LoadClaimTypesSelectList(Guid policyTypeID)
        {
            ClaimTypesList = new List<SelectListItem>();
            DataTable claimTypesTable = _policyClaimRepository.GetClaimTypesByPolicyType(policyTypeID);

            foreach (DataRow row in claimTypesTable.Rows)
            {
                ClaimTypesList.Add(new SelectListItem
                {
                    Value = row["ID"].ToString(),
                    Text = row["ClaimType"].ToString()
                });
            }
        }
        private void CheckInitiationRules(Guid PolicyTypeID, Guid RequestID, string AddedBy)
        {
            BusinessRulesParameters claimsSubmissionRulesParameters = new BusinessRulesParameters();
            claimsSubmissionRulesParameters.RequestID = RequestID;
            List<StatusReport> statusReports = _businessRuleRepository.GetAllChecks(PolicyTypeID, "Claims Initiation", claimsSubmissionRulesParameters);
            foreach (StatusReport statusReport in statusReports)
            {
                _policyClaimRepository.UpdateStatus(PolicyTypeID, statusReport.StatusID, AddedBy);
                if (statusReport.SuccessStatus == 0)
                {
                    throw new Exception(statusReport.StatusMessage);
                }
            }           
        }
    }
}
