using LAIMS.Interfaces;
using LAIMS.Models.Utilities;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using LAIMS.Interfaces.Lifeproducts;
using Microsoft.AspNetCore.Authorization;
using LAIMS.Interfaces.Utilities;


namespace LAIMS.Pages.Adm
{
    [Authorize(Roles = "Admin")]
    public class ResetUserModel : PageModel
    {         
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserList _userList;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly ICustomValidator _customValidator;
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [BindProperty]
        public string Password { get; set; } 
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [BindProperty]
        public string ConfirmPassword { get; set; }
        public bool UserFound { get; set; }
        public DataTable RolesDT { get; set; }
        public DataTable PoliciesDT { get; set; }
        public ResetUserModel(UserManager<ApplicationUser> userManager, IUserList userList,
            IPolicyTypeRepository policyTypeRepository, ICustomValidator customValidator)
        {
            _userManager = userManager;
            _userList = userList;
            _policyTypeRepository = policyTypeRepository;
            _customValidator = customValidator;
        }

        [BindProperty]
        public UserDetails userDetails { get; set; }
        public async Task OnGetAsync(string? userId)
        {
            UserFound = false;
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    UserFound = true;
                    userDetails = _userList.GetUserDetailsById(user.Id);
                    RolesDT = _userList.GetUserRoleList(userId);
                    PoliciesDT = _policyTypeRepository.GetDesignationPolicyTypes(userId);
                }
            }
        }       
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.FindByNameAsync(userDetails.Username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return NotFound();
            }
            return Redirect("ResetUser?userid=" + user.Id);
        }
        public async Task<IActionResult> OnPostReset()
        {
            string ReturnUrl=Request.Path + Request.QueryString;
            try
            {
                _customValidator.ValidatePassword(Password);
                var user = (ApplicationUser)await _userManager.FindByNameAsync(userDetails.Username);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return NotFound();
                }
                if (ConfirmPassword != Password)
                {
                    throw new Exception("Password and Confirm Password entries should be identical!");
                }
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, resetToken, Password);
                user.MustChangePassword = true;
                await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }           
            return Redirect("TranUpdate?Success=true");
        }
        public async Task<IActionResult> OnPostUnlock()
        {
            var user = await _userManager.FindByNameAsync(userDetails.Username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return NotFound();
            }
            // Unlock the user by setting the LockoutEnd to a past date
            var unlockResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);

            if (unlockResult.Succeeded)
            { 
                await _userManager.ResetAccessFailedCountAsync(user);

                return Redirect("TranUpdate?Success=true");
            }

            return BadRequest("Failed to unlock user");
        }
        public async Task<IActionResult> OnPostDelete ()
        {
            var user = await _userManager.FindByNameAsync(userDetails.Username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
            return Redirect("TranUpdate?Success=true");
        }
    }
}
