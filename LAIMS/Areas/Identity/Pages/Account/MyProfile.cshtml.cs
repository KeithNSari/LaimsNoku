using LAIMS.Interfaces;
using LAIMS.Models.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Utilities;
using LAIMS.Models.Security;

namespace LAIMS.Areas.Identity.Pages.Account
{
    public class MyProfileModel : PageModel
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
        public DataTable RolesDT { get; set; }
        public DataTable PoliciesDT { get; set; }
        public bool MustChangePassword { get; set; } = false;
        public MyProfileModel(UserManager<ApplicationUser> userManager, IUserList userList,
            IPolicyTypeRepository policyTypeRepository, ICustomValidator customValidator)
        {
            _userManager = userManager;
            _userList = userList;
            _policyTypeRepository = policyTypeRepository;
            _customValidator = customValidator;
        }

        [BindProperty]
        public UserDetails userDetails { get; set; }
        public async Task OnGetAsync()
        {
            string userId = _userManager.GetUserId(User).ToString();
            userDetails = _userList.GetUserDetailsById(userId);
            RolesDT = _userList.GetUserRoleList(userId);
            PoliciesDT = _policyTypeRepository.GetDesignationPolicyTypes(userId);
            var user = await _userManager.FindByNameAsync(userDetails.Username);
            if (user.MustChangePassword)
            {
                MustChangePassword = true;
            }
        } 
        public async Task<IActionResult> OnPostReset()
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                _customValidator.ValidatePassword(Password);
                var user = await _userManager.FindByNameAsync(userDetails.Username);
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
                user.MustChangePassword = false;
                await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
            return Redirect("TranUpdate?Success=true");
        } 
    }
}
