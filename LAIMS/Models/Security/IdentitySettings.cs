namespace LAIMS.Models.Security
{
    public class IdentitySettings
    {
        public PasswordSettings Password { get; set; }
        public LockoutSettings Lockout { get; set; }
    }
    public class PasswordSettings
    {
        public int RequiredLength { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireNonAlphanumeric { get; set; }
        public bool RequireUppercase { get; set; }
        public bool RequireLowercase { get; set; }
        public int RequiredUniqueChars { get; set; }
    }

    public class LockoutSettings
    {
        public int DefaultLockoutTimeSpanInMinutes { get; set; }
        public int MaxFailedAccessAttempts { get; set; }
        public bool AllowedForNewUsers { get; set; }
    }
}
