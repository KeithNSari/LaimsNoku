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
    public class DeathPremiumWaiversModel : PageModel
    {
        private readonly IPremiumWaiverRepository _premiumWaiverRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeathPremiumWaiversModel(IPremiumWaiverRepository premiumWaiverRepository, UserManager<ApplicationUser> userManager)
        {
            _premiumWaiverRepository = premiumWaiverRepository;
            _userManager = userManager;
        }

        [BindProperty]
        public DeathPremiumWaiver DeathPremiumWaiver { get; set; } = new();
        public DataTable WaiversDT { get; set; } = new();

        public void OnGet()
        {
            WaiversDT = _premiumWaiverRepository.GetDeathPremiumWaivers();
        }

        public IActionResult OnPost()
        {
            DeathPremiumWaiver.RequestID = Guid.NewGuid();
            DeathPremiumWaiver.AddedBy = _userManager.GetUserId(User) ?? string.Empty;
            _premiumWaiverRepository.AddDeathPremiumWaiver(DeathPremiumWaiver);
            return RedirectToPage();
        }
    }
}
