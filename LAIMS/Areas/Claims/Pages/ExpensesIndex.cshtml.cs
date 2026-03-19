using Azure.Core;
using LAIMS.Interfaces.Claims;
using LAIMS.Models.Claims;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Policies;
using LAIMS.Models.Security;
using LAIMS.Models.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;


namespace LAIMS.Areas.Claims.Pages
{
    public class ExpensesIndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyClaimRepository _policyClaimRepository;
        [BindProperty]
        public Policy Policy { get; set; }

        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid ProposerID { get; set; }
        [BindProperty]
        public Guid RequestID { get; set; }
        [BindProperty]
        public List<ClaimExpense> ClaimExpenses { get; set; }
        [BindProperty]
        public Dictionary<int, int> SelectedOptions { get; set; }
        public decimal OverallTotal => ClaimExpenses.Where(e => e.IsSelected).Sum(e => e.Price);
        public DataTable UnpaidDT;
        public ExpensesIndexModel(UserManager<ApplicationUser> userManager,
           IPolicyClaimRepository policyClaimRepository)
        {
            _userManager = userManager;
            _policyClaimRepository = policyClaimRepository;
        }
        public void OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            ProposerID = id;
            PolicyTypeID = policyTypeid;
            PolicyID = policyid;
            RequestID = reqid;
            ClaimExpenses = _policyClaimRepository.GetClaimExpensePriceList(RequestID);
            UnpaidDT = _policyClaimRepository.GetUnpaidPremiums(policyid);
        }

        public IActionResult OnPost()
        {
            string AddedBy = _userManager.GetUserId(User).ToString();
            _policyClaimRepository.ArchiveExpense(RequestID, AddedBy);
            foreach (var (opid, sop) in SelectedOptions )
            {
                _policyClaimRepository.AddClaimExpense(RequestID, sop, AddedBy);
            } 
            return RedirectToPage(); 
        } 
    }

   
} 
