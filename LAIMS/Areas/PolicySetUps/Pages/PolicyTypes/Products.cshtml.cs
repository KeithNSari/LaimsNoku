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
    public class ProductsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPolicyTypesLinesRepository _policyTypesLinesRepository;
        public List<SelectListItem> MyProducts = new List<SelectListItem>();
        public DataTable ProductsDT;
        public ProductsModel(UserManager<ApplicationUser> userManager, IPolicyTypeRepository policyTypeRepository, IPolicyTypesLinesRepository policyTypesLinesRepository, ICurrencyRepository currencyRepository, IProductRepository productRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
            _policyTypesLinesRepository = policyTypesLinesRepository;
            _productRepository = productRepository;
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
                    ReturnUrl = Request.Path + Request.QueryString;
                    MyPolicyType = policytypes;
                    LoadProductsSelectList();
                    ProductsDT = _policyTypesLinesRepository.GetProducts(id);
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }            
            return Page();
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
        public IActionResult OnPost(Guid id)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    
                //    return Page();
                //}
                if ((MyPolicyTypeLines.Main ==0) && (_policyTypesLinesRepository.CheckExistenceOfMain(id) == 0))
                {
                    throw new Exception("Please add a main product first!");
                }
                if ((MyPolicyTypeLines.Optional == 1) && (MyPolicyTypeLines.Main == 1)){
                    throw new Exception("A main product cannot be optional!");
                }
                if (_policyTypesLinesRepository.CheckExistence(id, MyPolicyTypeLines.ProductID)>0)
                {
                    throw new Exception("This product has already been added!");
                }
                MyPolicyTypeLines.ID = Guid.NewGuid();
                MyPolicyTypeLines.HeaderID = id;
                MyPolicyTypeLines.Current = 1;
                MyPolicyTypeLines.AddedBy = _userManager.GetUserId(User).ToString();
                _policyTypesLinesRepository.InsertPolicyTypeLine(MyPolicyTypeLines);
                ProductsDT = _policyTypesLinesRepository.GetProducts(id);
                MyPolicyType = _policyTypeRepository.GetPolicyType(id);
                LoadProductsSelectList();
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostRemove(Guid id, Guid productId)
        {
            try
            {
                if (_policyTypesLinesRepository.CheckExistenceOfMain(id) == 1)
                {
                    throw new Exception("Please add another main product first, before removing the current one!");
                }
                string addedBy = _userManager.GetUserId(User).ToString();
                DateTime addedOn = DateTime.Now;
                _policyTypesLinesRepository.Archive(id, addedBy, addedOn);
                return Redirect(ReturnUrl);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
}
