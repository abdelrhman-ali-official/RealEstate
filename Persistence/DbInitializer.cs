using Domain.Contracts;
using Domain.Entities.ProductEntities;
using Domain.Entities.SecurityEntities;
using Domain.Entities.SubscriptionEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Persistence
{
    public class DbInitializer : IDbInitializer
    {
        private readonly StoreContext _storeContext;

        private readonly UserManager<User> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(StoreContext storeContext,
          RoleManager<IdentityRole> roleManager,
          //NewModuleContext newModuleContext,
          UserManager<User> userManager)
        {
            _storeContext = storeContext;
            _roleManager = roleManager;
            _userManager = userManager;
            //_newModuleContext = newModuleContext;
        }

      

        public async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine("Attempting to connect to database...");
                bool hasConnection = false;

                try
                {
                    // Test the connection before attempting any operations
                    hasConnection = await _storeContext.Database.CanConnectAsync();
                    Console.WriteLine($"Database connection test: {(hasConnection ? "Successful" : "Failed")}");

                    if (!hasConnection)
                    {
                        Console.WriteLine("Cannot connect to database. Check connection string and network.");
                        return;
                    }
                }
                catch (Exception connEx)
                {
                    Console.WriteLine($"Connection test error: {connEx.Message}");
                    Console.WriteLine($"Connection error type: {connEx.GetType().Name}");
                    if (connEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {connEx.InnerException.Message}");
                    }
                    return;
                }

                if (_storeContext.Database.GetPendingMigrations().Any())
                {
                    Console.WriteLine("Applying pending migrations...");
                    await _storeContext.Database.MigrateAsync();
                    Console.WriteLine("Migrations completed successfully");
                }

                // Initialize Identity (roles and admin user)
                await InitializeIdentityAsync();

                // Seed product data
                await SeedProductDataAsync();

                // Seed subscription packages
                await SeedPackagesAsync();

               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization error: {ex.Message}");
                Console.WriteLine($"Error type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner exception type: {ex.InnerException.GetType().Name}");
                }
                // Re-throw only critical exceptions that should stop the application
                if (ex is DbUpdateException || ex is SqlException)
                    throw;
            }
        }

        private async Task SeedProductDataAsync()
        {
            try
            {
                // Get the seeding directory path
                string seedingPath = GetSeedingPath();
                Console.WriteLine($"Seeding path: {seedingPath}");

                // Seed ProductTypes first
                if (!_storeContext.ProductType.Any())
                {
                    Console.WriteLine("Seeding product types...");
                    string typesPath = Path.Combine(seedingPath, "types.json");
                    if (File.Exists(typesPath))
                    {
                        var typesData = await File.ReadAllTextAsync(typesPath);
                        var types = JsonSerializer.Deserialize<List<ProductType>>(typesData);
                        if (types != null && types.Any())
                        {
                            await _storeContext.ProductType.AddRangeAsync(types);
                            await _storeContext.SaveChangesAsync();
                            Console.WriteLine($"Added {types.Count} product types");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Types file not found at: {typesPath}");
                    }
                }

                // Seed ProductBrands
                if (!_storeContext.ProductBrands.Any())
                {
                    Console.WriteLine("Seeding product brands...");
                    string brandsPath = Path.Combine(seedingPath, "brands.json");
                    if (File.Exists(brandsPath))
                    {
                        var brandsData = await File.ReadAllTextAsync(brandsPath);
                        var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);
                        if (brands != null && brands.Any())
                        {
                            await _storeContext.ProductBrands.AddRangeAsync(brands);
                            await _storeContext.SaveChangesAsync();
                            Console.WriteLine($"Added {brands.Count} product brands");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Brands file not found at: {brandsPath}");
                    }
                }

                // Seed Products
                if (!_storeContext.Products.Any())
                {
                    Console.WriteLine("Seeding products...");
                    string productsPath = Path.Combine(seedingPath, "products.json");
                    if (File.Exists(productsPath))
                    {
                        var productsData = await File.ReadAllTextAsync(productsPath);
                        var products = JsonSerializer.Deserialize<List<Product>>(productsData);
                        if (products != null && products.Any())
                        {
                            await _storeContext.Products.AddRangeAsync(products);
                            await _storeContext.SaveChangesAsync();
                            Console.WriteLine($"Added {products.Count} products");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Products file not found at: {productsPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding product data: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        private async Task SeedPackagesAsync()
        {
            try
            {
                if (!_storeContext.Packages.Any())
                {
                    Console.WriteLine("Seeding subscription packages...");

                    var packages = new List<Package>
                    {
                        new Package
                        {
                            Name = "Basic",
                            Description = "Perfect for getting started with property listings",
                            Price = 0,
                            MonthlyPrice = 0, // Free for monthly
                            YearlyPrice = 10000, // 10000 L.E for yearly
                            PropertyLimit = 10,
                            ShowPropertyViews = true,
                            ShowWishlistNotifications = true,
                            ShowWishlistUserDetails = false,
                            FunnelTracking = false,
                            ExportLeads = false,
                            DirectContactSystem = false,
                            WhatsAppIntegration = false,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Package
                        {
                            Name = "Pro",
                            Description = "Advanced features for professional real estate agents",
                            Price = 0,
                            MonthlyPrice = 0, // Free for monthly
                            YearlyPrice = 10000, // 10000 L.E for yearly
                            PropertyLimit = 50,
                            ShowPropertyViews = true,
                            ShowWishlistNotifications = true,
                            ShowWishlistUserDetails = true,
                            FunnelTracking = true,
                            ExportLeads = true,
                            DirectContactSystem = false,
                            WhatsAppIntegration = false,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Package
                        {
                            Name = "Premium",
                            Description = "Ultimate package with unlimited properties and direct communication",
                            Price = 0,
                            MonthlyPrice = 0, // Free for monthly
                            YearlyPrice = 10000, // 10000 L.E for yearly
                            PropertyLimit = -1, // Unlimited
                            ShowPropertyViews = true,
                            ShowWishlistNotifications = true,
                            ShowWishlistUserDetails = true,
                            FunnelTracking = true,
                            ExportLeads = true,
                            DirectContactSystem = true,
                            WhatsAppIntegration = true,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    await _storeContext.Packages.AddRangeAsync(packages);
                    await _storeContext.SaveChangesAsync();
                    Console.WriteLine($"Added {packages.Count} subscription packages");
                }
                else
                {
                    Console.WriteLine("Subscription packages already exist, skipping seeding");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding packages: {ex.Message}");
                // Don't throw exception to prevent application startup failure
            }
        }

        private string GetSeedingPath()
        {
            // Try multiple possible paths
            var possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Persistence", "Data", "Seeding"),
                Path.Combine(Directory.GetCurrentDirectory(), "Persistence", "Data", "Seeding"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seeding"),
                Path.Combine(Directory.GetCurrentDirectory(), "Data", "Seeding"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "Data", "Seeding"),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "Seeding")
            };

            foreach (var path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    Console.WriteLine($"Found seeding directory at: {path}");
                    return path;
                }
            }

            // If no path found, return the default one
            var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Persistence", "Data", "Seeding");
            Console.WriteLine($"No seeding directory found, using default: {defaultPath}");
            return defaultPath;
        }

        public async Task InitializeIdentityAsync()
        {
            // Create all roles from the Role enum
            var roles = Enum.GetValues(typeof(Role)).Cast<Role>();
            
            foreach (var role in roles)
            {
                var roleName = role.ToString();
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine($"Created role: {roleName}");
                }
            }

            if (!_userManager.Users.Any())
            {
                var adminUser = new User
                {
                    DisplayName = "Abdelrhman Ali",
                    Email = "abdelrhmanali2119@gmail.com",
                    UserName = "AbdelrhmanAli22",
                    PhoneNumber = "01142029061"
                };

                await _userManager.CreateAsync(adminUser, "Abdo@888");
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}

