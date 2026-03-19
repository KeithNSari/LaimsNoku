using LAIMS.Interfaces;
using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Models.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Transactions; 

namespace LAIMS.Areas.UTools.Pages.Organisations
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        public DataTable DT;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        private readonly IMemberContactRepository _memberContactRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IUploadData _dataUpload;        
        [BindProperty]
        public IFormFile Upload { get; set; }
        public string SearchPageUrl;

        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        public IndexModel (IWebHostEnvironment environment,
            IUploadData dataUpload,
            UserManager<ApplicationUser> userManager,
            IMemberRepository memberRepository,
            IMemberContactRepository memberContactRepository,
            ICountryRepository countryRepository)
        {
            _environment = environment;
            _userManager = userManager;
            _memberRepository = memberRepository;
            _dataUpload = dataUpload;
            _memberContactRepository = memberContactRepository;
            _countryRepository = countryRepository;
        }
        public async Task OnPostAsync()
        {
            try 
            {
                if (Upload != null)
                {
                    int addedCount = 0;
                    int existingCount = 0;
                    using (TransactionScope TS = new TransactionScope())
                    {
                        Guid BatchID = Guid.NewGuid();
                        string AddedBy = _userManager.GetUserId(User).ToString();
                        string file = _dataUpload.Documentupload(Upload);
                        FileContents fileContents = _dataUpload.ExcelData(file);
                        DT = RemoveNullRows(fileContents.DataDT);
                        int contactsCount = 0;
                        for (int i = 0; i < fileContents.ColumnsList.Count; i++)
                        {
                            if (fileContents.ColumnsList[i].Contains("Contact")) contactsCount++;
                        }
                        ValidateFile(contactsCount, fileContents.ColumnsList);
                        int cityID = 0;
                        foreach (DataRow DR in DT.Rows)
                        {
                            if (_memberRepository.CheckMemberNameExistence(DR["Organisation"].ToString())>-1)
                            {
                                existingCount += 1;
                                continue;
                            }
                            if (DR["Country"] != null)
                            {
                                Country country = _countryRepository.GetCountryName(DR["Country"].ToString());
                                if (country.CountryID == 0) throw new Exception("Invalid country found: " + DR["Country"].ToString());
                                cityID = _countryRepository.AddCity(country.CountryID, DR["City"].ToString(), AddedBy, DateTime.Now);
                            }
                            Member member = new()
                            {
                                BatchID = BatchID,
                                UID = Guid.NewGuid(),
                                Name1 = DR["Organisation"].ToString(),
                                IsOrganisation = 1,
                                AddedBy = AddedBy
                            };
                            _memberRepository.AddMember(member);
                            MemberContact memberContact = new MemberContact
                            {
                                AddedBy = AddedBy,
                                ContactTypeID = 3,
                                City = cityID,
                                Line1 = DR["Address Line 1"].ToString(),
                                Line2 = DR["Address Line 2"].ToString(),
                                Line3 = DR["Address Line 3"].ToString(),
                                MemberUID = member.UID
                            };
                            _memberContactRepository.InsertMemberContact(memberContact);
                            addedCount += 1;
                        }
                        TS.Complete();
                    }
                    if (existingCount > 0)
                    {
                        ViewData["Notification"] = addedCount + " organisation/s added, " + existingCount + " were already existing!";
                    }
                    else
                    {
                        ViewData["Notification"] = addedCount + " organisation/s added.";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
            }           
        }
        public void OnGet()
        {
            try
            {
                DT = _memberRepository.GetLatestOrganisations();
                ViewData["Notification"] = "Showing " + DT.Rows.Count + " organisation/s!";
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
            }           
        }

        DataTable RemoveNullRows(DataTable DT)
        {
            for (int i = DT.Rows.Count - 1; i >= 0; i--)
            {
                DataRow row = DT.Rows[i];
                if (row["Organisation"] == DBNull.Value || row["Organisation"] == null)
                {
                    DT.Rows.RemoveAt(i);
                }
            }
            return DT;
        }
        public void OnGetSearch(string? search)
        {
            try
            {
                SearchTerm = search;
                SearchPageUrl = "/utools/organisations/Index";
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    DT = _memberRepository.SearchOrganisations2(SearchTerm);
                    ResultsCount = DT.Rows.Count;
                    ViewData["Notification"] = ResultsCount + " organisation/s found!";
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
            }           
        }
        private void ValidateFile(int ContactCount,  List<string> columnNames)
        {
           List<string> requiredColumns = new List<string>
            {
            "Organisation", 
            "Address Line 1",
            "Address Line 2",
            "Address Line 3",
            "City",
            "Country",
            "Contact",
            "Designation",
            "Email Address",
            "Cellphone",
            "Phone Number"
           };

            foreach (string column in requiredColumns)
            { 
                if (!columnNames.Contains(column))
                {
                    throw new Exception($"Column '{column}' is missing.");
                }
            }
            if (ContactCount > 1)
            {
                //verify that there are corresponding Designation, Email Address, Cellphone, Phone Number columns
                for(int i=1;i<ContactCount;i++)
                {
                    string contactColName= "Contact" + i.ToString();
                    if (!columnNames.Contains(contactColName))
                    {
                        throw new Exception("There is an incorrect contact column title!");
                    }
                    string designationColName = "Designation" + i.ToString();
                    if (!columnNames.Contains(designationColName))
                    {
                        throw new Exception("There is a missing or incorrect designation column!");
                    }
                    string emailAddressColName = "Email Address" + i.ToString();
                    if (!columnNames.Contains(emailAddressColName))
                    {
                        throw new Exception("There is a missing or incorrect email address column!");
                    }
                    string cellphoneColName="Cellphone" + i.ToString();
                    if (!columnNames.Contains(cellphoneColName))
                    {
                        throw new Exception("There is a missing or incorrect cellphone column!");
                    }
                    string phoneNumberColName = "Phone Number" + i.ToString();
                    if (!columnNames.Contains(phoneNumberColName))
                    {
                        throw new Exception("There is a missing or incorrect phone number column!");
                    }
                }
            }
        }

        public IActionResult OnGetLoadContacts(Guid id)
        {
            // Use the id parameter in your logic to fetch data
            var data = _memberContactRepository.GetAllMemberContacts(id); 
            return new JsonResult(data);
        } 
    }
}
