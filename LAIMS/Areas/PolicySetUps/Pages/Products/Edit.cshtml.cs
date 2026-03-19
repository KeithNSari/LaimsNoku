using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.PolicySetUps.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;
        public EditModel (UserManager<ApplicationUser> userManager, IProductRepository productRepository)
        {
            _userManager = userManager;
            _productRepository = productRepository;
        }
        [BindProperty]
        public Product Product { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public void OnGet(Guid id)
        {           
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                Product = _productRepository.GetProduct(id);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
        }
        public IActionResult OnPost(Guid id)
        {
            try
            {
                //if (ModelState.IsValid)
                //{
                Product.ID = id;
                if (_productRepository.CheckExistence(Product.ID) > 0)
                {
                    if (_productRepository.CheckOtherExistence(Product.ProductName, id) > 0)
                    {
                        throw new Exception("An active product with the same name already exists!");
                    }
                    Product.AddedBy = _userManager.GetUserId(User).ToString();
                    _productRepository.UpdateProduct(Product);
                }
                //}
                return Redirect("Details?id=" + id.ToString());
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            } ;           
        }
    }
}
