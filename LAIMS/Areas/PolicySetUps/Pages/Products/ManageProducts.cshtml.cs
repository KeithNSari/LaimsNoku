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
    public class ManageProductsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;
        public DataTable ProductsDT;
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        public ManageProductsModel(UserManager<ApplicationUser> userManager, IProductRepository productRepository)
        {
            _userManager = userManager;
            _productRepository = productRepository;
        }
        
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
        public IActionResult OnPostRemove(Guid EntryID)
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
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            } 
        }
    }
}
