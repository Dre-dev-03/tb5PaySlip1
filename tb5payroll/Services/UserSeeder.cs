using Microsoft.AspNetCore.Identity;

namespace tb5payroll.Services
{
    public class UserSeeder
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserSeeder> _logger;

        public UserSeeder(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserSeeder> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await SeedRoles();
            await SeedAdminUser();
            await ResetAdminPassword(); // Reset to ensure password works
        }

        private async Task SeedRoles()
        {
            string[] roleNames = { "Admin", "Manager", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                    _logger.LogInformation($"Created role: {roleName}");
                }
            }
        }

        private async Task SeedAdminUser()
{
    const string adminEmail = "admin@bigfive.com";
    var adminUser = await _userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        _logger.LogInformation("Creating new admin user...");
        var user = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user);
        
        if (createResult.Succeeded)
        {
            _logger.LogInformation("Admin user created successfully");
            var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
            if (!roleResult.Succeeded)
            {
                _logger.LogError($"Failed to add admin role: {string.Join(", ", roleResult.Errors)}");
            }
        }
        else
        {
            _logger.LogError($"Failed to create admin: {string.Join(", ", createResult.Errors)}");
            throw new Exception($"Admin user creation failed: {string.Join(", ", createResult.Errors)}");
        }
    }
    else
    {
        _logger.LogInformation("Admin user already exists");
    }
}

private async Task ResetAdminPassword()
{
    const string adminEmail = "admin@bigfive.com";
    const string tempPassword = "Admin@123"; // Note: This meets default requirements
    
    var admin = await _userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        _logger.LogError("Admin user not found for password reset");
        throw new Exception("Admin user not found");
    }

    // Check if password exists first
    var hasPassword = await _userManager.HasPasswordAsync(admin);
    if (hasPassword)
    {
        _logger.LogInformation("Removing existing password...");
        var removeResult = await _userManager.RemovePasswordAsync(admin);
        if (!removeResult.Succeeded)
        {
            _logger.LogError($"Failed to remove old password: {string.Join(", ", removeResult.Errors)}");
            throw new Exception($"Password removal failed: {string.Join(", ", removeResult.Errors)}");
        }
    }

    _logger.LogInformation("Setting new password...");
    var addResult = await _userManager.AddPasswordAsync(admin, tempPassword);
    if (addResult.Succeeded)
    {
        _logger.LogInformation($"Admin password successfully reset to: {tempPassword}");
    }
    else
    {
        _logger.LogError($"Password reset failed: {string.Join(", ", addResult.Errors)}");
        throw new Exception($"Password reset failed: {string.Join(", ", addResult.Errors)}");
    }
}
    }
}