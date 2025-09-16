using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TripEnjoy.Domain.Account;

namespace TripEnjoy.Infrastructure.Persistence.Seeding
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<TripEnjoyDbContext>>();
            try
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var dbContext = serviceProvider.GetRequiredService<TripEnjoyDbContext>();

                // Seed Roles
                string[] roleNames = { "Admin", "Partner", "User" };
                foreach (var roleName in roleNames)
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
                    }
                }

                // Seed Partner Accounts
                for (int i = 1; i <= 3; i++)
                {
                    var email = $"partner{i}@tripenjoy.com";
                    var user = await userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        var newUser = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = true
                        };

                        var result = await userManager.CreateAsync(newUser, "Partner@123");

                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(newUser, "Partner");

                            var accountResult = Account.Create(newUser.Id, newUser.Email);
                            if (accountResult.IsSuccess)
                            {
                                var account = accountResult.Value;
                                account.MarkAsActive();
                                account.AddNewPartner($"Partner Company {i}", $"12345678{i}", $"Address {i}", email);

                                dbContext.Accounts.Add(account);
                                await dbContext.SaveChangesAsync();
                                logger.LogInformation("Partner account '{Email}' created and seeded successfully.", email);
                            }
                            else
                            {
                                logger.LogError("Failed to create domain account for {Email}: {Errors}", email, string.Join(", ", accountResult.Errors.Select(e => e.Description)));
                            }
                        }
                        else
                        {
                            logger.LogError("Failed to create user {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}
