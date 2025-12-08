using Microsoft.AspNetCore.Identity;
using TicketManagementApp.Models;

namespace TicketManagementApp.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            var admin = new ApplicationUser { UserName = "admin@local.com", Email = "admin@local.com", FullName = "Admin User" };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");

            var dev1 = new ApplicationUser { UserName = "dev1@local.com", Email = "dev1@local.com", FullName = "Dev One" };
            await userManager.CreateAsync(dev1, "Dev123!");
            await userManager.AddToRoleAsync(dev1, "User");

            if (!context.Tickets.Any())
            {
                context.Tickets.AddRange(
                    new Ticket { Title = "Bug page rapport", Description = "Impossible de générer PDF",UrgencyLevel=2 ,Status = "Open", Category = "Bug", CreatedByUserId = admin.Id, AssignedToUserId = dev1.Id },
                    new Ticket { Title = "Ajouter champ employé", Description = "Champ pour responsable",UrgencyLevel=5 , Status = "Open", Category = "Improvement", CreatedByUserId = admin.Id, AssignedToUserId = dev1.Id }
                );
                context.SaveChanges();
            }
        }
    }

}
