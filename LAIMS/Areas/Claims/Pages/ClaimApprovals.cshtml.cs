using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Claims;
using LAIMS.Models.Documents;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.Claims.Pages
{
  [Authorize(Roles = "Claims Approver")]
    public class ClaimApprovalsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMediaUploadRepository _mediaUploadRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDocumentsRepository _documentsRepository;
        private readonly IPolicyClaimRepository _policyClaimRepository;
        private readonly IPolicyPremiumLineRepository _policyPremiumLineRepository;
        private readonly IPolicyPremiumRepository _policyPremiumRepository;
        private readonly IObjectRulesStatiiHistoryRepository _objectRulesStatiiHistoryRepository;
        private readonly IProcessPayments _processPayments;
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
        [BindProperty]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public int StatusID { get; set; }
        [BindProperty]
        public string StatusComment { get; set; }
        [BindProperty]
        public List<DeathRecord> DeathRecords { get; set; }
        public DataTable BasicCoverClaimantsDT;
        public DataTable SupplementaryCoverClaimantsDT;
        public DataTable CoverDT;
        public DataTable ServiceBreakdownDT;
        public DataTable StatiiHistoryDT { get; set; }
        [BindProperty]
        public string SystemDecision { get; set; }
        public DataTable SubmittedByDT;
        [BindProperty ]
        public decimal DisbursementAmount { get; set; }
        [BindProperty ]
        public bool ClaimAccepted { get; set; }
        public DataTable UnpaidDT;
        [BindProperty]
        public decimal UnpaidTotal { get; set; } = 0;
        public DataTable ExpensesDT;
        [BindProperty]
        public decimal ExpensesTotal { get; set; } = 0;
        public ClaimApprovalsModel(UserManager<ApplicationUser> userManager, IMediaUploadRepository mediaUploadRepository,
            IWebHostEnvironment webHostEnvironment, IPolicyClaimRepository policyClaimRepository, 
            IPolicyPremiumRepository policyPremiumRepository, IPolicyPremiumLineRepository policyPremiumLineRepository, 
            IDocumentsRepository documentsRepository, IObjectRulesStatiiHistoryRepository objectRulesStatiiHistoryRepository,
            IProcessPayments processPayments)
        {
            _userManager = userManager;
            _mediaUploadRepository = mediaUploadRepository;
            _webHostEnvironment = webHostEnvironment;
            _documentsRepository = documentsRepository;
            _policyClaimRepository = policyClaimRepository;
            _objectRulesStatiiHistoryRepository = objectRulesStatiiHistoryRepository;
            _policyPremiumRepository = policyPremiumRepository;
            _policyPremiumLineRepository = policyPremiumLineRepository;
            _processPayments = processPayments;
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
        }
        [BindProperty]
        public List<FileUploadModel> FileUploadModelList { get; set; } = new List<FileUploadModel>(); 
        public void OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            ProposerID = id;
            PolicyTypeID = policyTypeid;
            PolicyID = policyid;
            RequestID = reqid;
            ReturnUrl = Request.Path + Request.QueryString;
            LoadDeathRecords(RequestID);
            LoadCoverDetails();
            LoadFileUploadList();
            StatiiHistoryDT = _objectRulesStatiiHistoryRepository.GetHistory(reqid);
            int failed = _policyClaimRepository.GetSystemDecision(RequestID); 
            if(failed>0)
            {
                ClaimAccepted = false;
                SystemDecision = "Claim denied";
            }
            else
            {
                ClaimAccepted = true;
                SystemDecision = "Claim Accepted";
            }
            DisbursementAmount = _policyClaimRepository.GetDisbursementAmount(RequestID);
            UnpaidDT = _policyClaimRepository.GetUnpaidPremiums(policyid);
            if (UnpaidDT.Rows.Count > 0)
            {
                UnpaidTotal = _policyClaimRepository.GetUnpaidPremiumsBalance(policyid);
            }
            ExpensesDT = _policyClaimRepository.GetExpenses(RequestID); 
            if (ExpensesDT.Rows.Count > 0)
            {
                ExpensesTotal = _policyClaimRepository.GetExpensesTotal(RequestID);
            }
        }
        private void LoadDeathRecords(Guid RequestID)
        {
            int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
            DeathRecords = _policyClaimRepository.GetAllDeathRecords(claimID);
        }
        private void LoadCoverDetails()
        {
            int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
            CoverDT = _policyClaimRepository.GetClaimCover(claimID);
            BasicCoverClaimantsDT = _policyClaimRepository.GetPolicyClaimants(claimID, 1);
            SupplementaryCoverClaimantsDT = _policyClaimRepository.GetPolicyClaimants(claimID, 0);
            ServiceBreakdownDT = _policyClaimRepository.GetServiceBreakdown(claimID);
            SubmittedByDT = _policyClaimRepository.GetSubmittedBy(claimID);
        }
        private void LoadFileUploadList()
        {
            foreach (Models.LifeProducts.Document document in _documentsRepository.GetClaimRequiredDocuments(RequestID))
            {
                FileUploadModel fileUploadModel = new()
                {
                    DocumentName = document.DocumentName,
                    DocumentID = document.ID,
                    AddedOn = document.AddedOn,
                    FilingNo = document.FilingNo,
                    Uploaded = document.Uploaded,
                    MediaUploadID = document.UploadID
                };
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
        public IActionResult OnPost()
        {
            try
            {
                using (TransactionScope TS = new TransactionScope())
                {
                    string AddedBy = _userManager.GetUserId(User).ToString();
                    _policyClaimRepository.UpdateStatus(RequestID, StatusID, AddedBy);
                    ExpensesTotal = _policyClaimRepository.GetExpensesTotal(RequestID);
                    UnpaidTotal = _policyClaimRepository.GetUnpaidPremiumsBalance(PolicyID);
                    _policyClaimRepository.UpdateDeductions(RequestID, ExpensesTotal + UnpaidTotal);
                    if (StatusID == 3300)
                    {
                        if (DisbursementAmount <= 0) throw new Exception("Disbursement amount should be greater than 0.");
                        _policyClaimRepository.UpdateDisbursementAmount(RequestID, DisbursementAmount);
                        ObjectRulesStatiiHistory objectRulesStatiiHistory = new()
                        {
                            RequestID = RequestID,
                            SourceID = 5, //5 allocated to claims, 1 is new business, 2 premiums, 3 commissions, 4 policy servicing
                            MemberID = 0,
                            StatusRuleID = Guid.Parse("fd26d856-e192-4cf6-9b0b-8e8c88d0152f"),
                            SuccessStatus = 1,
                            Status = 3300,
                            StatusReason = 0,
                            StatusComment = StatusComment,
                            StatusDate = DateTime.Now,
                            StatusAddedBy = AddedBy
                        };
                        _objectRulesStatiiHistoryRepository.Add(objectRulesStatiiHistory);
                    }
                    if (StatusID == 3250)
                    {
                        if ((UnpaidTotal < DisbursementAmount) && (UnpaidTotal > 0))
                        {
                            UnpaidDT = _policyClaimRepository.GetUnpaidPremiums(PolicyID);
                            foreach (DataRow dr in UnpaidDT.Rows)
                            {
                                decimal amount = Convert.ToDecimal(dr["Amount"]);
                                _processPayments.AdhocPayment(PolicyID, RequestID.ToString(), amount, 6, 0, DateTime.Now, AddedBy);
                            }
                        }
                        int memberID = _policyClaimRepository.GetMemberID(RequestID);
                        _policyPremiumRepository.ArchiveBeneficiaryPremiumLines(PolicyID, memberID);
                        _policyPremiumLineRepository.UpdatePolicyPremiumLines(PolicyID);
                        _policyPremiumRepository.UpdatePremiumAmount(PolicyID);
                        _policyClaimRepository.DeathClaimUpdatePolicyStatus(RequestID);
                        _policyClaimRepository.UpdatePolicyStatus(RequestID, AddedBy);
                    } 
                    TS.Complete();
                }
                return Redirect("MyReviews");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
}
