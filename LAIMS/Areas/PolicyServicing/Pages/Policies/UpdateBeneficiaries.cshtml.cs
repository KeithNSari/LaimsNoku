using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Membership;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Lifeproducts;
using LAIMS.Repositories.Premiums;
using LAIMS.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Text.RegularExpressions;

namespace LAIMS.Areas.PolicyServicing.Pages.Policies
{
    public class UpdateBeneficiariesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyRepository _policyRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IPolicyBeneficiaryRepository _policyBeneficiaryRepository;
        private readonly IPolicyBeneficiaryLineRepository _policyBeneficiaryLineRepository;
        private readonly IGenderRepository _genderRepository;
        private readonly ITitleRepository _titleRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMaritalStatusRepository _maritalStatusRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        public List<SelectListItem> GenderList = new List<SelectListItem>();
        public List<SelectListItem> TitleList = new List<SelectListItem>();
        public List<SelectListItem> MaritalStatiiSelectList = new List<SelectListItem>();
        public List<SelectListItem> CountrySelectList = new List<SelectListItem>();
        public List<SelectListItem> MyRelationships = new List<SelectListItem>();
        public UpdateBeneficiariesModel(UserManager<ApplicationUser> userManager,
            IMemberRepository memberRepository,
            IPolicyTypeRepository policyTypeRepository,
            IPolicyRepository policyRepository,
            IPolicyBeneficiaryRepository policyBeneficiaryRepository,
            IPolicyBeneficiaryLineRepository policyBeneficiaryLineRepository,
            IGenderRepository genderRepository, 
            IRelationshipRepository relationshipRepository,
            ITitleRepository titleRepository,
            ICountryRepository countryRepository,
            IMaritalStatusRepository maritalStatusRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository; 
            _policyRepository = policyRepository;
            _memberRepository = memberRepository;
            _genderRepository = genderRepository;
            _titleRepository = titleRepository;
            _countryRepository = countryRepository;
            _maritalStatusRepository = maritalStatusRepository;
            _relationshipRepository = relationshipRepository;
            _policyBeneficiaryRepository = policyBeneficiaryRepository;
            _policyBeneficiaryLineRepository = policyBeneficiaryLineRepository;
        }
        [BindProperty]
        public Policy Policy { get; set; }
        public Member Member { get; set; }
        public DataTable BeneficiariesDT;
        public List<Gender> Genders { get; set; }
        public List<Title> Titles { get; set; }
        public List<Country> Countries { get; set; }
        public List<MaritalStatus> MaritalStatii { get; set; }
        [BindProperty]
        public int IDType { get; set; }
        [BindProperty]
        public string? IDContent { get; set; }
        [BindProperty]
        public string? ConfirmIDContent { get; set; }

