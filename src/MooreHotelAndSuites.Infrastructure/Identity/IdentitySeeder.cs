using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Domain.Constants;  // Add this using

namespace MooreHotelAndSuites.Infrastructure.Identity
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

            // Use Roles.AllRoles from the Constants class
            foreach (var role in Roles.AllRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Create admin user from environment variables
            var adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin@moorehotel.com";
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@moorehotel.com";
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin123!";

            var adminUser = await userManager.FindByNameAsync(adminUsername)
                            ?? await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    FullName = "Moore Hotel Administrator",
                    UserName = adminUsername,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception(errors);
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        }
    }
}