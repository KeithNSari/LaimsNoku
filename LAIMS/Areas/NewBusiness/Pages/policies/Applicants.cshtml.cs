using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Questionnaires;
using LAIMS.Repositories.Lifeproducts;
using LAIMS.Repositories.Premiums;
using LAIMS.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class ApplicantsModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        private readonly IGenderRepository _genderRepository;
        private readonly ITitleRepository _titleRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMaritalStatusRepository _maritalStatusRepository;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IPolicyTypesLinesRepository _policyTypesLinesRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly ILIRoleRepository _lIRoleRepository;
        private readonly IPolicyBeneficiaryRepository _policyBeneficiaryRepository;
        private readonly IPolicyBeneficiaryLineRepository _policyBeneficiaryLineRepository;
        private readonly IPolicyPremiumRepository _policyPremiumRepository;
        private readonly IPolicyPremiumLineRepository _policyPremiumLineRepository; 
        private readonly IQuestionnaireResponseRepository _questionnaireResponseRepository;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        public List<SelectListItem> MyProducts = new List<SelectListItem>();
        public List<SelectListItem> MyRelationships = new List<SelectListItem>();
        public List<SelectListItem> LIRoles = new List<SelectListItem>();
        public List<SelectListItem> GenderList = new List<SelectListItem>();
        public List<SelectListItem> TitleList = new List<SelectListItem>();
        public List<SelectListItem> MaritalStatiiSelectList = new List<SelectListItem>();
        public List<SelectListItem> CountrySelectList = new List<SelectListItem>();
        [BindProperty]
        public List<PolicyBeneficiaryLineDetail> PolicyBeneficiaryLineDetails { get; set; }
        public ApplicantsModel(UserManager<IdentityUser> userManager,
            IMemberRepository memberRepository,
            IGenderRepository genderRepository,
            ITitleRepository titleRepository,
            ICountryRepository countryRepository,
            IMaritalStatusRepository maritalStatusRepository,
            IPolicyTypeRepository policyTypeRepository,
            IPolicyTypesLinesRepository policyTypesLinesRepository,
            IPolicyRepository policyRepository,
            IRelationshipRepository relationshipRepository,
            ILIRoleRepository lIRoleRepository, 
            IPolicyBeneficiaryRepository policyBeneficiaryRepository, 
            IPolicyBeneficiaryLineRepository policyBeneficiaryLineRepository,
            IPolicyPremiumRepository policyPremiumRepository,
            IPolicyPremiumLineRepository policyPremiumLineRepository,
            IQuestionnaireResponseRepository questionnaireResponseRepository,
            IBusinessRuleRepository businessRuleRepository)
        {
            _userManager = userManager;
            _memberRepository = memberRepository;
            _genderRepository = genderRepository;
            _titleRepository = titleRepository;
            _countryRepository = countryRepository;
            _maritalStatusRepository = maritalStatusRepository;
            _policyTypeRepository = policyTypeRepository;
            _policyTypesLinesRepository = policyTypesLinesRepository;
            _policyRepository = policyRepository;
            _relationshipRepository = relationshipRepository;
            _lIRoleRepository = lIRoleRepository;
            _policyBeneficiaryRepository = policyBeneficiaryRepository;
            _policyBeneficiaryLineRepository = policyBeneficiaryLineRepository;
            _policyPremiumRepository = policyPremiumRepository;
            _policyPremiumLineRepository = policyPremiumLineRepository; 
            _questionnaireResponseRepository =questionnaireResponseRepository;
            _businessRuleRepository = businessRuleRepository;
        }
        [BindProperty]
        public Guid MemberID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; } 
        [BindProperty]
        public Member Member { get; set; }
        [BindProperty]
        public Member NewMember { get; set; }
        [BindProperty]
        public PolicyBeneficiaryLine PolicyBeneficiaryLine { get; set; }
        public List<Gender> Genders { get; set; }
        public List<Title> Titles { get; set; }
        public List<Country> Countries { get; set; }
        public List<MaritalStatus> MaritalStatii { get; set; }
        public DataTable MembersDT;
        public DataTable BeneficiariesDT;
        [BindProperty ]
        public string? PolicyName { get; set; }
        [BindProperty]
        public int IDType { get; set; }
        [BindProperty]
        public string? IDContent { get; set; }
        [BindProperty]
        public string? ConfirmIDContent { get; set; } 

        [BindProperty]
        public int RelationshipID { get; set; } = -1;
        [BindProperty]
        public int LIRoleID { get; set; } = 0;
        [BindProperty]
        public bool IsPremiumPayer { get; set; } = false;
        [BindProperty]
        public bool IsPrincipalMember { get; set; } = false;

        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public int PolicyTerm { get; set; }
        [BindProperty]
        public bool IsLife { get; set; }
        [BindProperty]
        public int MinimumTerm { get; set; }
        [BindProperty]
        public int MaximumTerm { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }        

        public IActionResult OnGet(Guid id, Guid policyTypeid, Guid policyid)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            PolicyID = policyid;
            MemberID = id;
            PolicyTypeID = policyTypeid;            
            Member = _memberRepository.GetMemberById(id);
            if (Member == null)
            {
                return NotFound();
            }
            _policyRepository.UpdatePolicyStage(PolicyID, 2);
            Genders = _genderRepository.GetAllGenders();
            Titles = _titleRepository.GetAllTitles();
            Countries = _countryRepository.GetAllCountries();
            MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
            IsLife = _policyTypeRepository.IsLife(PolicyTypeID);
            MinimumTerm = _policyTypeRepository.GetMinimumTerm(policyTypeid); 
            PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
            if(!IsLife)
            {
                PolicyTerm = _policyRepository.GetPolicyTerm(PolicyID);
            }           
            LoadProductsSelectList(policyTypeid);
            LoadRelationshipsSelectList();
            LoadLIRolesSelectList();
            LoadGenderSelectList();
            LoadTitleSelectList();
            LoadMaritalSelectList();
            LoadCountrySelectList();
            BeneficiariesDT = _policyBeneficiaryRepository.GetBeneficiaryList(policyid); 
            PolicyBeneficiaryLineDetails = _policyBeneficiaryLineRepository.GetPolicyBeneficiaryLineDetailsByPolicyID(policyid);
            return Page();
        }
        private void LoadProductsSelectList(Guid PolicyTypeID)
        {
            foreach (Product product in _policyTypesLinesRepository.GetAllProducts(PolicyTypeID))
            {
                MyProducts.Add(new SelectListItem
                {
                    Value = product.ID.ToString(),
                    Text = product.ProductName + " (" + product.Category + ")"
                });
            }
        }
        private void LoadRelationshipsSelectList()
        {
            foreach (Relationship relationship in _relationshipRepository.GetAllRelationships())
            {
                MyRelationships.Add(new SelectListItem
                {
                    Value = relationship.ID.ToString(),
                    Text = relationship.RelationshipName
                });
            }
        }
        private void LoadLIRolesSelectList()
        {
            foreach (LIRole lIRole in _lIRoleRepository.GetAllLIRoles())
            {
                LIRoles.Add(new SelectListItem
                {
                    Value = lIRole.ID.ToString(),
                    Text = lIRole.RoleName
                });
            }
        }
        private void LoadGenderSelectList()
        {
            foreach (Gender gender in Genders)
            {
               GenderList.Add(new SelectListItem
                {
                    Value =gender.Id.ToString(),
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
        public IActionResult OnPostAddRoles(Guid id, Guid policyTypeid, Guid policyid)
        { 
            string AddedBy = _userManager.GetUserId(User).ToString();
            PolicyID = policyid;
            MemberID = id;
            PolicyTypeID = policyTypeid;
            Member = _memberRepository.GetMemberById(id);
            if (Member == null)
            {
                return NotFound();
            }
            Genders = _genderRepository.GetAllGenders();
            Titles = _titleRepository.GetAllTitles();
            Countries = _countryRepository.GetAllCountries();
            MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
            PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
            PolicyTerm = _policyRepository.GetPolicyTerm(policyid); 
            LoadProductsSelectList(policyTypeid);
            LoadRelationshipsSelectList();
            LoadLIRolesSelectList();
            LoadGenderSelectList();
            LoadTitleSelectList();
            LoadMaritalSelectList();
            LoadCountrySelectList();
            //int age = Calculations.CalculateCurrentAge((DateTime)member.DOB, DateTime.Now);
            //if (age < 18)
            //{
            //    //how do we handle this?
            //}
            if (IsPremiumPayer)
            {
                BusinessRulesParameters newBusinessParameters = new BusinessRulesParameters();
                newBusinessParameters.PolicyTypeID = PolicyTypeID;
                newBusinessParameters.PolicyID = PolicyID;
                newBusinessParameters.PremiumPayerID = Member.ID;
                StatusReport statusReport = _businessRuleRepository.CheckRules(PolicyTypeID, "Premium Payer", newBusinessParameters);
                if (statusReport.SuccessStatus == 0)
                {
                    throw new Exception(statusReport.StatusMessage);
                }
                PolicyPremium policyPremium = new PolicyPremium();
                policyPremium.HeaderID = policyid;
                policyPremium.PremiumPayer = Member.ID;
                policyPremium.AddedBy = AddedBy;
                policyPremium.AddedOn= DateTime.Now;
                _policyPremiumRepository.AddPolicyPremium(policyPremium);   
            }
            if (IsPrincipalMember)
            {
                BusinessRulesParameters newBusinessParameters = new BusinessRulesParameters();
                newBusinessParameters.PolicyTypeID = PolicyTypeID;
                newBusinessParameters.PolicyID = PolicyID;
                newBusinessParameters.PrincipalMemberID = Member.ID;
                StatusReport statusReport = _businessRuleRepository.CheckRules(PolicyTypeID, "Principal Member", newBusinessParameters);
                if (statusReport.SuccessStatus == 0)
                {
                    _policyRepository.UpdatePolicyStatus(PolicyID, 2, statusReport.RuleID, statusReport.StatusID, statusReport.StatusReasonID, statusReport.StatusMessage, AddedBy);
                    // _policyRepository.UpdatePolicyStatus(PolicyID, statusReport.StatusID, statusReport.StatusReasonID, statusReport.StatusMessage, AddedBy);
                    throw new Exception(statusReport.StatusMessage);
                }
                PolicyBeneficiary policyBeneficiary = new PolicyBeneficiary();
                policyBeneficiary.MemberID = Member.ID;
                policyBeneficiary.HeaderID = policyid;
                policyBeneficiary.LIRole = 1; //principal member
                policyBeneficiary.RelationshipID = 0; //Self
                policyBeneficiary.IDType = 1; //National ID, this will need adjustment
                policyBeneficiary.AddedOn = DateTime.Now;
                policyBeneficiary.AddedBy = AddedBy;
                if(_policyBeneficiaryRepository.CountPrincipalMembers(policyid)==0) 
                {
					_policyBeneficiaryRepository.InsertPolicyBeneficiary(policyBeneficiary);
				}                             
            }
            return Redirect("Applicants?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
        }       
        public IActionResult OnPostAddBeneficiary(Guid id, Guid policyTypeid, Guid policyid)
        {
            try
            {
                PolicyID = policyid;
                MemberID = id;
                PolicyTypeID = policyTypeid;
                string AddedBy = _userManager.GetUserId(User).ToString();                
                Member = _memberRepository.GetMemberById(id);
                if (Member == null)
                {
                    return NotFound();
                }
                Genders = _genderRepository.GetAllGenders();
                Titles = _titleRepository.GetAllTitles();
                Countries = _countryRepository.GetAllCountries();
                MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
                PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
                LoadProductsSelectList(policyTypeid);
                LoadRelationshipsSelectList();
                LoadLIRolesSelectList();
                LoadGenderSelectList();
                LoadTitleSelectList();
                LoadMaritalSelectList();
                LoadCountrySelectList();
                if (string.IsNullOrEmpty((IDContent))||string.IsNullOrEmpty(ConfirmIDContent)|| (IDType==0))
                {
                    throw new Exception("Please select a valid identity option and add the corresponding values!");
                }
                if (IDContent != ConfirmIDContent)
                {
                    throw new Exception("ID confirmation values do not match!");
                }
                if (RelationshipID == 0)
                {
                    if (_policyBeneficiaryRepository.CountRelationship(0, policyid) > 0)
                    {
                        throw new Exception("A member with relationship self already exists!");
                    }
                }
                if (LIRoleID == 1)
                {
                    if (_policyBeneficiaryRepository.CountPrincipalMembers(policyid) > 0)
                    {
                        throw new Exception("There can only be one principal member!");
                    }
                }             
                Member member = _memberRepository.GetMemberById(IDContent);
                if (member == null)
                {
                    if ((NewMember.Name1 == null) || (NewMember.Name3 == null) || (NewMember.GenderID == null) || (NewMember.TitleID == null) || (NewMember.MaritalStatusID == null) || (NewMember.DOB == null) || (NewMember.BirthCountryID == null) ||(NewMember.CountryID==null) || (RelationshipID == -1) || (LIRoleID == 0))
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
                BusinessRulesParameters newBusinessParameters = new BusinessRulesParameters();
                newBusinessParameters.PolicyTypeID = PolicyTypeID;
                newBusinessParameters.PolicyID = PolicyID;
                newBusinessParameters.BeneficiaryUID = member.UID; 
                StatusReport statusReport = _businessRuleRepository.CheckRules(PolicyTypeID, "Beneficiaries", newBusinessParameters);
                if (statusReport.SuccessStatus == 0)
                {
                    _policyRepository.UpdatePolicyStatus(PolicyID, 2, statusReport.RuleID, statusReport.StatusID, statusReport.StatusReasonID, statusReport.StatusMessage, AddedBy);
                    // _policyRepository.UpdatePolicyStatus(PolicyID, statusReport.StatusID, statusReport.StatusReasonID, statusReport.StatusMessage, AddedBy);
                    throw new Exception(statusReport.StatusMessage);
                }
                PolicyBeneficiary policyBeneficiary = new PolicyBeneficiary();
                policyBeneficiary.MemberID = member.ID;
                policyBeneficiary.HeaderID = policyid;            
                int age = Calculations.CalculateCurrentAge((DateTime)member.DOB, DateTime.Now);
                if (age < 18)
                {
                    policyBeneficiary.LIRole = 6;// minor
                }
                else
                {
                    policyBeneficiary.LIRole = 5;// adult beneficiary 
                }             
                
                policyBeneficiary.RelationshipID = RelationshipID;               
                policyBeneficiary.IDType = IDType;
                policyBeneficiary.AddedOn = DateTime.Now;
                policyBeneficiary.AddedBy = AddedBy; 
                _policyBeneficiaryRepository.InsertPolicyBeneficiary(policyBeneficiary);  
                BeneficiariesDT = _policyBeneficiaryRepository.GetBeneficiaryList(policyid);
                return Redirect("Applicants?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostAddProductPremium(Guid id, Guid policyTypeid, Guid policyid)
        {
            string AddedBy = _userManager.GetUserId(User).ToString();
            PolicyBeneficiaryLine.AddedOn = DateTime.Now;
            PolicyBeneficiaryLine.AddedBy = AddedBy;
            PolicyBeneficiaryLine.Current = 1;
            BusinessRulesParameters newBusinessParameters = new BusinessRulesParameters();
            newBusinessParameters.ProductID = PolicyBeneficiaryLine.ProductID;
            newBusinessParameters.Premium = PolicyBeneficiaryLine.Contribution;
            newBusinessParameters.Cover = PolicyBeneficiaryLine.Cover;
            newBusinessParameters.PolicyBeneficiary = PolicyBeneficiaryLine.HeaderID;  
            newBusinessParameters.PolicyID = PolicyID;
            StatusReport statusReport = _businessRuleRepository.CheckRules(PolicyTypeID, "Premiums", newBusinessParameters);
            if (statusReport.SuccessStatus == 0)
            {
                _policyRepository.UpdatePolicyStatus(PolicyID, 2, statusReport.RuleID, statusReport.StatusID, statusReport.StatusReasonID, statusReport.StatusMessage, AddedBy);
                //_policyRepository.UpdatePolicyStatus(PolicyID, statusReport.StatusID, statusReport.StatusReasonID, statusReport.StatusMessage, AddedBy);
                throw new Exception(statusReport.StatusMessage);
            }
            _policyBeneficiaryLineRepository.InsertPolicyBeneficiaryLine(PolicyBeneficiaryLine);
            _policyPremiumRepository.UpdatePremiumAmount(policyid);
            return Redirect("Applicants?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
        }
        public IActionResult OnPostUpdatePremiums(Guid id, Guid policyTypeid, Guid policyid)
        {              
            PolicyBeneficiaryLine pbl = new PolicyBeneficiaryLine();
            string AddedBy = _userManager.GetUserId(User).ToString();
            pbl.AddedOn = DateTime.Now;
            pbl.AddedBy = AddedBy;
            foreach (PolicyBeneficiaryLineDetail beneficiaryLine in PolicyBeneficiaryLineDetails)
            {
                pbl.ID = beneficiaryLine.LineId;
                pbl.Contribution = beneficiaryLine.Premium;
                pbl.Cover = beneficiaryLine.Cover;
                _policyBeneficiaryLineRepository.UpdatePolicyBeneficiaryLine(pbl);
            }
            _policyPremiumRepository.UpdatePremiumAmount(policyid); 
            Genders = _genderRepository.GetAllGenders();
            Titles = _titleRepository.GetAllTitles();
            Countries = _countryRepository.GetAllCountries();
            MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
            PolicyName = _policyTypeRepository.GetPolicyTypeName(policyTypeid);
            LoadProductsSelectList(policyTypeid);
            LoadRelationshipsSelectList(); 
            return Redirect("Applicants?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
            //} 
            //return Page();
        }
        public IActionResult OnPostNext(Guid id, Guid policyTypeid, Guid policyid)
        {
            try
            {
                int testedBusiness = 1; //need a function to fetch this.
                string AddedBy = _userManager.GetUserId(User).ToString();
                DataTable DT = _policyBeneficiaryRepository.GetRequiredPolicyDocumentsList(policyid, testedBusiness);
                foreach (DataRow DR in DT.Rows)
                {
                    int policyBeneficiaryLineID = Convert.ToInt32(DR["PolicyBeneficiaryLineID"]);
                    int productDocumentID = Convert.ToInt32(DR["ProductDocumentID"]);
                    Guid documentID = Guid.Parse(DR["DocumentID"].ToString());
                    _policyBeneficiaryLineRepository.InsertPolicyBeneficiaryLineDocument(policyBeneficiaryLineID, productDocumentID, documentID, AddedBy);
                }
                foreach (DataRow DR in _policyRepository.GetRequiredQuestionnaires(policyid).Rows)
                {
                    QuestionnaireResponse questionnaireResponse = new QuestionnaireResponse
                    {
                        PolicyID = policyid,
                        MemberUID = Guid.Parse(DR["MembersUID"].ToString()),
                        Questionnaire = Guid.Parse(DR["QuestionnaireID"].ToString()),
                        ID = Guid.NewGuid(),
                        AddedBy = AddedBy,
                        AddedOn = DateTime.Now
                    };
                    _questionnaireResponseRepository.PreInsertQuestionnaireResponse(questionnaireResponse);
                }
                _policyPremiumLineRepository.UpdatePolicyPremiumLines(policyid);
                _policyRepository.UpdatePolicyTerm(policyid, PolicyTerm);
                BusinessRulesParameters newBusinessParameters = new()
                {
                    PolicyTypeID = PolicyTypeID,
                    PolicyID = PolicyID
                };
                StatusReport statusReport = _businessRuleRepository.CheckRules(PolicyTypeID, "Terms", newBusinessParameters);
                if (statusReport.SuccessStatus == 0)
                {
                    //_policyRepository.UpdatePolicyStatus(PolicyID, statusReport.StatusID, statusReport.StatusReasonID, statusReport.StatusMessage, AddedBy);
					_policyRepository.UpdatePolicyStatus(PolicyID, 2, statusReport.RuleID, statusReport.StatusID, statusReport.StatusReasonID, statusReport.StatusMessage, AddedBy);
					throw new Exception(statusReport.StatusMessage);
                }
                else
                {
                    //_policyRepository.UpdatePolicyStatus(PolicyID, 2, statusReport.RuleID, statusReport.StatusID, statusReport.StatusReasonID, statusReport.StatusMessage, AddedBy);                   
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }           
            return Redirect("ivshares?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid); 
        }
        public IActionResult OnPostArchiveBeneficiary(int id, Guid policyTypeid, Guid policyid, Guid memberID)
        {
            string archivedBy = _userManager.GetUserId(User).ToString();
            _policyBeneficiaryLineRepository.ArchiveBeneficiaryRecords(policyid, id, archivedBy);
            return Redirect("Applicants?id=" + memberID + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
        }
    }
}
