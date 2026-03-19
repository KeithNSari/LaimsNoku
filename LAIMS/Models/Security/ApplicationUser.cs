using Microsoft.AspNetCore.Identity;

namespace LAIMS.Models.Security
{
    public class ApplicationUser : IdentityUser
    {
        // Custom property to enforce password change
        public bool MustChangePassword { get; set; } = false;
    }
}
