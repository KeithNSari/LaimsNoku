using LAIMS.Interfaces.Banking;
using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Commissions;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Banking;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Commissions;
using LAIMS.Models.Membership;
using LAIMS.Models.Premiums;
using LAIMS.Repositories.Membership;
using LAIMS.Repositories.Policies;
using LAIMS.Repositories.Premiums;
using LAIMS.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Protocol.Plugins;
using System.Data;
using System.Text.RegularExpressions;
using System.Transactions;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class PPDetailsModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IPolicyPremiumRepository _policyPremiumRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IGenderRepository _genderRepository;
        private readonly ITitleRepository _titleRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMaritalStatusRepository _maritalStatusRepository;
        private readonly IPaymentProviderRepository _paymentProviderRepository;
        private readonly IMemberBankAccountRepository _memberBankAccountRepository;        
        private readonly IBusinessRuleRepository _businessRuleRepository;
        private readonly IBankRepository _bankRepository;
		private readonly IEmploymentRepository _employmentRepository;
		public List<SelectListItem> GenderList = new List<SelectListItem>();
        public List<SelectListItem> TitleList = new List<SelectListItem>();
        public List<SelectListItem> MaritalStatiiSelectList = new List<SelectListItem>();
        public List<SelectListItem> CountrySelectList = new List<SelectListItem>();
        public List<SelectListItem> StopOrderProviderList = new List<SelectListItem>();
        public List<SelectListItem> DebitOrderProviderList = new List<SelectListItem>();
        public List<SelectListItem> CategoriesList = new List<SelectListItem>();

        public List<Gender> Genders { get; set; }
        public List<Title> Titles { get; set; }
        public List<Country> Countries { get; set; }
        public List<MaritalStatus> MaritalStatii { get; set; }
        public List <PaymentProvider> StopOrderProviders { get; set; }
        public List<PaymentProvider> DebitOrderProviders { get; set; }
        public DataTable PremiumDetails { get; set; }
		[BindProperty]
		public Employment Employment { get; set; }
		public PPDetailsModel(UserManager<IdentityUser> userManager,
            IMemberRepository memberRepository,
			IEmploymentRepository employmentRepository,
			IPolicyTypeRepository policyTypeRepository,
            IPolicyRepository policyRepository,
            IPolicyPremiumRepository policyPremiumRepository,
            IGenderRepository genderRepository,
            ITitleRepository titleRepository,
            ICountryRepository countryRepository,
            IMaritalStatusRepository maritalStatusRepository,
            IPaymentProviderRepository paymentProviderRepository,
            IMemberBankAccountRepository memberBankAccountRepository,      
            IBusinessRuleRepository businessRuleRepository,
            IBankRepository bankRepository
        )
        {
            _userManager = userManager;           
            _policyTypeRepository = policyTypeRepository;
            _policyRepository = policyRepository; 
            _policyPremiumRepository = policyPremiumRepository;
            _memberRepository = memberRepository;
			_employmentRepository = employmentRepository;
			_genderRepository = genderRepository;
            _titleRepository = titleRepository;
            _countryRepository = countryRepository;
            _maritalStatusRepository = maritalStatusRepository;
            _paymentProviderRepository = paymentProviderRepository;
            _memberBankAccountRepository = memberBankAccountRepository;           
            _businessRuleRepository = businessRuleRepository;
            _bankRepository = bankRepository;
        }
        [BindProperty]
        public int PremiumPayerID { get; set; }
        [BindProperty]
        public Member PremiumPayer { get; set; }
        [BindProperty]
        public int PolicyPremiumID { get; set; }
        [BindProperty]
        public Guid ProposerID { get; set; }
        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public string? PolicyName { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public int IDType { get; set; }
        [BindProperty]
        public string? IDContent { get; set; }
        [BindProperty]
        public string? ConfirmIDContent { get; set; }
        [BindProperty]
        public Member NewMember { get; set; }
        [BindProperty]
        public PolicyPremium PolicyPremium {  get; set; }
        [BindProperty ]  
        public string BranchCode { get; set; }
        [BindProperty]
        public string PremiumPayerAccount { get; set; }
        [BindProperty]
        public string PremiumPayerAccountConfirm { get; set; }
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
        private void LoadStopOrderProvidersSelectList()
        {
            foreach (PaymentProvider paymentProvider in StopOrderProviders)
            {
                StopOrderProviderList.Add(new SelectListItem
                {
                    Value = paymentProvider.ID.ToString(),
                    Text = paymentProvider.ProviderName
                });
            }
        }
        private void LoadDebitOrderProvidersSelectList()
        {
            foreach (PaymentProvider paymentProvider in DebitOrderProviders)
            {
                DebitOrderProviderList.Add(new SelectListItem
                {
                    Value = paymentProvider.ID.ToString(),
                    Text = paymentProvider.ProviderName
                });
            }
        }
        private void LoadCategoriesSelectList()
        {
            foreach (EmploymentCategory employmentCategory in _employmentRepository.GetEmploymentCategories())
            {
                CategoriesList.Add(new SelectListItem
                {
                    Value = employmentCategory.ID.ToString(),
                    Text = employmentCategory.Category
                });
            }
        }
        public void OnGet(Guid id, Guid policyTypeid, Guid policyid)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            PolicyID = policyid;  
            PolicyTypeID = policyTypeid;
            ProposerID = id;
            _policyRepository.UpdatePolicyStage(PolicyID, 6);
            Genders = _genderRepository.GetAllGenders();
            Titles = _titleRepository.GetAllTitles();
            Countries = _countryRepository.GetAllCountries();
            MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
            StopOrderProviders = _paymentProviderRepository.GetPaymentProviders(2);
            DebitOrderProviders = _paymentProviderRepository.GetDebitOrderProviderList();
            PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
            PremiumPayerID = _policyPremiumRepository.  GetPremiumPayer(policyid);
            if (PremiumPayerID > 0)
            {
                PremiumPayer = _memberRepository.GetMemberById(PremiumPayerID);
                PolicyPremium = _policyPremiumRepository.GetMainPolicyPremium(policyid);
                PolicyPremiumID = PolicyPremium.ID;
                PremiumDetails = _policyPremiumRepository.GetPremiumDetails(policyid, PolicyPremiumID); 
            }
            LoadGenderSelectList();
            LoadTitleSelectList();
            LoadMaritalSelectList();
            LoadCountrySelectList();
            LoadStopOrderProvidersSelectList();
            LoadCategoriesSelectList();
            LoadDebitOrderProvidersSelectList();
        }
        public IActionResult OnPostAddPremiumPayer(Guid id, Guid policyTypeid, Guid policyid)
        {
            using (TransactionScope TS = new TransactionScope())
            {
                try
                {
                    PolicyID = policyid;
                    PolicyTypeID = policyTypeid;
                    ProposerID = id;
                    Genders = _genderRepository.GetAllGenders();
                    Titles = _titleRepository.GetAllTitles();
                    Countries = _countryRepository.GetAllCountries();
                    MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
                    StopOrderProviders = _paymentProviderRepository.GetPaymentProviders(2);
                    DebitOrderProviders = _paymentProviderRepository.GetDebitOrderProviderList();
                    PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
                    LoadGenderSelectList();
                    LoadTitleSelectList();
                    LoadMaritalSelectList();
                    LoadCountrySelectList();
                    LoadStopOrderProvidersSelectList();
                    LoadCategoriesSelectList();
                    LoadDebitOrderProvidersSelectList();
                    if (string.IsNullOrEmpty((IDContent)) || string.IsNullOrEmpty(ConfirmIDContent) || (IDType == 0))
                    {
                        throw new Exception("Please select a valid identity option and add the corresponding values!");
                    }
                    if (IDContent != ConfirmIDContent)
                    {
                        throw new Exception("ID confirmation values do not match!");
                    }
                    string AddedBy = _userManager.GetUserId(User).ToString();
                    Member member = _memberRepository.GetMemberById(IDContent);
                    if (member == null)
                    {
                        if ((NewMember.Name1 == null) || (NewMember.Name3 == null) || (NewMember.GenderID == null) || (NewMember.TitleID == null) || (NewMember.MaritalStatusID == null) || (NewMember.DOB == null) || (NewMember.BirthCountryID == null) || (NewMember.CountryID == null))
                        {
                            throw new Exception("Please fill in all required details!");
                        }
                        if (NewMember.DOB > DateTime.Today)
                        {
                            throw new Exception("Date of birth cannot be in the future!");
                        }
                        switch (IDType)
                        {
                            case 1:
                                NewMember.NationalID = IDContent;
                                if (NewMember.NationalID != string.Empty)
                                {
                                    string pattern1 = @"(^\d{2})-(\d{4,7})\s([A-Za-z]{1}\s(\d{2}$))";
                                    string pattern2 = @"(^\d{2})-(\d{4,7})([A-Za-z]{1}(\d{2}$))";
                                    Regex regex1 = new Regex(pattern1);
                                    Regex regex2 = new Regex(pattern2);
                                    if (!(regex1.IsMatch(NewMember.NationalID) || regex2.IsMatch(NewMember.NationalID)))
                                    {
                                        throw new Exception("Invalid National ID format");
                                    }
                                }
                                break;
                            case 2:
                                NewMember.BirthCertificate = IDContent;
                                break;
                            case 3:
                                NewMember.Passport = IDContent;
                                break;
                            default:
                                throw new Exception("At least one form of Identity is required!");
                        }
                        NewMember.IsOrganisation = 0;
                        NewMember.UID = Guid.NewGuid();
                        NewMember.AddedBy = AddedBy;
                        NewMember.AddedOn = DateTime.Now;
                        _memberRepository.AddMember(NewMember);
                        member = _memberRepository.GetMemberById(IDContent);
                    }
                    if (member.ID > 0)
                    {
                        PolicyPremium policyPremium = new PolicyPremium();
                        policyPremium.HeaderID = policyid;
                        policyPremium.PremiumPayer = member.ID;
                        policyPremium.AddedBy = AddedBy;
                        policyPremium.AddedOn = DateTime.Now;
                        _policyPremiumRepository.AddPolicyPremium(policyPremium);
                        PolicyPremium = _policyPremiumRepository.GetMainPolicyPremium(policyid);
                    }
                    BusinessRulesParameters newBusinessParameters = new BusinessRulesParameters();
                    newBusinessParameters.PolicyTypeID = PolicyTypeID;
                    newBusinessParameters.PolicyID = PolicyID;
                    newBusinessParameters.PremiumPayerID = PolicyPremium.PremiumPayer;
                    StatusReport statusReport = _businessRuleRepository.CheckRules(PolicyTypeID, "Premium Payer", newBusinessParameters);
                    if (statusReport.SuccessStatus == 0)
                    {
                        throw new Exception(statusReport.StatusMessage);
                    }
                    TS.Complete();
                    TempData["PostbackSuccess"] = true;
                    return Redirect("PPDetails?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
                }
                catch (Exception ex)
                { 
                    return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
                }
            }           
        }
        public IActionResult OnPostAddStopOrder(Guid id, Guid policyTypeid, Guid policyid)
        {
            using (TransactionScope TS = new TransactionScope())
            {
                try
                {
                    PolicyID = policyid;
                    PolicyTypeID = policyTypeid;
                    ProposerID = id;
                    Genders = _genderRepository.GetAllGenders();
                    Titles = _titleRepository.GetAllTitles();
                    Countries = _countryRepository.GetAllCountries();
                    MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
                    StopOrderProviders = _paymentProviderRepository.GetPaymentProviders(2);
                    DebitOrderProviders = _paymentProviderRepository.GetDebitOrderProviderList();
                    PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
                    PremiumDetails = _policyPremiumRepository.GetPremiumDetails(policyid, PolicyPremiumID);
                    LoadGenderSelectList();
                    LoadTitleSelectList();
                    LoadMaritalSelectList();
                    LoadCountrySelectList();
                    LoadStopOrderProvidersSelectList();
                    LoadCategoriesSelectList();
                    LoadDebitOrderProvidersSelectList(); 
                    string AddedBy = _userManager.GetUserId(User).ToString();
                    PolicyPremium.ID = PolicyPremiumID;
                    PolicyPremium.PaymentMethodID = 2;//stop order
                    CheckPaymentMethodRules(PolicyTypeID, PolicyID, id, (int)PolicyPremium.PaymentMethodID);
                    _policyPremiumRepository.UpdatePaymentMethod(PolicyPremium, policyid);
                    Employment.AddedBy = AddedBy;
                    Employment.ID = Guid.NewGuid();
                    _employmentRepository.InsertByPaymentProvider(Employment, (int)PolicyPremium.PaymentProviderID, PolicyID);
                    TS.Complete();
                    TempData["PostbackSuccess"] = true;
                    return Redirect("PPDetails?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
                }
                catch (Exception ex)
                {
                    return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
                }
            }
        }
        public IActionResult OnPostAddDebitOrder(Guid id, Guid policyTypeid, Guid policyid)
        {
            using (TransactionScope TS = new TransactionScope())
            {
                try
                {
                    if ((PremiumPayerAccount == null) || (PremiumPayerAccountConfirm == null))
                    {
                        throw new Exception("Please enter all required details! Account/ Confirm Account cannot be empty for debit orders");
                    }
                    if (PremiumPayerAccount != PremiumPayerAccountConfirm)
                    {
                        throw new Exception("Account and Confirm must match exactly!");
                    }
                    string AddedBy = _userManager.GetUserId(User).ToString();
                    int paymentProviderID= (int)PolicyPremium.PaymentProviderID;
                    int bankID = _bankRepository.GetBankID(paymentProviderID);
                    Member premiumPayer = _memberRepository.GetMemberById(id);
                    MemberBankAccount memberBankAccount = new MemberBankAccount();
                    memberBankAccount.BankAccountNo = PremiumPayerAccount;
                    memberBankAccount.BranchCode = BranchCode;
                    memberBankAccount.BankID = bankID;
                    memberBankAccount.MemberID = premiumPayer.ID;
                    memberBankAccount.AddedOn = DateTime.Now;
                    memberBankAccount.AddedBy = AddedBy;
                        BankAccountFormat bankAccountFormat = _bankRepository.GetBankAccountFormatByPaymentProvider(paymentProviderID);
                    Regex accountNoFormat = new Regex(bankAccountFormat.BankAccountNoFormat);
                    if (!accountNoFormat.IsMatch(memberBankAccount.BranchCode + memberBankAccount.BankAccountNo))
                    {
                        throw new Exception("Invalid account number format! The expected format for this provider is: " + bankAccountFormat.FormatDescription);
                    }
                    int premiumPayerAccountID = _memberBankAccountRepository.AddMemberBankAccount(memberBankAccount);
                    if(premiumPayerAccountID  == 0)
                    {
                        throw new Exception("Bank account could not be created!");
                    }
                    PolicyID = policyid;
                    PolicyTypeID = policyTypeid;
                    ProposerID = id;
                    Genders = _genderRepository.GetAllGenders();
                    Titles = _titleRepository.GetAllTitles();
                    Countries = _countryRepository.GetAllCountries();
                    MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
                    StopOrderProviders = _paymentProviderRepository.GetPaymentProviders(2);
                    DebitOrderProviders = _paymentProviderRepository.GetDebitOrderProviderList();
                    PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
                    PremiumDetails = _policyPremiumRepository.GetPremiumDetails(policyid, PolicyPremiumID);
                    LoadGenderSelectList();
                    LoadTitleSelectList();
                    LoadMaritalSelectList();
                    LoadCountrySelectList();
                    LoadStopOrderProvidersSelectList();
                    LoadCategoriesSelectList();
                    LoadDebitOrderProvidersSelectList();                    
                    PolicyPremium.ID = PolicyPremiumID;
                    PolicyPremium.PaymentMethodID = 1;//debit order
                    PolicyPremium.PremiumPayerAccountID = premiumPayerAccountID;
                    CheckPaymentMethodRules(PolicyTypeID, PolicyID, id, (int)PolicyPremium.PaymentMethodID);
                    _policyPremiumRepository.UpdatePaymentMethod(PolicyPremium, policyid);
                    TS.Complete();
                    TempData["PostbackSuccess"] = true;
                    return Redirect("PPDetails?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
                }
                catch (Exception ex)
                {
                    return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
                }
            }
        }
        public IActionResult OnPostAddDirectPayment(Guid id, Guid policyTypeid, Guid policyid)
        { 
           try
                {
                    PolicyID = policyid;
                    PolicyTypeID = policyTypeid;
                    ProposerID = id;
                    Genders = _genderRepository.GetAllGenders();
                    Titles = _titleRepository.GetAllTitles();
                    Countries = _countryRepository.GetAllCountries();
                    MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
                    StopOrderProviders = _paymentProviderRepository.GetPaymentProviders(2);
                    DebitOrderProviders = _paymentProviderRepository.GetDebitOrderProviderList();
                    PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
                    PremiumDetails = _policyPremiumRepository.GetPremiumDetails(policyid, PolicyPremiumID);
                    LoadGenderSelectList();
                    LoadTitleSelectList();
                    LoadMaritalSelectList();
                    LoadCountrySelectList();
                    LoadStopOrderProvidersSelectList();
                    LoadCategoriesSelectList();
                    LoadDebitOrderProvidersSelectList();
                    string AddedBy = _userManager.GetUserId(User).ToString();
                    PolicyPremium.ID = PolicyPremiumID;
                    PolicyPremium.PaymentMethodID = 3;//Direct Payment                   
                    CheckPaymentMethodRules(PolicyTypeID, PolicyID, id, (int)PolicyPremium.PaymentMethodID);
                   _policyPremiumRepository.UpdatePaymentMethod(PolicyPremium, policyid);
                   TempData["PostbackSuccess"] = true;
                return Redirect("PPDetails?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
                }
                catch (Exception ex)
                {
                    return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
                } 
        }
        public void CheckPaymentMethodRules(Guid PolicyTypeID, Guid PolicyID, Guid ProposerUID, int PaymentMethodID)
        {
            BusinessRulesParameters newBusinessParameters = new BusinessRulesParameters();
            newBusinessParameters.PolicyTypeID = PolicyTypeID;
            newBusinessParameters.PolicyID = PolicyID;
            newBusinessParameters.ProposerUID = ProposerUID;
            newBusinessParameters.PaymentMethodID = PaymentMethodID;
            StatusReport statusReport = _businessRuleRepository.CheckRules(PolicyTypeID, "Payment Method", newBusinessParameters);
            if (statusReport.SuccessStatus == 0)
            {
                throw new Exception(statusReport.StatusMessage);
            }
        }
            
        public IActionResult OnPostNext(Guid id, Guid policyTypeid, Guid policyid)
        { 
            //string addedBy = _userManager.GetUserId(User).ToString(); 
            _policyRepository.UpdatePolicyStatus(policyid, 5);
            return Redirect("Submission?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
        }
    }
}
