using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Security;
using LAIMS.Repositories.Lifeproducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
namespace LAIMS.Areas.PolicySetUps.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;
        public DataTable ProductsDT;
        public CreateModel(UserManager<ApplicationUser> userManager, IProductRepository productRepository)
        {
            _userManager = userManager;
            _productRepository = productRepository;
        }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public Product Product { get; set; }

        public void OnGet()
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                ProductsDT = _productRepository.Get();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
            
        }

        public IActionResult OnPost()
        {           
            try
            {
                //if (ModelState.IsValid)
                //{
                Product.ID = Guid.NewGuid();
                Product.AddedBy = _userManager.GetUserId(User).ToString();  // "15feae26-41ab-468f-ad34-e8342bca7ad3";
                if (_productRepository.CheckExistence(Product.ProductName) == 0)
                {
                    _productRepository.InsertProduct(Product);
                }
                //}
                return Redirect("Create");
            }
            catch (Exception ex)
            {
                //ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message,returnUrl=ReturnUrl });
            } 
        } 
        public IActionResult OnPostRemove(Guid ID, Guid EntryID)
        {           
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                DateTime deletedOn = DateTime.Now;
                _productRepository.DeleteProduct(EntryID, addedBy, deletedOn);
                return RedirectToPage("Create");
            }
            catch (Exception ex)
            {
                //ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            } 
        }
    }
}
