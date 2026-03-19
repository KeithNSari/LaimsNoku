using Azure.Core;
using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Investments;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.Claims;
using LAIMS.Models.Documents;
using LAIMS.Models.Investments;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Pages.NewBusiness.Pages.policies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;
using System.Transactions;
using Microsoft.AspNetCore.Mvc.Rendering;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Banking;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Banking;
using LAIMS.Interfaces.BusinessRules;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
namespace LAIMS.Areas.Claims.Pages
{
	[Authorize(Roles = "Claims Approver")]
	public class PUFinalApprovalModel : PageModel
    { 
            private readonly IPolicyRepository _policyRepository;
            private readonly IWebHostEnvironment _webHostEnvironment;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IUnitTrustRepository _unitTrustRepository;
            private readonly IUnitsPricesListRepository _unitsPricesListRepository;
            private readonly IPolicyClaimRepository _policyClaimRepository;
            private readonly IDocumentsRepository _documentsRepository;
            private readonly IMediaUploadRepository _mediaUploadRepository;
            private readonly IRelationshipRepository _relationshipRepository;
            private readonly ICountryRepository _countryRepository;
            private readonly ICityRepository _cityRepository;
            private readonly IGenderRepository _genderRepository;
            private readonly IMemberRepository _memberRepository;
            private readonly IMemberContactRepository _memberContactRepository;
            private readonly IBankBranchRepository _bankBranchRepository;
            private readonly IObjectRulesStatiiHistoryRepository _objectRulesStatiiHistoryRepository;
            public List<SelectListItem> GenderList = new List<SelectListItem>();
            public List<SelectListItem> CountrySelectList = new List<SelectListItem>();
            public List<SelectListItem> MyRelationships = new List<SelectListItem>();
            public List<SelectListItem> MyBankBranches = new List<SelectListItem>();
            public List<Gender> Genders { get; set; }
            public DataTable PoliciesDT;
            public DataTable UnitTrustBalancesDT;
            [BindProperty]
            public Guid ProposerID { get; set; }
            [BindProperty]
            public Guid RequestID { get; set; }
            public string? IDContent { get; set; }
            [BindProperty]
            public string? ConfirmIDContent { get; set; }
            [BindProperty]
            public Member NewMember { get; set; }
            [BindProperty]
            public int RelationshipID { get; set; } = -1;
            [BindProperty]
            public ContactCard ContactCard { get; set; }
            public List<Country> Countries { get; set; }
            [BindProperty(SupportsGet = true)]
            public int CountryID { get; set; }
            [BindProperty]
            public string CustomCity { get; set; }
            public DataTable ExpensesDT;
            [BindProperty]
            public decimal ExpensesTotal { get; set; } = 0;
            [BindProperty]
            public DateTime SignedOn { get; set; }
            [BindProperty]
            public int SubmittedBankBranchID { get; set; }
            public DataTable SubmittedByDT;
            public DataTable StatiiHistoryDT { get; set; }
            [BindProperty]
            public string SystemDecision { get; set; }
            [BindProperty]
            public decimal DisbursementAmount { get; set; }
            [BindProperty]
            public bool ClaimAccepted { get; set; }
            public List<BankBranch> BankBranches { get; set; }
            [BindProperty]
            public string ReturnUrl { get; set; }
            [BindProperty]
            public int StatusID { get; set; }
            [BindProperty]
            public string StatusComment { get; set; }
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

