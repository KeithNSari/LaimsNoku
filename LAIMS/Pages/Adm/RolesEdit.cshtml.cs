using LAIMS.Interfaces;
using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using LAIMS.Models.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using LAIMS.Models.Security;

namespace LAIMS.Pages.Adm
{
    [Authorize(Roles = "Admin")]
    public class RolesEditModel : PageModel
    {
        private readonly IEmploymentRepository _employmentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserList _userList;
        public DataTable RolesDT { get; set; }
        public bool UserFound { get; set; }
        public List<SelectListItem> Designations = new List<SelectListItem>();
        public RolesEditModel(UserManager<ApplicationUser> userManager, IUserList userList,
            IEmploymentRepository employmentRepository)
        {
            _userManager = userManager;
            _userList = userList;
            _employmentRepository = employmentRepository;
        }
        [BindProperty]
        public Designation Designation { get; set; }
        [BindProperty ]
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
                    RolesDT = _userList.GetUserRoles(userId);
                    LoadDesignations();
                }
            }           
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
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.FindByNameAsync(userDetails.Username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return NotFound();
            }
            return Redirect("RolesEdit?userid=" + user.Id);
        }
        public IActionResult OnPostUpdateUser(string?[] RoleIDS)
        {
            if (!string.IsNullOrEmpty(userDetails.UserID))
            {
                _userList.ClearUserRoles(userDetails.UserID);
                int roleCount = 0;
                StringBuilder sb = new StringBuilder();
                foreach (string role in RoleIDS)
                {
                    if (roleCount == 0)
                    {
                        sb.Append("'" + role + "'");
                    }
                    else
                    {
                        sb.Append(",'" + role + "'");
                    }
                    roleCount += 1;
                }
                if (roleCount > 0)
                {
                    _userList.InsertUserRoles(sb.ToString(), userDetails.UserID);
                }
               if(Designation!=null) _userList.UpdateUserDesignationRoles(Designation.ID, userDetails.UserID);
                return Redirect("TranUpdate?Success=true");
            }
            return Redirect("RolesEdit");
        }
        public IActionResult OnPostClearRoles()
        {
            if (!string.IsNullOrEmpty(userDetails.UserID))
            {
                _userList.ClearUserRoles(userDetails.UserID);  
            }
            return Redirect("RolesEdit?userid=" + userDetails.UserID );
        }
    }
}
