using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Models.Documents;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Membership;
using LAIMS.Models.Security;
using LAIMS.Repositories.Membership;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using NuGet.Protocol.Core.Types;
using System.Data;
using System.Globalization;

namespace LAIMS.Areas.NewBusiness.Pages.Membership
{
    [Authorize(Roles = "Policy Servicing Initiator, New Business Initiator")]
    public class MemberProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        private readonly IContactTypeRepository _contactTypeRepository;
        private readonly IMemberContactRepository _memberContactRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IMediaUploadRepository _mediaUploadRepository;
        private readonly IDocumentsRepository _documentsRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IStatiiRepository _statiiRepository;
        private readonly IStatiiReasonsRepository _statiiReasonsRepository;
        private readonly IMemberStatusRepository _memberStatusRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public List<SelectListItem> DocumentsList = new List<SelectListItem>();
        public DataTable DocumentsDT;
        public DataTable ContactsDT;
        public DataTable PoliciesDT;
        public DataTable MemberStatiiDT;
        [BindProperty]
        public MemberStatus MemberStatus { get; set; }
        [BindProperty]
        public MemberContact MemberContact { get; set; }
        public Member Member { get; set; }
        [BindProperty]
        public string MemberDOB { get; set; }

        [BindProperty]
        public Guid DocumentsID { get; set; }
        [BindProperty]
        public IFormFile Upload { get; set; }
        public List<ContactType> ContactTypes { get; set; }
        public List<Statii> StatiiList { get; set; }
        public List<StatiiReason > StatiiReasons { get; set; }
        public List<Country> Countries { get; set; }
        public SelectList Cities { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CountryID { get; set; }
        [BindProperty(SupportsGet = true)]
        public int StatusID { get; set; }
        [BindProperty]
        public int MemberStatusID { get; set; }
        [BindProperty]
        public int MemberStatusReasonID { get; set; }

        [BindProperty]
        public int CityID { get; set; }

        [BindProperty]
        public string CustomCity { get; set; }
        public string SearchPageUrl { get; set; }= "SearchMembers"; 
        public MemberProfileModel(UserManager<ApplicationUser> userManager, IMemberRepository memberRepository, 
            IContactTypeRepository contactTypeRepository, IMemberContactRepository memberContactRepository,
            ICountryRepository countryRepository, ICityRepository cityRepository, IDocumentsRepository documentsRepository, 
            IMediaUploadRepository mediaUploadRepository,IPolicyRepository policyRepository, IMemberStatusRepository memberStatusRepository,
            IStatiiRepository statiiRepository,IWebHostEnvironment webHostEnvironment, IStatiiReasonsRepository statiiReasonsRepository)
        {
            _userManager = userManager;
            _memberContactRepository = memberContactRepository;
            _contactTypeRepository = contactTypeRepository;
            _memberContactRepository = memberContactRepository;
            _memberRepository = memberRepository;
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
            _documentsRepository = documentsRepository;
            _mediaUploadRepository = mediaUploadRepository;
            _policyRepository = policyRepository;
            _webHostEnvironment = webHostEnvironment;
            _statiiRepository = statiiRepository;
            _statiiReasonsRepository = statiiReasonsRepository;
            _memberStatusRepository = memberStatusRepository;
        }
       
        public IActionResult OnGet(Guid id)
        {
            try
            {
                Member = _memberRepository.GetMemberById(id);
                if (Member == null)
                {
                    return NotFound();
                }
                if (!_memberRepository.CheckIDConfirm(id))
                {
                    return Redirect("ConfirmIdentity?id=" + id);
                }
                MemberDOB = Convert.ToDateTime(Member.DOB).ToString("dd MMMM yyyy");
                LoadPageComponents(id);
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = "" });
            }
        }
        public void LoadPageComponents(Guid id)
        {
            
            ContactTypes = _contactTypeRepository.GetAllContactTypes();
            StatiiList = _statiiRepository.GetAllMemberStatii();
            Countries = _countryRepository.GetAllCountries();
            ContactsDT = _memberContactRepository.Get(id);
            MemberStatiiDT = _memberStatusRepository.GetMemberStatusHistory(id);  
            //Documents Section 
            DocumentsDT =_mediaUploadRepository.GetDocuments (id);
            LoadDocumentsSelectList();
            PoliciesDT = _policyRepository.GetMemberPolicies(id); 
        }
        private void LoadDocumentsSelectList()
        {
            foreach (Document document in _documentsRepository.GetAllDocuments())
            {
                DocumentsList.Add(new SelectListItem
                {
                    Value = document.ID.ToString(),
                    Text = document.DocumentName
                });
            }
        }
        public JsonResult OnGetCitiesByCountry()
        {
            var cities = _cityRepository.GetCitiesByCountry(CountryID);
            return new JsonResult(cities);
        }
        public JsonResult OnGetReasonsByStatus()
        {
            var reasons = _statiiReasonsRepository.GetAllStatiiReasons(StatusID);
            return new JsonResult(reasons);
        }
        public IActionResult OnPostAddContact(Guid id)
        {
            try
            {
                string AddedBy = _userManager.GetUserId(User).ToString();
                MemberContact.AddedBy = AddedBy;
                if (CityID == 0)
                {
                    if (!string.IsNullOrEmpty(CustomCity))
                    {
                        if (MemberContact.CountryID == 0)
                        {
                            throw new Exception("Country must be selected to add a new city!");
                        }
                        CityID = _countryRepository.AddCity(MemberContact.CountryID, CustomCity, AddedBy, DateTime.Now);
                    }
                    CityID = -1;
                }
                MemberContact.City = CityID;
                MemberContact.MemberUID = id;
                _memberContactRepository.InsertMemberContact(MemberContact);
                LoadPageComponents(id);
                return Redirect("MemberProfile?tab=tab2&id=" + id.ToString());
            } 
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = "" });
            } 
        }
        public IActionResult OnPostAddStatus(Guid id)
        {
            try
            {
                string AddedBy = _userManager.GetUserId(User).ToString();
                MemberStatus.Status = MemberStatusID;
                MemberStatus.StatusReason = MemberStatusReasonID; 
                MemberStatus.MemberUID = id;
                MemberStatus.AddedBy = AddedBy;
                MemberStatus.AddedOn = DateTime.Now; 
                MemberStatus.StatusDate = DateTime.Now;
                _memberStatusRepository.AddMemberStatus(MemberStatus);
                LoadPageComponents(id);
                return Redirect("MemberProfile?tab=tab5&id=" + id.ToString());
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = "" });
            }
        }
        public IActionResult OnPostArchive(Guid MemberUID, int ContactID)
        {
            try 
            {
                string archivedBy = _userManager.GetUserId(User).ToString();
                _memberContactRepository.ArchiveMemberContact(MemberUID, ContactID, archivedBy);
                return Redirect("memberprofile?id=" + MemberUID.ToString());
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = "" });
            }
        }
        public IActionResult OnPostUploadFile(IFormFile postedFile, Guid id)
        {
            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads"); 
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Upload.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                string contentType = Upload.ContentType;
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Upload.CopyTo(stream);
                }
                var mediaUpload = new MediaUpload
                {
                    ID = Guid.NewGuid(),
                    MemberUID = id,
                    DocumentsID = DocumentsID,
                    FileName = uniqueFileName,
                    ContentType = contentType,
                    Data = System.IO.File.ReadAllBytes(filePath),
                    AddedOn = DateTime.Now,
                    AddedBy = _userManager.GetUserId(User).ToString()
                };
                _mediaUploadRepository.SaveMediaUpload(mediaUpload);
                return Redirect("memberprofile?tab=tab3&id=" + id.ToString());
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = "" });
            }
        } 
        public IActionResult OnPostDownloadFile(Guid id, Guid docId)
        {
            try
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
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = "" });
            }            
        }
        public IActionResult OnPostPolicyDetails(Guid memberId,Guid policyTypeId, Guid policyId)
        {             
            int stage = _policyRepository.GetPolicyStage(policyId); 
            switch (stage)
            {
                case 2:
                    return Redirect("~/newbusiness/policies/AddDetails?id=" + memberId + "&policytypeid=" + policyTypeId + "&policyid=" + policyId);
                case 3:
                    return Redirect("~/newbusiness/policies/ivshares?id=" + memberId + "&policytypeid=" + policyTypeId + "&policyid=" + policyId);
                case 4:
                    return Redirect("~/newbusiness/policies/documents?id=" + memberId + "&policytypeid=" + policyTypeId + "&policyid=" + policyId);
                case 5:
                    return Redirect("~/newbusiness/policies/pbquestionnaires?id=" + memberId + "&policytypeid=" + policyTypeId + "&policyid=" + policyId);
                case 6:
                    return Redirect("~/newbusiness/policies/submission?id=" + memberId + "&policytypeid=" + policyTypeId + "&policyid=" + policyId);
                default: break;
            }
            return Redirect("MemberProfile?id=" + memberId.ToString());
        }
    }
}
