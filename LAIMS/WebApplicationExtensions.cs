using Microsoft.AspNetCore.Identity;
namespace LAIMS
{
    public static class WebApplicationExtensions
    {
        public static async Task<WebApplication> CreateRolesAsync(this WebApplication app, IConfiguration configuration)
        {
            using var scope = app.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("Evaluators"))
            {
                await roleManager.CreateAsync(new IdentityRole("Evaluators"));
            }
            if (!await roleManager.RoleExistsAsync("Users"))
            {
                await roleManager.CreateAsync(new IdentityRole("Users"));
            } 
            return app;
        }
    }
}
