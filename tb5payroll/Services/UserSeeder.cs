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
            await SeedAdminUser(); // Password handled inside this method
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
            const string tempPassword = "Admin@123"; // Must meet policy

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

                var createResult = await _userManager.CreateAsync(user, tempPassword);
                if (createResult.Succeeded)
                {
                    _logger.LogInformation("Admin user created successfully");

                    var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogError($"Failed to assign 'Admin' role: {string.Join(", ", roleResult.Errors)}");
                    }
                }
                else
                {
                    _logger.LogError($"Admin creation failed: {string.Join(", ", createResult.Errors)}");
                    throw new Exception($"Admin user creation failed: {string.Join(", ", createResult.Errors)}");
                }
            }
            else
            {
                _logger.LogInformation("Admin user already exists");

                var hasPassword = await _userManager.HasPasswordAsync(adminUser);
                if (!hasPassword)
                {
                    _logger.LogInformation("Admin user has no password. Setting default...");
                    var addPasswordResult = await _userManager.AddPasswordAsync(adminUser, tempPassword);
                    if (addPasswordResult.Succeeded)
                    {
                        _logger.LogInformation("Admin password set successfully.");
                    }
                    else
                    {
                        _logger.LogError(
                            $"Failed to set admin password: {string.Join(", ", addPasswordResult.Errors)}");
                    }
                }

                // Optional: ensure admin has the "Admin" role
                if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Admin role re-assigned to existing user.");
                }
            }
        }
        
        




    }
}