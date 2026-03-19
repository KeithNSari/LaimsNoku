using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Security;
using LAIMS.Repositories.Lifeproducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LAIMS.Areas.PolicySetUps.Pages.PolicyTypes
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPolicyTypesLinesRepository _policyTypesLinesRepository;
        private readonly IPolicyTypeDocumentsRepository _policyTypeDocumentsRepository;
        private readonly IPolicyTypeLinesBenefitsRepository _policyTypeLinesBenefitsRepository;
        private readonly IPolicyTypeLinesBenefitsDocumentsRepository _policyTypeLinesBenefitsDocumentsRepository;
        public List<SelectListItem> MyProducts = new List<SelectListItem>();
        public DataTable ProductsDT;
        public DataTable DocumentsDT;
        public DataTable ClaimDocumentsDT;
        public DataTable BenefitsDT;
        public DetailsModel(UserManager<ApplicationUser> userManager, IPolicyTypeRepository policyTypeRepository, IPolicyTypesLinesRepository policyTypesLinesRepository,  ICurrencyRepository currencyRepository, IProductRepository productRepository, IPolicyTypeDocumentsRepository policyTypeDocumentsRepository, IPolicyTypeLinesBenefitsRepository policyTypeLinesBenefitsRepository, IPolicyTypeLinesBenefitsDocumentsRepository policyTypeLinesBenefitsDocumentsRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
            _policyTypesLinesRepository = policyTypesLinesRepository;
            _productRepository = productRepository;
            _policyTypeDocumentsRepository = policyTypeDocumentsRepository;
            _policyTypeLinesBenefitsRepository = policyTypeLinesBenefitsRepository;
            _policyTypeLinesBenefitsDocumentsRepository = policyTypeLinesBenefitsDocumentsRepository;
        }
        [BindProperty]
        public PolicyType MyPolicyType { get; set; } = default!;
        [BindProperty]
        public PolicyTypesLines MyPolicyTypeLines { get; set; } = default!;
        public IActionResult OnGet(Guid id)
        {
            try
            {
                var policytypes = _policyTypeRepository.GetPolicyType(id);
                if (policytypes == null)
                {
                    return NotFound();
                }
                else
                {
                    MyPolicyType = policytypes;
                    LoadProductsSelectList();
                    ProductsDT = _policyTypesLinesRepository.GetProducts(id);
                    DocumentsDT = _policyTypeDocumentsRepository.GetDocuments(id);
                    BenefitsDT = _policyTypeLinesBenefitsRepository.GetBenefits(id);
                    ClaimDocumentsDT = _policyTypeLinesBenefitsDocumentsRepository.GetDocuments(id);
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }          
            return Page();
        }
        public IActionResult OnPost(Guid id)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    LoadProductsSelectList();
                //    return Page();
                //}
                ReturnUrl = Request.Path + Request.QueryString;
                MyPolicyTypeLines.ID = Guid.NewGuid();
                MyPolicyTypeLines.HeaderID = id;
                MyPolicyTypeLines.Current = 1;
                MyPolicyTypeLines.AddedBy = _userManager.GetUserId(User).ToString();
                _policyTypesLinesRepository.InsertPolicyTypeLine(MyPolicyTypeLines);
                ProductsDT = _policyTypesLinesRepository.GetProducts(id);
                MyPolicyType = _policyTypeRepository.GetPolicyType(id);
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }            
        }
        public IActionResult OnPostArchive(Guid id)
        {
            try
            {
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        private void LoadProductsSelectList()
        {
            foreach (Product product in _productRepository.GetAllProducts())
            {
                MyProducts.Add(new SelectListItem
                {
                    Value = product.ID.ToString(),
                    Text = product.ProductName
                });
            }
        }
    }
}
