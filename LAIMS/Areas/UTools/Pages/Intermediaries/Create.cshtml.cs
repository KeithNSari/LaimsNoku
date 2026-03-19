using LAIMS.Interfaces;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Membership;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using System.Data;

namespace LAIMS.Areas.UTools.Pages.Intermediaries
{
	[Authorize(Roles = "Commission Intermediaries")]
	public class CreateModel : PageModel
    {
        public DataTable DT;
        private readonly IWebHostEnvironment _environment; 
        private readonly IUploadData _dataUpload;
        private readonly IIntermediaryRepository _intermediaryRepository;
        private readonly IIntermediaryTypeRepository _intermediaryTypeRepository; 
        private readonly IMemberRepository _memberRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(IWebHostEnvironment environment,
            IUploadData dataUpload,
            UserManager<ApplicationUser> userManager,
            IIntermediaryRepository intermediaryRepository,
            IMemberRepository memberRepository,
            ICountryRepository countryRepository,
            IIntermediaryTypeRepository intermediaryTypeRepository
            )
        {
            _environment = environment;
            _userManager = userManager;
            _dataUpload = dataUpload;
            _intermediaryRepository = intermediaryRepository;
            _memberRepository = memberRepository;
            _countryRepository = countryRepository;
            _intermediaryTypeRepository = intermediaryTypeRepository;
        }
        [BindProperty]
        public IFormFile Upload { get; set; }
        public IActionResult OnPostAsync()
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
               
                if (Upload != null)
                {
                    var extension = Path.GetExtension(Upload.FileName);
                    if (!extension.Contains(".xls"))
                    {
                        throw new Exception($"This file extension is not allowed!");
                    }
                    Guid BatchID = Guid.NewGuid();
                    string AddedBy = _userManager.GetUserId(User).ToString();
                    string file = _dataUpload.Documentupload(Upload);
                    DT = RemoveNullRows(_dataUpload.ExcelDataTable(file));
                    List<IntermediaryImport> intermediaryImports = _intermediaryRepository.FormatIntermediaries(DT);
                    foreach (IntermediaryImport intermediaryImport in intermediaryImports)
                    {
                        if (!string.IsNullOrEmpty(intermediaryImport.AgentCode))
                        {
                            Country country = new Country();
                            int cityID = 0;
                            if (intermediaryImport.Country != null)
                            {
                                country = _countryRepository.GetCountryName(intermediaryImport.Country);
                                if (country.CountryID == 0) throw new Exception("Invalid country found: " + intermediaryImport.Country);
                                cityID = _countryRepository.AddCity(country.CountryID, intermediaryImport.City.Trim(), AddedBy, DateTime.Now);
                            }
                            Member member = new()
                            {
                                UID = Guid.NewGuid(),
                                Name1 = intermediaryImport.FirstName,
                                Name2 = intermediaryImport.MiddleName,
                                Name3 = intermediaryImport.Surname,
                                NationalID = intermediaryImport.NationalID,
                                CountryID = country.CountryID,
                                Confirmed = 1
                            };
                            _memberRepository.AddMember(member);
                            int designationID = _intermediaryRepository.GetDesignationID(intermediaryImport.Designation.Trim());
                            int intermediaryTypeID = _intermediaryTypeRepository.GetTypeID(intermediaryImport.TypeOfAgent.Trim());
                            Intermediary intermediary = new()
                            {
                                BatchID = BatchID,
                                IntermediaryTypeID = intermediaryTypeID,
                                ReportsToAgentCode = intermediaryImport.ReportsTo,
                                MemberID = member.ID,
                                Started = intermediaryImport.DateOfAppointment,
                                Ended = intermediaryImport.DateOfExit,
                                DesignationID = designationID,
                                AgentCode = intermediaryImport.AgentCode,
                                EmployeeNo = intermediaryImport.EmployeeNumber
                            };
                            _intermediaryRepository.AddIntermediary(intermediary);
                        }
                    }
                    _intermediaryRepository.UpdateIntemediarySupervisors(BatchID);
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
            return Page();
        }
        public void OnGet()
        {
            DT = _intermediaryRepository.Get();
        }
        DataTable RemoveNullRows(DataTable DT)
        { 
            for (int i = DT.Rows.Count - 1; i >= 0; i--)
            {
                DataRow row = DT.Rows[i]; 
                if (row["Agent Code"] == DBNull.Value || row["Agent Code"]== null)
                { 
                    DT.Rows.RemoveAt(i);
                }
            } 
            return DT;
        }
    }
}
