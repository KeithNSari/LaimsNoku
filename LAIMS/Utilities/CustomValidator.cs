using LAIMS.Interfaces.Utilities;

namespace LAIMS.Utilities
{
    public class CustomValidator: ICustomValidator
    {
        private readonly IConfiguration _configuration;
        public CustomValidator(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void ValidatePassword(string Password)
        {
            // Fetch settings from configuration with null handling and default values
            int requiredLength = GetConfigValue<int>("IdentitySettings:Password:RequiredLength", 8);
            bool requireDigit = GetConfigValue<bool>("IdentitySettings:Password:RequireDigit", true);
            bool requireNonAlphanumeric = GetConfigValue<bool>("IdentitySettings:Password:RequireNonAlphanumeric", true);
            bool requireUppercase = GetConfigValue<bool>("IdentitySettings:Password:RequireUppercase", true);
            bool requireLowercase = GetConfigValue<bool>("IdentitySettings:Password:RequireLowercase", true);
            int requiredUniqueChars = GetConfigValue<int>("IdentitySettings:Password:RequiredUniqueChars", 1);

            // Validate password length
            if (Password.Length < requiredLength)
            {
                throw new Exception($"Password must be at least {requiredLength} characters long.");
            }

            // Validate digit requirement
            if (requireDigit && !Password.Any(char.IsDigit))
            {
                throw new Exception("Password must contain at least one digit.");
            }

            // Validate non-alphanumeric characters
            if (requireNonAlphanumeric && !Password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                throw new Exception("Password must contain at least one non-alphanumeric character.");
            }

            // Validate uppercase letters
            if (requireUppercase && !Password.Any(char.IsUpper))
            {
                throw new Exception("Password must contain at least one uppercase letter.");
            }

            // Validate lowercase letters
            if (requireLowercase && !Password.Any(char.IsLower))
            {
                throw new Exception("Password must contain at least one lowercase letter.");
            }

            // Validate unique characters requirement
            if (Password.Distinct().Count() < requiredUniqueChars)
            {
                throw new Exception($"Password must contain at least {requiredUniqueChars} unique characters.");
            }
        }

        // Helper method to safely retrieve configuration values with a default fallback
        private T GetConfigValue<T>(string key, T defaultValue)
        {
            try
            {
                var value = _configuration[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    return defaultValue;
                }

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                // If any error occurs, return the default value
                return defaultValue;
            }
        }
    }
}
