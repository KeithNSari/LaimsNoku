using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces;
using LAIMS.Models.Membership;
using LAIMS.Models.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Transactions;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Premiums;
using LAIMS.Interfaces.Banking;
using LAIMS.Models.Banking;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;

namespace LAIMS.Areas.UTools.Pages.PaymentProviders
{
	[Authorize(Roles = "Payment Servicing")]
	public class CreateModel : PageModel
    {
        public DataTable DT;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        private readonly IMemberContactRepository _memberContactRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IPaymentProviderRepository _paymentProviderRepository;
        private readonly IMemberBankAccountRepository _memberBankAccountRepository;
        private readonly IPremiumCollectionConfigHeaderRepository _premiumCollectionConfigHeaderRepository;
        private readonly IUploadData _dataUpload;
        [BindProperty]
        public IFormFile Upload { get; set; }
        public string SearchPageUrl;

        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        public CreateModel(IWebHostEnvironment environment,
            IUploadData dataUpload,
            UserManager<ApplicationUser> userManager,
            IMemberRepository memberRepository,
            IMemberContactRepository memberContactRepository,
            ICurrencyRepository currencyRepository,
            IPaymentProviderRepository paymentProviderRepository,
            IMemberBankAccountRepository memberBankAccountRepository,
            IPremiumCollectionConfigHeaderRepository premiumCollectionConfigHeaderRepository,
            ICountryRepository countryRepository)
        {
            _environment = environment;
            _userManager = userManager;
            _memberRepository = memberRepository;
            _dataUpload = dataUpload;
            _memberContactRepository = memberContactRepository;
            _countryRepository = countryRepository;
            _currencyRepository = currencyRepository;
            _paymentProviderRepository = paymentProviderRepository;
            _memberBankAccountRepository = memberBankAccountRepository;
            _premiumCollectionConfigHeaderRepository = premiumCollectionConfigHeaderRepository;
        }
        public class PremiumCollectionConfig
        {
            public int MemberID;
            public int PaymentMethodID;
            public string InternalAccount;
            public int CurrencyID;
            public byte Aggregated;
            public string StopOrderName;
            public string StopOrderCode;
            public string RecordsFormat;
            public int SalaryDisbursementDate;
            public int BillingDate;
            public decimal CollectionCommissionRate;
            public byte Net;
            public string AddedBy;
        }
        public async Task OnPostAsync()
        {
            try
            {
                if (Upload != null)
                {
                    var extension = Path.GetExtension(Upload.FileName);
                    if (!extension.Contains(".xls"))
                    {
                        throw new Exception($"This file extension is not allowed!");
                    }
                    int addedCount = 0;
                    int existingCount = 0;
                    int premiumConfigRecordsCount = 0;
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
                            string organisation = DR["Organisation"].ToString();
                            int memberID = _memberRepository.CheckMemberNameExistence(organisation);
                            if (memberID == -1)
                            {
                                if (DR["Country"] != null)
                                {
                                    string countryName = DR["Country"].ToString();
                                    if (string.IsNullOrEmpty(countryName))
                                    {
                                        throw new Exception("Country is a required field!");
                                    }
                                    Country country = _countryRepository.GetCountryName(countryName);
                                    if (DR["City"] == null)
                                    {
                                        throw new Exception("City is a required field!");
                                    }
                                    string cityName = DR["City"].ToString();
                                    if (string.IsNullOrEmpty(cityName))
                                    {
                                        throw new Exception("City is a required field!");
                                    }
                                    if (country.CountryID == 0) throw new Exception("Invalid country found: " + DR["Country"].ToString());
                                    cityID = _countryRepository.AddCity(country.CountryID, cityName, AddedBy, DateTime.Now);
                                }
                                else
                                {
                                    throw new Exception("Country is a required field!");
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
                                memberID = member.ID;
                                addedCount += 1;
                            }
                            else
                            {
                                existingCount += 1;
                            }
                            if (memberID == 0)
                            {
                                throw new Exception("An error has occured, " + organisation + " could not be created.");
                            }
                            Guid UID = _memberRepository.GetUID(memberID);
                            MemberContact memberContact = new MemberContact
                            {
                                AddedBy = AddedBy,
                                ContactTypeID = 3,
                                ContactName = string.Empty,
                                City = cityID,
                                Line1 = DR["Address Line 1"].ToString(),
                                Line2 = DR["Address Line 2"].ToString(),
                                Line3 = DR["Address Line 3"].ToString(),
                                MemberUID = UID
                            };
                            if (!_memberContactRepository.CheckMemberContactExistence(memberContact))
                            {
                                _memberContactRepository.InsertMemberContact(memberContact);
                            }
                            if (contactsCount > 1)
                            {
                                for (int i = 1; i < contactsCount; i++)
                                {
                                    string contactColName = "Contact" + i.ToString();
                                    string contactName = DR[contactColName].ToString();
                                    if (!string.IsNullOrEmpty(contactName))
                                    {
                                        string designationColName = "Designation" + i.ToString();
                                        MemberContact OtherContact = new()
                                        {
                                            ContactName = contactName,
                                            Designation = DR[designationColName].ToString(),
                                            MemberUID = UID
                                        };
                                        string emailAddressColName = "Email Address" + i.ToString();
                                        string emailAddress = DR[emailAddressColName].ToString();
                                        if (!string.IsNullOrEmpty(emailAddress))
                                        {
                                            OtherContact.ContactTypeID = 6;
                                            OtherContact.Line1 = emailAddress;
                                            if (!_memberContactRepository.CheckMemberContactExistence(OtherContact))
                                            {
                                                _memberContactRepository.InsertMemberContact(OtherContact);
                                            }
                                        }
                                        string cellphoneColName = "Cellphone" + i.ToString();
                                        string cellPhone = DR[cellphoneColName].ToString();
                                        if (!string.IsNullOrEmpty(cellPhone))
                                        {
                                            OtherContact.ContactTypeID = 4;
                                            OtherContact.Line1 = cellPhone;
                                            if (!_memberContactRepository.CheckMemberContactExistence(OtherContact))
                                            {
                                                _memberContactRepository.InsertMemberContact(OtherContact);
                                            }
                                        }
                                        string phoneNumberColName = "Phone Number" + i.ToString();
                                        string phone = DR[cellphoneColName].ToString();
                                        if (!string.IsNullOrEmpty(phone))
                                        {
                                            OtherContact.ContactTypeID = 5;
                                            OtherContact.Line1 = phone;
                                            if (!_memberContactRepository.CheckMemberContactExistence(OtherContact))
                                            {
                                                _memberContactRepository.InsertMemberContact(OtherContact);
                                            }
                                        }
                                    }
                                }
                            }
                            Currency currency = new Currency() //clients may specify either short code or name
                            {
                                CurrencyName = DR["Currency"].ToString(),
                                ShortCode = DR["Currency"].ToString()
                            };
                            int currencyID = _currencyRepository.CheckExistence(currency);
                            if (currencyID == 0) throw new Exception("Invalid currency found: " + currency.CurrencyName);
                            byte aggregated = 0;
                            if (DR["Aggregated"].ToString() == "Yes") aggregated = 1;
                            byte net = 1;
                            if (DR["Gross/Net"].ToString() == "Gross") net = 0;
                            PremiumCollectionConfig premiumCollectionConfig = new()
                            {
                                MemberID = memberID,
                                PaymentMethodID = 2,
                                InternalAccount = DR["Receiving A/CNo"].ToString(),
                                CurrencyID = currencyID,
                                Aggregated = aggregated,
                                StopOrderName = DR["Stop Order Name"].ToString(),
                                StopOrderCode = DR["Stop Order Code"].ToString(),
                                RecordsFormat = DR["Format"].ToString(),
                                SalaryDisbursementDate = Convert.ToInt32(DR["Salary Disbursement Date"].ToString()),
                                BillingDate = Convert.ToInt32(DR["Billing Date"].ToString()),
                                CollectionCommissionRate = Convert.ToDecimal(DR["Collection Commission Rate"].ToString()),
                                Net = net,
                                AddedBy = AddedBy
                            };
                            premiumConfigRecordsCount += AddPremiumCollectionConfig(premiumCollectionConfig);
                        }
                        TS.Complete();
                    }
                    if (existingCount > 0)
                    {
                        ViewData["Notification"] = addedCount + " organisation/s added, " + existingCount + " were already existing! " + premiumConfigRecordsCount + " stop order records added.";
                    }
                    else
                    {
                        ViewData["Notification"] = addedCount + " organisation/s added. " + premiumConfigRecordsCount + " stop order records added.";
                    }
                    DT = _premiumCollectionConfigHeaderRepository.GetLatestByPaymentMethod(2);
                }
            }
            catch (Exception ex)
            {
                DT = _premiumCollectionConfigHeaderRepository.GetLatestByPaymentMethod(2);
                ViewData["ErrorMessage"] = ex.Message;
            }
        }
        public void OnGet()
        {
            try
            {
                DT = _premiumCollectionConfigHeaderRepository.GetByPaymentMethod(2);//2 is payment method id for stop orders 
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
                SearchTerm = search.Trim();
                SearchPageUrl = "/utools/paymentproviders/create";
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    DT =_premiumCollectionConfigHeaderRepository.SearchByPaymentMethod(2,SearchTerm); 
                    ResultsCount = DT.Rows.Count;
                    ViewData["Notification"] = ResultsCount + " stop order/s found!";
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
            }
        }
        private void ValidateFile(int ContactCount, List<string> columnNames)
        {
            List<string> requiredColumns = new List<string>
            {
            "Organisation",
            "Stop Order Name",
            "Stop Order Code",
            "Format",
            "Receiving A/CNo",
            "Currency",
            "Aggregated",
            "Address Line 1",
            "Address Line 2",
            "Address Line 3",
            "City",
            "Country",
            "Contact",
            "Designation",
            "Email Address",
            "Cellphone",
            "Phone Number",
            "Salary Disbursement Date",
            "Billing Date",
            "Collection Commission Rate",
            "Gross/Net"
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
                for (int i = 1; i < ContactCount; i++)
                {
                    string contactColName = "Contact" + i.ToString();
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
                    string cellphoneColName = "Cellphone" + i.ToString();
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

        private int AddPremiumCollectionConfig(PremiumCollectionConfig premiumCollectionConfig)
        {
            PaymentProvider paymentProvider = new PaymentProvider()
            {
                MemberID = premiumCollectionConfig.MemberID,
                PaymentMethodID = premiumCollectionConfig.PaymentMethodID,
                AddedBy = premiumCollectionConfig.AddedBy
            };
            int paymentProviderID = _paymentProviderRepository.AddPaymentProvider(paymentProvider);//if record already exists, this returns the id of the record
            if (_premiumCollectionConfigHeaderRepository.CheckPremiumCollectionConfigHeaderExistence(paymentProviderID, premiumCollectionConfig.StopOrderName, premiumCollectionConfig.StopOrderCode, premiumCollectionConfig.CurrencyID))
            {
                return 0;//0 premiumcollectionconfig record added, record already exists
            }
            MemberBankAccount memberBankAccount = new()
            {
                BankID=0, //to represent internal accounts
                BranchCode= premiumCollectionConfig.InternalAccount.Substring(0,4), //hard coded for ZB, will need to be improved
                BankAccountNo = premiumCollectionConfig.InternalAccount,
                CurrencyID = premiumCollectionConfig.CurrencyID, //also check if account currency and stop order currency match
                IsInternalAccount = 1,
                AddedBy = premiumCollectionConfig.AddedBy,
                AddedOn=DateTime.Now
            };
            int memberBankAccountID = _memberBankAccountRepository.AddMemberBankAccount(memberBankAccount); //if record already exists, this returns the id of the record
            if (memberBankAccountID == 0)
            {
                throw new Exception("Bank account could not be created!");
            }
            string storedProcedure;
            //switch (premiumCollectionConfig.PaymentMethodID)
            //{
            //    case 1: storedProcedure = "Debit_" + premiumCollectionConfig.StopOrderCode.Replace(" ",""); break;
            //    case 2: storedProcedure = "StopOrder_" + premiumCollectionConfig.StopOrderCode.Replace(" ", ""); break;
            //    default: break;
            //}
            if (premiumCollectionConfig.RecordsFormat == "Custom")
            {
                storedProcedure = "StopOrder_" + paymentProviderID;
            }
            else
            {
                storedProcedure = "StopOrder_Ordinary";
            }
            PremiumCollectionConfigHeader premiumCollectionConfigHeader = new()
            {
                PaymentMethodID = premiumCollectionConfig.PaymentMethodID,
                PaymentProviderID = paymentProviderID,
                InternalBankAccountID = memberBankAccountID, 
                CurrencyID = premiumCollectionConfig.CurrencyID,
                StopOrderName = premiumCollectionConfig.StopOrderName, 
                StopOrderCode = premiumCollectionConfig.StopOrderCode.Replace(" ", ""), 
                SalaryDisbursementdate= premiumCollectionConfig.SalaryDisbursementDate, 
                Billingdate = premiumCollectionConfig.BillingDate,
                CollectionCommissionRate= premiumCollectionConfig.CollectionCommissionRate,
                Net= premiumCollectionConfig.Net,
                StoredProcedureName = storedProcedure,
                Aggregated = premiumCollectionConfig.Aggregated,
                AddedBy = premiumCollectionConfig.AddedBy
            };          
            _premiumCollectionConfigHeaderRepository.Create(premiumCollectionConfigHeader);
            return 1; //1 premiumcollectionconfig record added
        }
        public IActionResult OnGetLoadContacts(Guid id)
        {
            // Use the id parameter in your logic to fetch data
            var data = _memberContactRepository.GetAllMemberContacts(id);
            return new JsonResult(data);
        }
    }
}
