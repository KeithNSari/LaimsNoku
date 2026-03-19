using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using LAIMS.Interfaces;
using Microsoft.AspNetCore.Authorization;
using LAIMS.Models.Membership;
using LAIMS.Utilities;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Utilities;
using LAIMS.Models.Security;

namespace LAIMS.Pages.Adm
{
    [Authorize(Roles = "Admin")]
    public class UploadUsersModel : PageModel
    {
        public DataTable UsersDT;
        private readonly IWebHostEnvironment _environment;
        private readonly IUploadData _userUpload;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<CreateSingleUserModel> _logger;
       // private readonly IEmailSender _emailSender;
        private readonly IUserList _userList;
        private readonly IEmploymentRepository _employmentRepository;
        private readonly ICustomValidator _customValidator;
        public UploadUsersModel(IWebHostEnvironment environment,
            IUploadData userUpload,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<CreateSingleUserModel> logger,
            ICustomValidator customValidator,
            IUserList userList, IEmploymentRepository employmentRepository)
        {
            _environment = environment;
            _userUpload = userUpload;
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
        [BindProperty]
        public IFormFile Upload { get; set; }
        public async Task<IActionResult> OnPostAsync()
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                if (Upload != null)
                {
                    var extension = Path.GetExtension(Upload.FileName);
                    if (!extension.Contains(".xls"))
                    {
                        throw new Exception($"This file extension is not allowed!");
                    }
                    string file = _userUpload.Documentupload(Upload);
                    DataTable UsersDT = _userUpload.ExcelDataTable(file);
                    // UsersDT = _userUpload.CSVDataTable(file);
                    string AddedBy = _userManager.GetUserId(User).ToString();
                    foreach (DataRow DR in UsersDT.Rows)
                    {
                        if (DR[0] != null)
                        {
                            string userName = DR[0].ToString();
                            string password = DR[4].ToString();
                            ValidatePassword(password);
                            string firstName = DR[1].ToString();
                            string surname = DR[2].ToString();
                            string designation = DR[3].ToString().Trim();
                            int designationID = _employmentRepository.GetDesignationID(designation);
                            var user = new ApplicationUser();
                            await _userStore.SetUserNameAsync(user, userName, CancellationToken.None);
                            user.LockoutEnabled = true;
                            var result = await _userManager.CreateAsync(user, password);
                            if (result.Succeeded)
                            {
                                //await _userManager.AddToRoleAsync(user, "User");
                                user.MustChangePassword = true;
                                await _userManager.UpdateAsync(user);
                                _userList.UpdateUserPersonalNames(firstName, surname, user.Id, designationID, AddedBy);
                                _userList.UpdateUserDesignationRoles(designationID, user.Id);
                                _logger.LogInformation("User created a new account with password.");
                            }
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                    //_userUpload.ToCSV(UsersDT); 
                }            
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
            return RedirectToPage("UploadUsers");
        }
        private void ValidatePassword(string Password)
        {
            try
            {
                _customValidator.ValidatePassword(Password);
            }
            catch (Exception Ex)
            {
                throw new Exception("Error at row with Username: " + Ex.Message);
            }
        }
        public IActionResult OnGet()
        {
            string ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                UsersDT = _userList.GetUsers();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
            return Page();
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