            public PUFinalApprovalModel(UserManager<ApplicationUser> userManager, IPolicyRepository policyRepository,
                IWebHostEnvironment webHostEnvironment, IUnitsPricesListRepository unitsPricesListRepository,
                IUnitTrustRepository unitTrustRepository, IPolicyClaimRepository policyClaimRepository,
                IDocumentsRepository documentsRepository, IMediaUploadRepository mediaUploadRepository,
                IRelationshipRepository relationshipRepository, IGenderRepository genderRepository,
                IObjectRulesStatiiHistoryRepository objectRulesStatiiHistoryRepository,
                ICountryRepository countryRepository, ICityRepository cityRepository,
                IBankBranchRepository bankBranchRepository,
                IMemberRepository memberRepository, IMemberContactRepository memberContactRepository)
            {
                _userManager = userManager;
                _policyRepository = policyRepository;
                _webHostEnvironment = webHostEnvironment;
                _unitTrustRepository = unitTrustRepository;
                _unitsPricesListRepository = unitsPricesListRepository;
                _policyClaimRepository = policyClaimRepository;
                _documentsRepository = documentsRepository;
                _mediaUploadRepository = mediaUploadRepository;
                _relationshipRepository = relationshipRepository;
                _countryRepository = countryRepository;
                _cityRepository = cityRepository;
                _memberRepository = memberRepository;
                _memberContactRepository = memberContactRepository;
                _genderRepository = genderRepository;
                _bankBranchRepository = bankBranchRepository;
                _objectRulesStatiiHistoryRepository = objectRulesStatiiHistoryRepository;
            }
            private void LoadRelationshipsSelectList()
            {
                foreach (Relationship relationship in _relationshipRepository.GetRelationships())
                {
                    MyRelationships.Add(new SelectListItem
                    {
                        Value = relationship.ID.ToString(),
                        Text = relationship.RelationshipName
                    });
                }
            }
            private void LoadCountrySelectList()
            {
                Countries = _countryRepository.GetAllCountries();
                foreach (Country country in Countries)
                {
                    CountrySelectList.Add(new SelectListItem
                    {
                        Value = country.CountryID.ToString(),
                        Text = country.CountryName
                    });
                }
            }
            public JsonResult OnGetCitiesByCountry()
            {
                var cities = _cityRepository.GetCitiesByCountry(CountryID);
                return new JsonResult(cities);
            }
            private void LoadGenderSelectList()
            {
                Genders = _genderRepository.GetAllGenders();
                foreach (Gender gender in Genders)
                {
                    GenderList.Add(new SelectListItem
                    {
                        Value = gender.Id.ToString(),
                        Text = gender.GenderName
                    });
                }
            }
            private void LoadBankBranchesSelectList()
            {
                BankBranches = _bankBranchRepository.GetAllBankBranches();
                foreach (BankBranch bankBranch in BankBranches)
                {
                    MyBankBranches.Add(new SelectListItem
                    {
                        Value = bankBranch.EntryNo.ToString(),
                        Text = bankBranch.BranchName
                    });
                }
            }
            public void OnGet(Guid id, Guid reqid)
            {
                ProposerID = id;
                RequestID = reqid;
                string PolicyNo = _policyClaimRepository.GetPolicyNo(RequestID);
                int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
                PoliciesDT = _policyRepository.PolicyInvestmentSummary(PolicyNo);
                UnitTrustBalancesDT = _policyClaimRepository.GetProposalDetails(claimID);
                LoadFileUploadList();
                LoadRelationshipsSelectList();
                LoadCountrySelectList();
                LoadGenderSelectList();
                LoadBankBranchesSelectList();
                SignedOn = _policyClaimRepository.GetSignedDate(RequestID);
                SubmittedBankBranchID = _policyClaimRepository.GetSubmittedBankBranch(RequestID);
                SubmittedByDT = _policyClaimRepository.GetSubmittedBy(claimID);
                //TransactionHistoryDT = _unitTrustRepository.GetLatestSalesTransactions(SearchTerm); 
                StatiiHistoryDT = _objectRulesStatiiHistoryRepository.GetHistory(reqid);
                int failed = _policyClaimRepository.GetSystemDecision(RequestID);
                if (failed > 0)
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
                ExpensesDT = _policyClaimRepository.GetExpenses(RequestID);
                if (ExpensesDT.Rows.Count > 0)
                {
                    ExpensesTotal = _policyClaimRepository.GetExpensesTotal(RequestID);
                }
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
                    _policyClaimRepository.UpdateDeductions(RequestID, ExpensesTotal);
                    if (StatusID == 3350)
                    {
                        string PolicyNo = _policyClaimRepository.GetPolicyNo(RequestID);
                        int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
                        int CurrencyID = _policyRepository.GetPolicyCurrency(PolicyNo);
                        Guid policyID = _policyRepository.GetPolicyID(PolicyNo);
                        Guid trustID = _policyClaimRepository.GetClaimTrust(claimID);
                        UnitsPricesList unitLatestPricing = _unitsPricesListRepository.GetLatestPrice(CurrencyID, trustID);
                        if (unitLatestPricing.BidPrice > 0)
                        {
							DisbursementAmount = _policyClaimRepository.GetDisbursementAmount(RequestID);
                            decimal purchaseQuantity = Math.Round(DisbursementAmount / unitLatestPricing.BidPrice,7); 
							decimal totalAvailableUnits = _unitTrustRepository.GetTotalAvailableunits(policyID, trustID);
                            if ((purchaseQuantity > 0) && (purchaseQuantity <= totalAvailableUnits))
                            {
                                int ID = _unitsPricesListRepository.GetPolicyUnitsHeader(policyID, trustID);
                                int policyunitsHeaderID = ID == 0 ? _unitsPricesListRepository.InsertPolicyUnitsHeader(policyID, trustID, AddedBy) : ID;
                                _unitsPricesListRepository.Sell(policyID, trustID, claimID, policyunitsHeaderID, purchaseQuantity, unitLatestPricing.ID, 2, AddedBy);
                            }
                            else if (purchaseQuantity > totalAvailableUnits)
                            {
                                throw new Exception("Units purchased cannot exceed available quantity!");
                            }
                        }
                        // _policyClaimRepository.PurchaseMade(claimID); 

                        ObjectRulesStatiiHistory objectRulesStatiiHistory = new()
                        {
                            RequestID = RequestID,
                            SourceID = 5, //5 allocated to claims, 1 is new business, 2 premiums, 3 commissions, 4 policy servicing
                            MemberID = 0,
                            StatusRuleID = Guid.Parse("D4D4762B-B336-4C19-8F48-5ADD6919FE56"),
                            SuccessStatus = 1,
                            Status = 3350,
                            StatusReason = 0,
                            StatusComment = StatusComment,
                            StatusDate = DateTime.Now,
                            StatusAddedBy = AddedBy
                        };
                        _objectRulesStatiiHistoryRepository.Add(objectRulesStatiiHistory); 
                        _policyClaimRepository.UpdateInvestmentPolicyStatus(RequestID); 
                       
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

