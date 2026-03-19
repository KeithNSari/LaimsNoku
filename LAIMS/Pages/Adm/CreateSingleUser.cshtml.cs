using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using LAIMS.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using LAIMS.Models.Membership;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Utilities;
using LAIMS.Models.Security;

namespace LAIMS.Pages.Adm
{
    [Authorize(Roles = "Admin")]
    public class CreateSingleUserModel : PageModel
    {        
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        //private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<CreateSingleUserModel> _logger;
        //private readonly IEmailSender _emailSender;
        private readonly IUserList _userList;
        private readonly IEmploymentRepository _employmentRepository;
        private readonly ICustomValidator _customValidator; 
        public List<SelectListItem> Designations = new List<SelectListItem>();
        public DataTable RolesDT { get; set; }
        public DataTable UsersDT;
        [BindProperty]
        public Designation Designation { get; set; }
        public CreateSingleUserModel(
           UserManager<ApplicationUser> userManager,
           IUserStore<ApplicationUser> userStore,
           SignInManager<ApplicationUser> signInManager,
           ILogger<CreateSingleUserModel> logger,
           //IEmailSender emailSender,
           IUserList userList,
           IEmploymentRepository employmentRepository,
           ICustomValidator customValidator)
        {
            _userManager = userManager;
            _userStore = userStore;
            //_emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            //_emailSender = emailSender;
            _userList = userList;
            _employmentRepository = employmentRepository;
            _customValidator = customValidator;
        }
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Role")]
            public string UserRole { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "User Name")]
            public string Username { get; set; }
            [DataType(DataType.Text)]
            [Display(Name = "First Names")]
            public string FirstNames { get; set; }
            [DataType(DataType.Text)]
            [Display(Name = "Surname")]
            public string Surname { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }
        private void LoadDesignations()
        {
            foreach (Designation designation in _employmentRepository.GetDesignations())
            {
                Designations.Add(new SelectListItem
                {
                    Value = designation.ID.ToString(),
                    Text = designation.DesignationName
                });
            }
        }
        public void OnGet()
        {
            UsersDT = _userList.GetUsers();
            RolesDT =_userList.GetRoles();
            LoadDesignations();
        }

        public async Task<IActionResult> OnPostAsync(string?[] RoleIDS, string returnUrl = null)
        {
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                _customValidator.ValidatePassword(Input.Password);
                string AddedBy = _userManager.GetUserId(User).ToString();                
                var user = new  ApplicationUser();
                await _userStore.SetUserNameAsync(user, Input.Username, CancellationToken.None);
                user.LockoutEnabled = true;
                user.MustChangePassword = true;
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    //user.MustChangePassword = true;
                    //await _userManager.UpdateAsync(user);
                    _userList.UpdateUserPersonalNames(Input.FirstNames, Input.Surname, user.Id, Designation.ID, AddedBy);
                    _userList.UpdateUserDesignationRoles(Designation.ID, user.Id);
                    if (RoleIDS != null)
                    {
                        foreach (string role in RoleIDS)
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description); // to be adjusted
                }
                UsersDT = _userList.GetUsers();
                RolesDT = _userList.GetRoles();
                LoadDesignations();
                //}
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }                        
            return Redirect("CreateSingleUser");
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        } 
    }
}
