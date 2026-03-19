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
    public class DetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;
        public DataTable ProductsDT;
        
        public DetailsModel(UserManager<ApplicationUser> userManager, IProductRepository productRepository)
        {
            _userManager = userManager;
            _productRepository = productRepository;
        } 
        [BindProperty]
        public Guid ProductID { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public IActionResult OnGet(Guid id)
        {            
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                if (_productRepository.CheckExistence(id) == 0)
                {
                    throw new Exception("An active product is required for this action!");
                }
                ProductID = id;
                ProductsDT = _productRepository.GetByID(id);
                return Page();
            }
            catch (Exception ex)
            {
                // ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }

        }
    }
}
