using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.PaymentCollection.Pages
{
    public class BatchSupenseIndexModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBilledPremiumRepository _billedPremiumRepository;
        private readonly ISuspenseProcessing _suspenseProcessing;
        private readonly IProcessPayments _processPayments;
        private readonly IBillingMessageRepository _billingMessageRepository;
        public DataTable AllocationSuspenseDT;
        public DataTable PolicySuspenseDT;
        public DataTable SystemSuspenseDT;
        public DataTable UnpaidDT;
        public DataTable MessagesDT { get; set; }
        [BindProperty]
        public BillingBatch BillingBatch { get; set; }
        public BatchSupenseIndexModel(UserManager<ApplicationUser> userManager, 
            IWebHostEnvironment webHostEnvironment, IBilledPremiumRepository billedPremiumRepository
            ,ISuspenseProcessing suspenseProcessing,IProcessPayments processPayments,
            IBillingMessageRepository billingMessageRepository)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _billedPremiumRepository = billedPremiumRepository;
            _suspenseProcessing = suspenseProcessing;
            _processPayments = processPayments;
            _billingMessageRepository = billingMessageRepository;
        }
        public void OnGet(long batchid)
        {
            AllocationSuspenseDT = _billedPremiumRepository.GetAllocationSuspenseEntries(batchid);
            PolicySuspenseDT = _suspenseProcessing.GetPolicySuspenseData(batchid);
            SystemSuspenseDT = _suspenseProcessing.GetSystemSuspenseData(batchid);
            BillingBatch = _processPayments.GetBillingBatchById(batchid);
            UnpaidDT = _billedPremiumRepository.GetUnPaid(batchid);
            MessagesDT = _billingMessageRepository.GetBillingMessages (batchid);
            //AllocationSuspenseBalance = _processPayments.GetBatchAllocationSuspenseBalance(batchid);
            //PolicySuspenseBalance = _suspenseProcessing.GetBatchSuspenseBalance(batchid, 2);
            //SystemSuspenseBalance = _suspenseProcessing.GetBatchSuspenseBalance(batchid, 3);
        }
    }
}
