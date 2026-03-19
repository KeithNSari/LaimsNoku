using System.Data;
using LAIMS.Interfaces.Commissions;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Commissions;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
namespace LAIMS.Areas.SalesCommission.Pages
{
	[Authorize(Roles = "Commission Indexing")]
	public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyCommissionRepository _policyCommissionRepository;
        private readonly IIntermediaryRepository _intermediaryRepository;
        private readonly ICurrencyRepository _currencyRepository;
        public List<SelectListItem> Currencies = new List<SelectListItem>();
        public IndexModel(UserManager<ApplicationUser> userManager,ICurrencyRepository currencyRepository,
            IPolicyCommissionRepository policyCommissionRepository, IIntermediaryRepository intermediaryRepository)
        {
            _userManager = userManager; 
            _currencyRepository = currencyRepository;
            _policyCommissionRepository = policyCommissionRepository;
            _intermediaryRepository = intermediaryRepository;
        }
        public int SearchCriteria { get; set; }
        public DataTable CommissionsDT;
        public string SearchPageUrl;
        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        [BindProperty ]
        public int Year { get; set; }
        [BindProperty]
        public int StartMonth { get; set; }
        [BindProperty]
        public int EndMonth { get; set; }
        [BindProperty]
        public string? AgentCode { get; set; }
        [BindProperty]
        public int CurrencyID { get; set; }
        [BindProperty]
        public CommissionSearchHeader CommissionSearchHeader { get; set; }
        [BindProperty]
        public int FilterLevel { get; set; }
        public void OnGet()
        {           
            try
            {
                FilterLevel = 0;
                Year = DateTime.Now.Year;
                ReturnUrl = Request.Path + Request.QueryString;
                LoadCurrencies();
                CommissionsDT = _policyCommissionRepository.GetLatest();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
        }
        private void LoadCurrencies()
        {
            foreach (Currency currency in _currencyRepository.GetAllCurrencies())
            {
                Currencies.Add(new SelectListItem
                {
                    Value = currency.ID.ToString(),
                    Text = currency.CurrencyName
                });
            }
        }
        public void OnPost()
        {
            try
            { 
                ReturnUrl = Request.Path + Request.QueryString;
                if (EndMonth < StartMonth) throw new Exception("End month should be less than start month");
                if (!string.IsNullOrEmpty(AgentCode))
                {
                    FilterLevel = 1; //single agent
                    int agentID = _intermediaryRepository.GetIntermediaryID(AgentCode);
                    if (agentID == 0) throw new Exception("Invalid agent code");
                    CommissionsDT = _policyCommissionRepository.Search(agentID,CurrencyID, StartMonth, EndMonth, Year);
                    CommissionSearchHeader= _policyCommissionRepository.GetSearchHeader(agentID, CurrencyID, StartMonth, EndMonth, Year);
                }
                else
                {
                    FilterLevel = 2; //all agents
                    CommissionSearchHeader = _policyCommissionRepository.GetSearchHeader(CurrencyID, StartMonth, EndMonth, Year);
                    CommissionsDT = _policyCommissionRepository.Search(CurrencyID, StartMonth, EndMonth, Year);
                }               
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
            finally
            {
                LoadCurrencies();
            }
        }
    }
}
