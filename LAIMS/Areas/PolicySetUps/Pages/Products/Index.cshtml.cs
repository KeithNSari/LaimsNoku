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
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;
        public DataTable ProductsDT;
        public IndexModel(UserManager<ApplicationUser> userManager, IProductRepository productRepository)
        {
            _userManager = userManager;
            _productRepository = productRepository;
        }
        public void OnGet()
        {
            try
            {

				ProductsDT = _productRepository.Get(); 
			}
			catch (Exception ex)
			{
				ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
			}

		}
	}
} 
