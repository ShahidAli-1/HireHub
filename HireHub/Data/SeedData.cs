using HireHub.Helpers;
using HireHub.Models;
using Microsoft.EntityFrameworkCore;

namespace HireHub.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created and migrations applied
            await context.Database.MigrateAsync();

            // ===== Seed Admin User =====
            if (!await context.Users.AnyAsync(u => u.Role == RoleConstants.Admin))
            {
                var admin = new User
                {
                    Email = "admin@hirehub.pk",
                    FullName = "System Admin",
                    Role = RoleConstants.Admin,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    IsActive = true,
                    IsVerified = true,
                    CurrentPlan = "Free",
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(admin);
                await context.SaveChangesAsync();
            }

            // ===== Seed Categories =====
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new() { Name = "IT & Software",       Icon = "bi-code-slash",     Description = "Software, web, mobile, DevOps" },
                    new() { Name = "Marketing",           Icon = "bi-megaphone",      Description = "Digital marketing, SEO, content" },
                    new() { Name = "Finance & Accounting",Icon = "bi-cash-coin",      Description = "Accounting, audit, taxation" },
                    new() { Name = "Engineering",         Icon = "bi-gear",           Description = "Civil, mechanical, electrical" },
                    new() { Name = "Sales & Business",    Icon = "bi-graph-up",       Description = "Sales, BD, account management" },
                    new() { Name = "Design & Creative",   Icon = "bi-palette",        Description = "UI/UX, graphic, video" },
                    new() { Name = "Customer Support",    Icon = "bi-headset",        Description = "Call center, technical support" },
                    new() { Name = "Human Resources",     Icon = "bi-people",         Description = "HR, recruitment, payroll" },
                    new() { Name = "Education & Teaching",Icon = "bi-book",           Description = "Teachers, trainers, tutors" },
                    new() { Name = "Healthcare",          Icon = "bi-heart-pulse",    Description = "Doctors, nurses, medical staff" }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }
        }
    }
}