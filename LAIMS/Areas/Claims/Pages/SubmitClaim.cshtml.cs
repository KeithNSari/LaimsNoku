using LAIMS.Interfaces.Banking;
using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Models.Banking;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Claims;
using LAIMS.Models.Documents;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Security;
using LAIMS.Repositories.Lifeproducts;
using LAIMS.Repositories.Membership;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Protocol.Plugins;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace LAIMS.Areas.Claims.Pages
{
    [Authorize(Roles = "Claims Initiator")]
    public class SubmitClaimModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMediaUploadRepository _mediaUploadRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDocumentsRepository _documentsRepository;
        private readonly IPolicyClaimRepository _policyClaimRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IGenderRepository _genderRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IMemberContactRepository _memberContactRepository;
        private readonly IBankBranchRepository _bankBranchRepository;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        private readonly IObjectRulesStatiiHistoryRepository _objectRulesStatiiHistoryRepository; 
        public List<SelectListItem> GenderList = new List<SelectListItem>();
        public List<SelectListItem> CountrySelectList = new List<SelectListItem>();
        public List<SelectListItem> MyRelationships = new List<SelectListItem>();
        public List<SelectListItem> MyBankBranches = new List<SelectListItem>();
        public List<Gender> Genders { get; set; }
        [BindProperty]
        public Policy Policy { get; set; }
        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid ProposerID { get; set; }
        [BindProperty]
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
        [BindProperty]
        public Guid RequestID { get; set; }
        [BindProperty]
        public DateTime SignedOn { get; set; }
        [BindProperty]
        public int SubmittedBankBranchID { get; set; }
        public int BankBranchID { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public List<DeathRecord> DeathRecords { get; set; }
        public List<BankBranch> BankBranches { get; set; } 
        public DataTable BasicCoverClaimantsDT;
        public DataTable SupplementaryCoverClaimantsDT;
        public DataTable CoverDT;
        public DataTable ServiceBreakdownDT;
        public DataTable SubmittedByDT;
        [BindProperty]
        public List<ClaimExpense> ClaimExpenses { get; set; }
        //[BindProperty]
        //public Dictionary<int, int> SelectedOptions { get; set; }
        public decimal OverallTotal => ClaimExpenses.Where(e => e.IsSelected).Sum(e => e.Price);
        public DataTable UnpaidDT;
        [BindProperty]
        public decimal UnpaidTotal { get; set; } = 0;
		[BindProperty]
		public string SubmittedByType { get; set; }  // "PolicyHolder" or "Other"s
		public SubmitClaimModel(UserManager<ApplicationUser> userManager, IMediaUploadRepository mediaUploadRepository,
            IWebHostEnvironment webHostEnvironment, IPolicyClaimRepository policyClaimRepository,
            IDocumentsRepository documentsRepository, IRelationshipRepository relationshipRepository, IGenderRepository genderRepository,
            ICountryRepository countryRepository, ICityRepository cityRepository,IBankBranchRepository bankBranchRepository,
            IMemberRepository memberRepository, IMemberContactRepository memberContactRepository,IBusinessRuleRepository businessRuleRepository,
            IObjectRulesStatiiHistoryRepository objectRulesStatiiHistoryRepository)
        {
            _userManager = userManager;
            _mediaUploadRepository = mediaUploadRepository;
            _webHostEnvironment = webHostEnvironment;
            _documentsRepository = documentsRepository;
            _policyClaimRepository = policyClaimRepository;
            _relationshipRepository = relationshipRepository;
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
            _memberRepository = memberRepository;
            _memberContactRepository = memberContactRepository;
            _genderRepository = genderRepository;
            _bankBranchRepository = bankBranchRepository;
            _businessRuleRepository = businessRuleRepository;
            _objectRulesStatiiHistoryRepository = objectRulesStatiiHistoryRepository;
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
            foreach (BankBranch bankBranch in BankBranches  )
            {
                MyBankBranches.Add(new SelectListItem
                {
                    Value = bankBranch.EntryNo.ToString(),
                    Text = bankBranch.BranchName
                });
            }
        }
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
            LoadRelationshipsSelectList();
            LoadCountrySelectList();
            LoadGenderSelectList();
            LoadBankBranchesSelectList();
            SignedOn = _policyClaimRepository.GetSignedDate(RequestID);
            SubmittedBankBranchID = _policyClaimRepository.GetSubmittedBankBranch(RequestID);
            ClaimExpenses = _policyClaimRepository.GetClaimExpensePriceList(RequestID);
            UnpaidDT = _policyClaimRepository.GetUnpaidPremiums(policyid);
            if (UnpaidDT.Rows.Count > 0)
            {
                UnpaidTotal = _policyClaimRepository.GetUnpaidPremiumsBalance(policyid);
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
        public IActionResult OnPost(int?[] SelectedOptions)
        {
            try
            {
                if ( SignedOn>DateTime.Now)
                {
                    throw new Exception("Signed date cannot be in the future!");
                }
                string AddedBy = _userManager.GetUserId(User).ToString();
                decimal claimTotal = _policyClaimRepository.UpdateAllocationTotal(RequestID);
                _policyClaimRepository.UpdateDisbursementAmount(RequestID, claimTotal);
                _policyClaimRepository.UpdateSignedDate(RequestID,SignedOn);
                _policyClaimRepository.UpdateSubmittedBankBranchID(RequestID, SubmittedBankBranchID);
                _policyClaimRepository.ArchiveExpense(RequestID, AddedBy);
				_policyClaimRepository.AddClaimSystemExpenses(RequestID, AddedBy);
				foreach (var exp in ClaimExpenses)
				{
					if (exp.ApplicationType == 2 && exp.IsSelected)
					{
						// this charge applies: exp.Price is either the original or user-edited
						if (exp.Price > 0)
						{
							_policyClaimRepository.AddClaimUserExpense(RequestID, exp.ID, exp.Price, AddedBy);
						}
						else
						{
							throw new Exception("Invalid expense entered!");
						}
					}
				}
				_policyClaimRepository.AddClaimCalculatedExpenses(RequestID, AddedBy);
				_policyClaimRepository.UpdateStatus(RequestID, 3050, AddedBy);
				if (SubmittedByType == "PolicyHolder")
				{
					int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
					int memberID = _memberRepository.GetID(ProposerID);
					PolicyClaimant policyClaimant = new()
					{
						ClaimID = claimID,
						MemberID = memberID,
						AddressID = 0,
						TelephoneID = 0,
						EmailAddressID = 0,
						RoleID = 5, //5 is submitted by
						BankAccountID = 0,
						CellPhoneID = 0,
						MainCover = 0, //reinforce check on code side
						PayAfter = 0,
						Amount = 0,
						PolicyBeneficiariesLineID = 0,
						AddedOn = DateTime.Now,
						AddedBy = AddedBy
					};
					_policyClaimRepository.AddClaimSubmitter(policyClaimant);
				}
				BusinessRulesParameters claimsSubmissionRulesParameters = new BusinessRulesParameters();
                claimsSubmissionRulesParameters.RequestID = RequestID; 
                List<StatusReport> statusReports = _businessRuleRepository.GetAllChecks(PolicyTypeID, "Claim Submission", claimsSubmissionRulesParameters);
                foreach (StatusReport statusReport in statusReports)
                {
                    ObjectRulesStatiiHistory objectRulesStatiiHistory = new()
                    {
                        RequestID = RequestID,
                        SourceID = 5, //allocated to claims, 1 is new business, 2 premiums, 3 commissions, 4 policy servicing
                        MemberID = 0,
                        StatusRuleID = statusReport.RuleID,
                        SuccessStatus=statusReport.SuccessStatus, 
                        Status = statusReport.StatusID,
                        StatusReason = statusReport.StatusReasonID,
                        StatusComment =statusReport.StatusMessage, 
                        StatusDate = DateTime.Now,
                        StatusAddedBy = AddedBy
                    };
                    _objectRulesStatiiHistoryRepository.Add(objectRulesStatiiHistory);
                } 
                return Redirect("MySubmissions");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostSubmittedBy()
        {
            try
            { 
                if (string.IsNullOrEmpty((IDContent)) || string.IsNullOrEmpty(ConfirmIDContent))
                {
                    throw new Exception("Please select a valid identity!");
                }
                if (IDContent != ConfirmIDContent)
                {
                    throw new Exception("ID confirmation values do not match!");
                }
                string AddedBy = _userManager.GetUserId(User).ToString();
                int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
                ContactCard.CountryID = CountryID;
                if (ContactCard.City == 0)
                {
                    if (string.IsNullOrEmpty(CustomCity))
                    {
                        throw new Exception("you must enter or select a city");
                    }
                    ContactCard.City = _countryRepository.AddCity(ContactCard.CountryID, CustomCity, AddedBy, DateTime.Now);
                }
                int roleID;
                Member member = _memberRepository.GetMemberById(IDContent);
                if (member == null)
                {
                    if ((NewMember.Name1 == null) || (NewMember.Name3 == null) || (RelationshipID == -1))
                    {
                        throw new Exception("Please fill in all required details!");
                    }
                    roleID = 1; //role id 1 means the claimant is not on the policy, new members are a trivial case
                    NewMember.NationalID = IDContent;
                    NewMember.IsOrganisation = 0;
                    NewMember.UID = Guid.NewGuid();
                    NewMember.AddedBy = AddedBy;
                    NewMember.AddedOn = DateTime.Now;
                    _memberRepository.AddMember(NewMember);
                    member = _memberRepository.GetMemberById(IDContent);
                } 
                // save address
                int addressID = 0;
                if (ContactCard.City == 0 || string.IsNullOrEmpty(ContactCard.AddressLine1) || ContactCard.CountryID == 0)
                {
                    if (RelationshipID != 0)
                    {
                        throw new Exception("Address is required for any other members who are not the policy holder.");
                    }
                }
                else
                {
                    MemberContact AddressContact = new()
                    {
                        AddedBy = AddedBy,
                        City = ContactCard.City,
                        MemberUID = member.UID,
                        Line1 = ContactCard.AddressLine1,
                        Line2 = ContactCard.AddressLine2,
                        Line3 = ContactCard.AddressLine3,
                        CountryID = ContactCard.CountryID,
                        ContactTypeID = 1
                    };
                    _memberContactRepository.InsertMemberContact(AddressContact);
                    addressID = AddressContact.ID;
                }
                int mobileNoID = 0;
                if (!string.IsNullOrEmpty(ContactCard.CellPhone))
                {
                    MemberContact MobileContact = new()
                    {
                        AddedBy = AddedBy,
                        MemberUID = member.UID,
                        Line1 = ContactCard.CellPhone,
                        ContactTypeID = 4
                    };
                    _memberContactRepository.InsertMemberContact(MobileContact);
                    mobileNoID = MobileContact.ID;
                }

                int telephoneID = 0;
                if (!string.IsNullOrEmpty(ContactCard.Telephone))
                {
                    MemberContact TelephoneContact = new()
                    {
                        AddedBy = AddedBy,
                        MemberUID = member.UID,
                        Line1 = ContactCard.Telephone,
                        ContactTypeID = 5
                    };
                    _memberContactRepository.InsertMemberContact(TelephoneContact);
                    telephoneID = TelephoneContact.ID;
                }

                int emailAddressID = 0;
                if (!string.IsNullOrEmpty(ContactCard.EmailAddress))
                {
                    MemberContact EmailContact = new()
                    {
                        AddedBy = AddedBy,
                        MemberUID = member.UID,
                        Line1 = ContactCard.EmailAddress,
                        ContactTypeID = 6
                    };
                    _memberContactRepository.InsertMemberContact(EmailContact);
                    emailAddressID = EmailContact.ID;
                } 
                PolicyClaimant policyClaimant = new()
                {
                    ClaimID = claimID,
                    MemberID = member.ID,
                    AddressID = addressID,
                    TelephoneID = telephoneID,
                    EmailAddressID = emailAddressID,
                    RoleID = 5, //5 is submitted by
                    BankAccountID = 0, 
                    CellPhoneID = mobileNoID,
                    MainCover = 0, //reinforce check on code side
                    PayAfter = 0,
                    Amount = 0,
                    PolicyBeneficiariesLineID = 0,
                    AddedOn = DateTime.Now,
                    AddedBy = AddedBy
                };
                _policyClaimRepository.AddClaimant(policyClaimant);
                return Redirect("SubmitClaim?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostRemoveSubmittedByEntry(int ID)
        {
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                _policyClaimRepository.ArchiveSubmittedByEntry(ID, addedBy, DateTime.Now);
                return Redirect(ReturnUrl);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
} 
