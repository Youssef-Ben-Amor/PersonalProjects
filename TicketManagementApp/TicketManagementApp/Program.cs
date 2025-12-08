



using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketManagementApp.Data;
using TicketManagementApp.Models;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// 1️⃣ Configurer DbContext avec InMemory
// ----------------------
builder.Services.AddDbContext<ApplicationDbContext>(static options =>
    options.UseInMemoryDatabase("TicketDb")
);

// ----------------------
// 2️⃣ Configurer Identity
// ----------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ----------------------
// 3️⃣ Ajouter MVC
// ----------------------
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ----------------------
// 4️⃣ Middleware
// ----------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ----------------------
// 5️⃣ Seed Data (Admin, User, Tickets)
// ----------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.Initialize(services);
}

// ----------------------
// 6️⃣ Route par défaut
// ----------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Login}" // Login comme page d'accueil
);

app.Run();
