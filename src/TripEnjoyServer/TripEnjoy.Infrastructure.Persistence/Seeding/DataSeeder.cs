using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Property;
using TripEnjoy.Domain.PropertyType;

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
                await SeedAdminUserAsync(userManager, roleManager, dbContext, logger);
                await SeedPropertyTypesAsync(dbContext, logger);
                await SeedPropertiesAsync(dbContext, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, TripEnjoyDbContext dbContext, ILogger logger)
        {
            var adminEmail = "admin@tripenjoy.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Admin");

                    var accountResult = Account.Create(newUser.Id, newUser.Email);
                    if (accountResult.IsSuccess)
                    {
                        var account = accountResult.Value;
                        account.MarkAsActive();
                        // You can add more admin-specific details to the account here if needed
                        dbContext.Accounts.Add(account);
                        await dbContext.SaveChangesAsync();
                        logger.LogInformation("Admin account '{Email}' created and seeded successfully.", adminEmail);
                    }
                    else
                    {
                        logger.LogError("Failed to create domain account for admin {Email}: {Errors}", adminEmail, string.Join(", ", accountResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogError("Failed to create admin user {Email}: {Errors}", adminEmail, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        private static async Task SeedPropertyTypesAsync(TripEnjoyDbContext dbContext, ILogger logger)
        {
            if (!dbContext.PropertyTypes.Any())
            {
                var propertyTypes = new List<PropertyType>
                {
                    PropertyType.Create("Hotel").Value,
                    PropertyType.Create("Apartment").Value,
                    PropertyType.Create("Resort").Value,
                    PropertyType.Create("Villa").Value,
                    PropertyType.Create("Cabin").Value,
                    PropertyType.Create("Guest House").Value,
                    PropertyType.Create("Hostel").Value,
                    PropertyType.Create("Motel").Value,
                };

                await dbContext.PropertyTypes.AddRangeAsync(propertyTypes);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Property types seeded successfully.");
            }
        }

        private static async Task SeedPropertiesAsync(TripEnjoyDbContext dbContext, ILogger logger)
        {
            if (dbContext.Properties.Any())
            {
                return; // Properties already seeded
            }

            var partners = dbContext.Partners.ToList();
            var propertyTypes = dbContext.PropertyTypes.ToList();

            if (!partners.Any() || !propertyTypes.Any())
            {
                logger.LogWarning("Cannot seed Properties because there are no Partners or PropertyTypes in the database.");
                return;
            }

            var properties = new List<Property>();
            var random = new Random();

            foreach (var partner in partners)
            {
                for (int i = 1; i <= 2; i++) // Seed 2 properties for each partner
                {
                    var propertyType = propertyTypes[random.Next(propertyTypes.Count)];
                    var propertyResult = Property.Create(
                        partner.Id,
                        propertyType.Id,
                        $"{propertyType.Name} by {partner.CompanyName} #{i}",
                        $"{i * 123} Main St",
                        "Sample City",
                        "Sample Country",
                        description: $"A wonderful {propertyType.Name} managed by {partner.CompanyName}. Property number {i}."
                    );

                    if (propertyResult.IsSuccess)
                    {
                        properties.Add(propertyResult.Value);
                    }
                    else
                    {
                        logger.LogError("Failed to create property for partner {PartnerId}: {Errors}", partner.Id, string.Join(", ", propertyResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            if (properties.Any())
            {
                await dbContext.Properties.AddRangeAsync(properties);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("{PropertyCount} properties seeded successfully.", properties.Count);
            }
        }
    }
}
