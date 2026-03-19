using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Pages.Lifeproducts.Products
{
    public class ProductModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IProductRepository _productRepository;
        public DataTable ProductsDT;
        public ProductModel(UserManager<IdentityUser> userManager, IProductRepository productRepository)
        {
            _userManager = userManager;
            _productRepository = productRepository;
        }

        [BindProperty]
        public Product Product { get; set; }

        public void OnGet()
        {
            ProductsDT = _productRepository.Get();
        }

        public IActionResult OnPost()
        {
            //if (ModelState.IsValid)
            //{
            Product.ID = Guid.NewGuid();
            Product.AddedBy = _userManager.GetUserId(User).ToString();  // "15feae26-41ab-468f-ad34-e8342bca7ad3";
            if (_productRepository.CheckExistence(Product.ProductName) == 0)
            {
                _productRepository.InsertProduct(Product);
                ProductsDT = _productRepository.Get();
            }
            //}
            return Page();
        }

        public IActionResult OnPostUpdate()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _productRepository.UpdateProduct(Product);

            return RedirectToPage("/Index"); // Redirect to the product list page
        }
    }
}
