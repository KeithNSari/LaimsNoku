using LAIMS.Interfaces.Claims;
using LAIMS.Models.Claims;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.Claims.Pages
{
    [Authorize(Roles = "Claims Initiator")]
    public class DisabilityPremiumWaiversModel : PageModel
    {
        private readonly IPremiumWaiverRepository _premiumWaiverRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DisabilityPremiumWaiversModel(IPremiumWaiverRepository premiumWaiverRepository, UserManager<ApplicationUser> userManager)
        {
            _premiumWaiverRepository = premiumWaiverRepository;
            _userManager = userManager;
        }

        [BindProperty]
        public DisabilityPremiumWaiver DisabilityPremiumWaiver { get; set; } = new();
        public DataTable WaiversDT { get; set; } = new();

        public void OnGet()
        {
            WaiversDT = _premiumWaiverRepository.GetDisabilityPremiumWaivers();
        }

        public IActionResult OnPost()
        {
            DisabilityPremiumWaiver.RequestID = Guid.NewGuid();
            DisabilityPremiumWaiver.AddedBy = _userManager.GetUserId(User) ?? string.Empty;
            _premiumWaiverRepository.AddDisabilityPremiumWaiver(DisabilityPremiumWaiver);
            return RedirectToPage();
        }
    }
}
