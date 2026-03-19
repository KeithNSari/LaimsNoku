using LAIMS.Interfaces.Banking;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using LAIMS.Models.Banking;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Transactions;
using System.Text.RegularExpressions;
using LAIMS.Interfaces.Membership;
using LAIMS.Repositories.Membership;
using Microsoft.EntityFrameworkCore;
using System.Text;
using CsvHelper;
using LAIMS.Models.Security;

namespace LAIMS.Areas.PaymentCollection.Pages
{
    public class CreateBillModel : PageModel
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymentProviderRepository _paymentProviderRepository;
        private readonly IMemberBankAccountRepository _memberBankAccountRepository;
        private readonly IBillingHeaderRepository _billingHeaderRepository;
        private readonly IBilledPremiumRepository _billedPremiumRepository; 
        private readonly IBankRepository _bankRepository;
        private readonly IMemberRepository _memberRepository;
        public List<SelectListItem> DebitOrderProviderList = new List<SelectListItem>();
        public List<PaymentProvider> DebitOrderProviders { get; set; }
        public DataTable PoliciesDT;
        public string SearchPageUrl;

        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        [BindProperty]
        public int PaymentMethod { get; set; }
        [BindProperty]
        public DateTime BillDate { get; set; }
        [BindProperty]
        public string BranchCode { get; set; }
        [BindProperty]
        public string PremiumPayerAccount { get; set; }
        [BindProperty]
        public string PremiumPayerAccountConfirm { get; set; }
        [BindProperty]
        public int PaymentProviderID { get; set; }

        [BindProperty ]
        public Guid PolicyID { get; set; }
        [BindProperty ]
        public string PolicyNo { get; set; }
        [BindProperty]
        public int MemberID { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        public CreateBillModel(UserManager<ApplicationUser> userManager, 
            IPolicyRepository policyRepository, 
            IWebHostEnvironment webHostEnvironment, IMemberRepository memberRepository,
            IMemberBankAccountRepository memberBankAccountRepository, IBankRepository bankRepository,
            IPaymentProviderRepository paymentProviderRepository, IBillingHeaderRepository billingHeaderRepository,
            IBilledPremiumRepository billedPremiumRepository)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _webHostEnvironment = webHostEnvironment;
            _memberBankAccountRepository = memberBankAccountRepository;
            _paymentProviderRepository = paymentProviderRepository;
            _billingHeaderRepository = billingHeaderRepository;
            _billedPremiumRepository=billedPremiumRepository;
            _bankRepository = bankRepository;
            _memberRepository = memberRepository;
        }
        public void OnGet()
        {
            BillDate = DateTime.Today; 
        }
        public void OnGetSearch(string? search)
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                SearchTerm = search;
                SearchPageUrl = "CreateBill";
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    BillDate = DateTime.Today;
                    PoliciesDT = _policyRepository.PoliciesSearchPostApproved(SearchTerm);//search only approved and beyond
                    ResultsCount = PoliciesDT.Rows.Count;
                    DebitOrderProviders = _paymentProviderRepository.GetPaymentProviders(1);
                    LoadDebitOrderProvidersSelectList();
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }           
        }
        private void LoadDebitOrderProvidersSelectList()
        {
            foreach (PaymentProvider paymentProvider in DebitOrderProviders)
            {
                DebitOrderProviderList.Add(new SelectListItem
                {
                    Value = paymentProvider.MemberID.ToString(),
                    Text = paymentProvider.ProviderName
                });
            }
        }
        public IActionResult OnPost(string PolicyNo)
        {
            using (TransactionScope TS = new TransactionScope())
            {
                try
                {
                    ReturnUrl = Request.Path + Request.QueryString;
                    if (BillDate.Date < DateTime.Today.Date)
                    {
                        throw new Exception("Due date cannot be in the past");
                    }
                    DebitOrderProviders = _paymentProviderRepository.GetPaymentProviders(1);
                    LoadDebitOrderProvidersSelectList();
                    int batchID = Convert.ToInt32(DateTime.Today.ToString("yyyyMMdd"));
                    BillingHeader billingHeader = new()
                    {
                        BatchID = batchID,
                        PolicyID = PolicyID,
                        DateDue = BillDate,
                        PaymentMethodID = PaymentMethod
                    };
                    if (PaymentMethod == 3)
                    {
                        billingHeader.PaymentProviderID = 0;
                    }
                    else if (PaymentMethod == 1)
                    {
                        if ((PremiumPayerAccount == null) || (PremiumPayerAccountConfirm == null))
                        {
                            throw new Exception("Please enter all required details! Account/ Confirm Account cannot be empty for debit orders");
                        }
                        if (PremiumPayerAccount != PremiumPayerAccountConfirm)
                        {
                            throw new Exception("Account and Confirm must match exactly!");
                        }
                        string AddedBy= _userManager.GetUserId(User).ToString();
                        MemberBankAccount memberBankAccount = new()
                        {
                            BankAccountNo = PremiumPayerAccount,
                            BranchCode = BranchCode,
                            BankID = PaymentProviderID,
                            AddedBy=AddedBy,
                            AddedOn=DateTime.Now,
                            MemberID= MemberID
                        };
                        BankAccountFormat bankAccountFormat = _bankRepository.GetBankAccountFormat(memberBankAccount.BankID);
                        Regex accountNoFormat = new Regex(bankAccountFormat.BankAccountNoFormat);
                        if (!accountNoFormat.IsMatch(memberBankAccount.BranchCode + memberBankAccount.BankAccountNo))
                        {
                            throw new Exception("Invalid account number format! The expected format for this provider is: " + bankAccountFormat.FormatDescription);
                        } 
                        int premiumPayerAccountID = _memberBankAccountRepository.AddMemberBankAccount(memberBankAccount);
                        if (premiumPayerAccountID == 0)
                        {
                            throw new Exception("Bank account could not be created!");
                        }
                        billingHeader.PaymentProviderID = PaymentProviderID;
                        billingHeader.PremiumPayerAccountID = premiumPayerAccountID;
                    }
                    int billedPremiumID = _billingHeaderRepository.AddAdhocBill(billingHeader);
                    if (billedPremiumID > 0)
                    {
                        _billingHeaderRepository.AddAdhocBillHeader(billedPremiumID);
                    }
                    else
                    {
                        throw new Exception("A current bill already exists for this policy. To create a new bill, please archive the existing one!");
                    }
                    TS.Complete();
                    TempData["PostbackSuccess"] = true;
                    return Redirect("BillingHistory?handler=search&search=" + PolicyNo);
                }
                catch (Exception ex)
                {
                    return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
                }
            }
        }
        public IActionResult OnGetLoadHistory(string PolicyNo)
        {
            // Use the id parameter in your logic to fetch data
            var data = _billedPremiumRepository.GetPolicyBilledPremiums(PolicyNo);
            return new JsonResult(data);
        }
        public IActionResult OnGetDownLoadCSV()
        {
            // Retrieve data from SQL table
            List<BillingSummary> data = _billedPremiumRepository.GetPolicyBilledPremiums("G24-110-G-512");
            // Convert data to CSV format
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(streamWriter, System.Globalization.CultureInfo.CurrentCulture))
            {
                csvWriter.WriteRecords(data);
                streamWriter.Flush();

                // Return CSV file as a download
                return File(memoryStream.ToArray(), "text/csv", "data.csv");
            }
        }
    }
}
