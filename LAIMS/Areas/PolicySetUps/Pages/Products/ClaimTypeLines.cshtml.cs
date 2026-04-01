using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.Claims;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.PolicySetUps.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class ClaimTypeLinesModel : PageModel
    {
        private readonly IProductRepository _productRepository;
        private readonly IPremiumWaiverRepository _premiumWaiverRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClaimTypeLinesModel(IProductRepository productRepository, IPremiumWaiverRepository premiumWaiverRepository, UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _premiumWaiverRepository = premiumWaiverRepository;
            _userManager = userManager;
        }

        public List<SelectListItem> ProductsList { get; set; } = new();
        public List<SelectListItem> ClaimTypesList { get; set; } = new();
        public DataTable ClaimTypeLinesDT { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public Guid ProductID { get; set; }

        [BindProperty]
        public ClaimTypeLine ClaimTypeLine { get; set; } = new();

        public void OnGet()
        {
            LoadPage();
        }

        public IActionResult OnPostAdd()
        {
            try
            {
                ClaimTypeLine.ProductID = ProductID;
                ClaimTypeLine.AddedBy = _userManager.GetUserId(User) ?? string.Empty;
                _premiumWaiverRepository.AddClaimTypeLine(ClaimTypeLine);
                return RedirectToPage(new { ProductID });
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                LoadPage();
                return Page();
            }
        }

        public IActionResult OnPostArchive(int id)
        {
            _premiumWaiverRepository.ArchiveClaimTypeLine(id, _userManager.GetUserId(User) ?? string.Empty);
            return RedirectToPage(new { ProductID });
        }

        private void LoadPage()
        {
            foreach (Product product in _productRepository.GetAllProducts())
            {
                ProductsList.Add(new SelectListItem { Value = product.ID.ToString(), Text = product.ProductName });
            }

            DataTable claimTypes = _premiumWaiverRepository.GetClaimTypes();
            foreach (DataRow row in claimTypes.Rows)
            {
                ClaimTypesList.Add(new SelectListItem
                {
                    Value = row["ID"].ToString(),
                    Text = row["ClaimType"].ToString()
                });
            }

            if (ProductID != Guid.Empty)
            {
                ClaimTypeLinesDT = _premiumWaiverRepository.GetClaimTypeLines(ProductID);
            }
        }
    }
}
