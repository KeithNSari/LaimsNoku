using LAIMS.Interfaces.Banking;
using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Banking;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Claims;
using LAIMS.Models.Investments;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.BusinessRules;
using LAIMS.Repositories.Lifeproducts;
using LAIMS.Repositories.Membership;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;

namespace LAIMS.Areas.Claims.Pages
{
    [Authorize(Roles = "Claims Initiator")]
    public class ClaimantsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMediaUploadRepository _mediaUploadRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDocumentsRepository _documentsRepository;
        private readonly IPolicyClaimRepository _policyClaimRepository;
        private readonly IPolicyBeneficiaryLineRepository _policyBeneficiaryLineRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IMemberContactRepository _memberContactRepository;
        private readonly IGenderRepository _genderRepository;    
        private readonly ITitleRepository _titleRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IMaritalStatusRepository _maritalStatusRepository;
        private readonly IRelationshipRepository _relationshipRepository;               
        private readonly IBankRepository _bankRepository;
        private readonly IMemberBankAccountRepository _memberBankAccountRepository;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        private readonly IObjectRulesStatiiHistoryRepository _objectRulesStatiiHistoryRepository;
        public List<SelectListItem> BanksSelectList = new List<SelectListItem>();
        public List<SelectListItem> MyRelationships = new List<SelectListItem>();
        public List<SelectListItem> GenderList = new List<SelectListItem>();
        public List<SelectListItem> TitleList = new List<SelectListItem>();
        public List<SelectListItem> MaritalStatiiSelectList = new List<SelectListItem>();
        public List<SelectListItem> CountrySelectList = new List<SelectListItem>();
        public List<Gender> Genders { get; set; }
        [BindProperty]
        public Policy Policy { get; set; }
        [BindProperty ]
        public int MainCover { get; set; }
        [BindProperty]
        public int PayAfter { get; set; } = 0;
        [BindProperty]
        public decimal Amount { get; set; } = 0;

        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid ProposerID { get; set; }
        [BindProperty]
        public int PolicyBeneficiariesLineID { get; set; }
        [BindProperty]
        public Guid RequestID { get; set; } 
        [BindProperty]
        public string? IDContent { get; set; }
        [BindProperty]
        public string? ConfirmIDContent { get; set; }
        [BindProperty]
        public Member NewMember { get; set; }
        [BindProperty]
        public string ServiceProviderName { get; set; }
        [BindProperty]
        public int SelectedMemberId { get; set; } = 0;
        [BindProperty]
        public MemberBankAccount MemberBankAccount { get; set; }   
        public List<Title> Titles { get; set; }
        public List<Country> Countries { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CountryID { get; set; }
        [BindProperty]
        public string CustomCity { get; set; }
        public List<MaritalStatus> MaritalStatii { get; set; }
        [BindProperty] 
        public ContactCard ContactCard { get; set; }
        public DataTable MembersDT;
        public DataTable BasicCoverClaimantsDT;
        public DataTable SupplementaryCoverClaimantsDT;
        public DataTable CoverDT;
        [BindProperty]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public int RelationshipID { get; set; } = -1;      
        [BindProperty]
        public List<PolicyBeneficiaryLineDetail> PolicyBeneficiaryLineDetails { get; set; } 
        public ClaimantsModel(UserManager<ApplicationUser> userManager, IMediaUploadRepository mediaUploadRepository,
           IWebHostEnvironment webHostEnvironment, IPolicyClaimRepository policyClaimRepository,
             IGenderRepository genderRepository, IMemberRepository memberRepository,
            ITitleRepository titleRepository, IRelationshipRepository relationshipRepository,
            ICountryRepository countryRepository, ICityRepository cityRepository,
            IMemberContactRepository memberContactRepository,IMaritalStatusRepository maritalStatusRepository, 
            IPolicyBeneficiaryLineRepository policyBeneficiaryLineRepository, IBankRepository bankRepository,
            IMemberBankAccountRepository memberBankAccountRepository,IBusinessRuleRepository businessRuleRepository,
            IObjectRulesStatiiHistoryRepository objectRulesStatiiHistoryRepository)
        {
            _userManager = userManager;
            _mediaUploadRepository = mediaUploadRepository;
            _webHostEnvironment = webHostEnvironment; 
            _policyClaimRepository = policyClaimRepository;
            _memberRepository = memberRepository;
            _memberContactRepository = memberContactRepository;
            _genderRepository = genderRepository;
            _titleRepository = titleRepository;
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
            _maritalStatusRepository = maritalStatusRepository;
            _policyBeneficiaryLineRepository = policyBeneficiaryLineRepository;
            _relationshipRepository = relationshipRepository;
            _bankRepository = bankRepository;
            _memberBankAccountRepository = memberBankAccountRepository;
            _businessRuleRepository = businessRuleRepository;
            _objectRulesStatiiHistoryRepository = objectRulesStatiiHistoryRepository;
        }
        public void OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            ProposerID = id;
            PolicyTypeID = policyTypeid;
            PolicyID = policyid;
            RequestID = reqid;
            PolicyBeneficiaryLineDetails = _policyBeneficiaryLineRepository.GetPolicyBeneficiaryLineDetailsByPolicyID(policyid);
            LoadLists();
        } 
        private void LoadLists()
        {
            Genders = _genderRepository.GetAllGenders();
            Titles = _titleRepository.GetAllTitles();
            Countries = _countryRepository.GetAllCountries();
            MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
            LoadRelationshipsSelectList();
            LoadGenderSelectList();
            LoadTitleSelectList();
            LoadMaritalSelectList();
            LoadCountrySelectList();
            LoadBanksSelectList();
            LoadCoverDetails();
        }
        private void LoadBanksSelectList()
        {
            List<Bank> banksList = _bankRepository.GetAllBanks();
            foreach (Bank bank in banksList)
            {
                BanksSelectList.Add(new SelectListItem
                {
                    Value = bank.BankID.ToString(),
                    Text = bank.BankName
                });
            }
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
        private void LoadGenderSelectList()
        {
            foreach (Gender gender in Genders)
            {
                GenderList.Add(new SelectListItem
                {
                    Value = gender.Id.ToString(),
                    Text = gender.GenderName
                });
            }
        }
        private void LoadTitleSelectList()
        {
            foreach (Title title in Titles)
            {
                TitleList.Add(new SelectListItem
                {
                    Value = title.TitleID.ToString(),
                    Text = title.TitleName
                });
            }
        }
        private void LoadMaritalSelectList()
        {
            foreach (MaritalStatus maritalStatus in MaritalStatii)
            {
                MaritalStatiiSelectList.Add(new SelectListItem
                {
                    Value = maritalStatus.MaritalStatusID.ToString(),
                    Text = maritalStatus.MaritalStatusName
                });
            }
        }
        private void LoadCountrySelectList()
        {
            foreach (Country country in Countries)
            {
                CountrySelectList.Add(new SelectListItem
                {
                    Value = country.CountryID.ToString(),
                    Text = country.CountryName
                });
            }
        }
        private void LoadCoverDetails()
        {
            int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
            CoverDT = _policyClaimRepository.GetClaimCover(claimID);
            BasicCoverClaimantsDT = _policyClaimRepository.GetPolicyClaimants(claimID,1);
            SupplementaryCoverClaimantsDT = _policyClaimRepository.GetPolicyClaimants(claimID,0); 
        }  
        public IActionResult OnPostAddClaimant()
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
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
                decimal balance = _policyClaimRepository.CoverBalance(claimID, PolicyBeneficiariesLineID);
                if (Amount > balance) throw new Exception("Amount exceeds the available cover balance! Please adjust.");
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
                else
                {
                    if (_policyClaimRepository.ContractualPartyCheck(member.ID, PolicyID) > 0)
                    {
                        roleID = 0;//role id=0 is for claimants who are also on the policy
                        //BusinessRulesParameters claimsSubmissionRulesParameters = new BusinessRulesParameters();
                        //claimsSubmissionRulesParameters.RequestID = RequestID;
                        //List<StatusReport> statusReports = _businessRuleRepository.GetAllChecks(PolicyTypeID, "Claim Submission", claimsSubmissionRulesParameters);
                        //foreach (StatusReport statusReport in statusReports)
                        //{
                        //    ObjectRulesStatiiHistory objectRulesStatiiHistory = new()
                        //    {
                        //        RequestID = RequestID,
                        //        SourceID = 5, //allocated to claims, 1 is new business, 2 premiums, 3 commissions, 4 policy servicing
                        //        MemberID = 0,
                        //        StatusRuleID = statusReport.RuleID,
                        //        Status = statusReport.StatusID,
                        //        StatusReason = statusReport.StatusReasonID,
                        //        StatusComment = statusReport.StatusMessage,
                        //        StatusDate = DateTime.Now,
                        //        StatusAddedBy = AddedBy
                        //    };
                        //    _objectRulesStatiiHistoryRepository.Add(objectRulesStatiiHistory);
                        //}
                        //var failedRules = statusReports.Where(sr => sr.SuccessStatus == 0).ToList();
                        //if (failedRules.Any())
                        //{
                        //    StringBuilder error = new();
                        //    error.Append("The claim failed the following: ");
                        //    foreach (var failedRule in failedRules)
                        //    {
                        //        error.Append(failedRule.StatusMessage + ". ");
                        //    }
                        //    error.Remove(error.Length - 1, 1);
                        //    throw new Exception(error.ToString());
                        //}
                    }
                    else
                    {
                        roleID = 1;//role id 1 means the claimant is not on the policy
                    }
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

                MemberBankAccount.MemberID = member.ID;
                MemberBankAccount.AddedOn = DateTime.Now;
                MemberBankAccount.AddedBy = AddedBy;
                BankAccountFormat bankAccountFormat = _bankRepository.GetBankAccountFormat(MemberBankAccount.BankID);
                Regex accountNoFormat = new Regex(bankAccountFormat.BankAccountNoFormat);
                if (!accountNoFormat.IsMatch(MemberBankAccount.BranchCode + MemberBankAccount.BankAccountNo))
                {
                    throw new Exception("Invalid account number format! The expected format for this provider is: " + bankAccountFormat.FormatDescription);
                }
                int accountID = _memberBankAccountRepository.AddMemberBankAccount(MemberBankAccount);
                if (accountID == 0)
                {
                    throw new Exception("Bank account could not be created!");
                }
                PolicyClaimant policyClaimant = new()
                {
                    ClaimID = claimID,
                    MemberID = member.ID,
                    AddressID = addressID,
                    TelephoneID = telephoneID,
                    EmailAddressID = emailAddressID,
                    RoleID = roleID,
                    BankAccountID=accountID,
                    CellPhoneID = mobileNoID,
                    MainCover=MainCover, //reinforce check on code side
                    PayAfter =PayAfter,
                    Amount =Amount,
                    PolicyBeneficiariesLineID=PolicyBeneficiariesLineID,
                    AddedOn = DateTime.Now,
                    AddedBy = AddedBy
                };
                _policyClaimRepository.AddClaimant(policyClaimant);
                return Redirect("Claimants?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
		public IActionResult OnPostAddPrincipalClaimant()
		{
			try
			{
				ReturnUrl = Request.Path + Request.QueryString; 
				string AddedBy = _userManager.GetUserId(User).ToString();
				int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
				decimal balance = _policyClaimRepository.CoverBalance(claimID, PolicyBeneficiariesLineID);
				if (Amount > balance) throw new Exception("Amount exceeds the available cover balance! Please adjust."); 
				int roleID=0;
				Member member = _memberRepository.GetMemberById(ProposerID);   
				MemberBankAccount.MemberID = member.ID;
				MemberBankAccount.AddedOn = DateTime.Now;
				MemberBankAccount.AddedBy = AddedBy;
				BankAccountFormat bankAccountFormat = _bankRepository.GetBankAccountFormat(MemberBankAccount.BankID);
				Regex accountNoFormat = new Regex(bankAccountFormat.BankAccountNoFormat);
				if (!accountNoFormat.IsMatch(MemberBankAccount.BranchCode + MemberBankAccount.BankAccountNo))
				{
					throw new Exception("Invalid account number format! The expected format for this provider is: " + bankAccountFormat.FormatDescription);
				}
				int accountID = _memberBankAccountRepository.AddMemberBankAccount(MemberBankAccount);
				if (accountID == 0)
				{
					throw new Exception("Bank account could not be created!");
				}
				PolicyClaimant policyClaimant = new()
				{
					ClaimID = claimID,
					MemberID = member.ID,
					AddressID = 0,
					TelephoneID = 0,
					EmailAddressID = 0,
					RoleID = roleID,
					BankAccountID = accountID,
					CellPhoneID = 0,
					MainCover = MainCover, //reinforce check on code side
					PayAfter = PayAfter,
					Amount = Amount,
					PolicyBeneficiariesLineID = PolicyBeneficiariesLineID,
					AddedOn = DateTime.Now,
					AddedBy = AddedBy
				};
				_policyClaimRepository.AddClaimant(policyClaimant);
				return Redirect("Claimants?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
			}
			catch (Exception ex)
			{
				return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
			}
		}
		public JsonResult OnGetCitiesByCountry()
        {
            var cities = _cityRepository.GetCitiesByCountry(CountryID);
            return new JsonResult(cities);
        }
        public IActionResult OnGetSearchMembers(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return new JsonResult(new List<Member>());
            }
            List<Member> OrgList = _memberRepository.SearchOrganisations(searchTerm);              
            return new JsonResult(OrgList);
        }
        public IActionResult OnPostAddServiceProvider()
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                string AddedBy = _userManager.GetUserId(User).ToString();
                int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
                decimal balance = _policyClaimRepository.CoverBalance(claimID, PolicyBeneficiariesLineID);
                if (Amount > balance) throw new Exception("Amount exceeds the available cover balance! Please adjust.");
                Guid ServiceProviderID = Guid.NewGuid();
                if (SelectedMemberId == 0)
                {
                    Member NewServiceProvider = new()
                    {
                        Name1 = ServiceProviderName,
                        UID = ServiceProviderID,
                        IsOrganisation = 1
                    };
                    _memberRepository.AddMember(NewServiceProvider);
                    SelectedMemberId = NewServiceProvider.ID;
                }
                else
                {
                    ServiceProviderID = _memberRepository.GetUID(SelectedMemberId);
                }
                int mobileNoID = 0;
                if (!string.IsNullOrEmpty(ContactCard.CellPhone))
                {
                    MemberContact MobileContact = new()
                    {
                        AddedBy = AddedBy,
                        MemberUID = ServiceProviderID,
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
                        MemberUID = ServiceProviderID,
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
                        MemberUID = ServiceProviderID,
                        Line1 = ContactCard.EmailAddress,
                        ContactTypeID = 6
                    };
                    _memberContactRepository.InsertMemberContact(EmailContact);
                    emailAddressID = EmailContact.ID;
                }



                MemberBankAccount.MemberID = SelectedMemberId;
                MemberBankAccount.AddedOn = DateTime.Now;
                MemberBankAccount.AddedBy = AddedBy;
                BankAccountFormat bankAccountFormat = _bankRepository.GetBankAccountFormat(MemberBankAccount.BankID);
                Regex accountNoFormat = new Regex(bankAccountFormat.BankAccountNoFormat);
                if (!accountNoFormat.IsMatch(MemberBankAccount.BranchCode + MemberBankAccount.BankAccountNo))
                {
                    throw new Exception("Invalid account number format! The expected format for this provider is: " + bankAccountFormat.FormatDescription);
                }
                int accountID = _memberBankAccountRepository.AddMemberBankAccount(MemberBankAccount);
                if (accountID == 0)
                {
                    throw new Exception("Bank account could not be created!");
                }
                PolicyClaimant policyClaimant = new()
                {
                    ClaimID = claimID,
                    MemberID = SelectedMemberId,
                    AddressID = 0,
                    TelephoneID = telephoneID,
                    EmailAddressID = emailAddressID,
                    CellPhoneID = mobileNoID,
                    MainCover = MainCover,
                    PayAfter = PayAfter,
                    Amount = Amount,
                    PolicyBeneficiariesLineID = PolicyBeneficiariesLineID,
                    RoleID = 2, //role id =2  is for service providers
                    BankAccountID = accountID,
                    AddedOn = DateTime.Now,
                    AddedBy = AddedBy
                };
                _policyClaimRepository.AddClaimant(policyClaimant);
                return Redirect("Claimants?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
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
        public IActionResult OnPostAddShares(int[] PBIDS, decimal[] Shares)
        {
            try
            { 
                using (TransactionScope TS = new TransactionScope())
                {
                    string addedBy = _userManager.GetUserId(User).ToString();
                    int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
                    decimal totalValue = 0m;
                    for (int i = 0; i < PBIDS.Length; i++)
                    {
                        int recordID = PBIDS[i];
                        decimal shareValue = Shares[i];
                        totalValue += shareValue;
                        if (shareValue > 0)
                        {
                            _policyClaimRepository.UpdateClaimantAmount(recordID, shareValue);
                        }
                    }
                    //if (totalValue != 100) throw new Exception("Split must add up to a 100%!"); 
                    TS.Complete();
                }
                return Redirect("Claimants?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostAddSupplementaryShares(int[] SPBIDS, decimal[] SShares)
        {
            try
            {
                using (TransactionScope TS = new TransactionScope())
                {
                    string addedBy = _userManager.GetUserId(User).ToString();
                    int claimID = _policyClaimRepository.GetClaimIDByRequestID(RequestID);
                    decimal totalValue = 0m;
                    for (int i = 0; i < SPBIDS.Length; i++)
                    {
                        int recordID = SPBIDS[i];
                        decimal shareValue = SShares[i];
                        totalValue += shareValue;
                        if (shareValue > 0)
                        {
                            _policyClaimRepository.UpdateClaimantAmount(recordID, shareValue);
                        }
                    }
                    //if (totalValue != 100) throw new Exception("Split must add up to a 100%!"); 
                    TS.Complete();
                }
                return Redirect("Claimants?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostNext()
        {
            try
            {
                return Redirect("ClaimServicesFullList?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostRemove(int id)
        {
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                _policyClaimRepository.ArchiveClaimLine(id, addedBy);
                return Redirect("Claimants?id=" + ProposerID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
}