        [BindProperty]
        public int RelationshipID { get; set; } = -1;
        [BindProperty]
        public Guid MemberID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public Member NewMember { get; set; }
        [BindProperty]
        public Guid RequestID { get; set; }
        public IActionResult OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                RequestID = reqid;
                PolicyID = policyid;
                MemberID = id;
                PolicyTypeID = policyTypeid;
                Policy = _policyRepository.GetPolicyById(policyid);
                Member = _memberRepository.GetMemberById(id);
                BeneficiariesDT = _policyBeneficiaryRepository.GetStagingBeneficiaryList(policyid, RequestID);
                Genders = _genderRepository.GetAllGenders();
                Titles = _titleRepository.GetAllTitles();
                Countries = _countryRepository.GetAllCountries();
                MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
                LoadPageLookups();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
            return Page();
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
        private void LoadPageLookups()
        {
            LoadRelationshipsSelectList();
            LoadGenderSelectList();
            LoadTitleSelectList();
            LoadMaritalSelectList();
            LoadCountrySelectList();
        }
        private static int CalculateAgeInYears(DateTime dob, DateTime onDate)
        {
            int age = onDate.Year - dob.Year;
            if (dob.Date > onDate.AddYears(-age)) age--;
            return age;
        }
        public IActionResult OnPostAddParticipant(Guid id, Guid policyTypeid, Guid policyid)
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
                Policy = _policyRepository.GetPolicyById(policyid);
                Genders = _genderRepository.GetAllGenders();
                Titles = _titleRepository.GetAllTitles();
                Countries = _countryRepository.GetAllCountries();
                MaritalStatii = _maritalStatusRepository.GetAllMaritalStatuses();
                LoadPageLookups();
                if (string.IsNullOrEmpty((IDContent)) || string.IsNullOrEmpty(ConfirmIDContent) || (IDType == 0))
                {
                    throw new Exception("Please select a valid identity option and add the corresponding values!");
                }
                if (IDContent != ConfirmIDContent)
                {
                    throw new Exception("ID confirmation values do not match!");
                } 
                Member member = _memberRepository.GetMemberById(IDContent);
                if (member == null)
                {
                    if ((NewMember.Name1 == null) || (NewMember.Name3 == null) || (NewMember.GenderID == null) || (NewMember.TitleID == null) || (NewMember.MaritalStatusID == null) || (NewMember.DOB == null) || (NewMember.BirthCountryID == null) || (NewMember.CountryID == null) || (RelationshipID == -1))
                    {
                        throw new Exception("A member with this ID number does not exist, please fill in all required details to create them!");
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

                if (member == null || member.DOB == null)
                {
                    throw new Exception("Unable to validate beneficiary age because Date of Birth is missing.");
                }

                Guid policyTypeID = Policy.PolicyType;
                var ageLimits = _policyBeneficiaryRepository.GetPolicyTypeRelationshipAgeLimits(policyTypeID, RelationshipID);
                int age = CalculateAgeInYears(member.DOB.Value.Date, DateTime.Today);
                if (ageLimits.MinAgeAtEntry.HasValue && age < ageLimits.MinAgeAtEntry.Value)
                {
                    throw new Exception($"Beneficiary age ({age}) is less than the minimum entry age ({ageLimits.MinAgeAtEntry.Value}) configured for this policy type relationship.");
                }
                if (ageLimits.MaxAgeAtEntry.HasValue && age > ageLimits.MaxAgeAtEntry.Value)
                {
                    throw new Exception($"Beneficiary age ({age}) is greater than the maximum entry age ({ageLimits.MaxAgeAtEntry.Value}) configured for this policy type relationship.");
                }

                PolicyBeneficiary policyBeneficiary = new PolicyBeneficiary();                
                policyBeneficiary.MemberID = member.ID;
                policyBeneficiary.HeaderID = policyid;  
                policyBeneficiary.RelationshipID = RelationshipID;
                policyBeneficiary.IDType = IDType;
                policyBeneficiary.Beneficiary = 1;
                policyBeneficiary.AddedOn = DateTime.Now;
                policyBeneficiary.AddedBy = AddedBy;
                _policyBeneficiaryRepository.InsertPolicyBeneficiaryStaging(policyBeneficiary, RequestID);
                return Redirect(ReturnUrl);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPost()
        {
            try
            {
                bool HasInvestmentContent = _policyTypeRepository.HasInvestmentProduct(PolicyTypeID);
                if (HasInvestmentContent)
                {
                    return Redirect("BeneficiaryShares?id=" + MemberID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);            
                }
                return Redirect("BeneficiaryIndex?id=" + MemberID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + RequestID);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostArchiveBeneficiary(int id, Guid policyTypeid, Guid policyid, Guid memberID, Guid reqid)
        {
            try
            {
                string archivedBy = _userManager.GetUserId(User).ToString();
                _policyBeneficiaryLineRepository.ArchiveBeneficiaryStagingRecords(policyid, id, archivedBy);
                bool HasInvestmentContent = _policyTypeRepository.HasInvestmentProduct(PolicyTypeID);
                if (HasInvestmentContent)
                {                    
                    return Redirect("UpdateBeneficiaries?id=" + MemberID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + reqid);
                }
                return Redirect("UpdateBeneficiaries?id=" + MemberID + "&policytypeid=" + PolicyTypeID + "&policyid=" + PolicyID + "&reqid=" + reqid);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
}
