using System;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Data;
using ShoppingCart.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuration: database provider and connection string
var dbProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";
if (dbProvider.Equals("MySQL", StringComparison.OrdinalIgnoreCase))
{
    // Use MySQL (Pomelo)
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));
}
else
{
    // Default: SQL Server
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Authentication with Microsoft Entra ID (OpenID Connect)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    var azure = builder.Configuration.GetSection("AzureAd");
    var tenant = azure["TenantId"] ?? "common";
    options.ClientId = azure["ClientId"];
    options.ClientSecret = azure["ClientSecret"];
    options.Authority = $"https://login.microsoftonline.com/{tenant}/v2.0";
    options.ResponseType = "code";
    options.ResponseMode = "form_post";
    options.UsePkce = true;
    options.SaveTokens = true;
    options.Scope.Add("offline_access");
    options.Scope.Add("openid");
    options.Scope.Add("profile");
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Seed some products if none exist
    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new Product { Name = "Laptop", Description = "High-performance laptop for work and gaming", Price = 1299.99m },
            new Product { Name = "Wireless Mouse", Description = "Ergonomic wireless mouse with long battery life", Price = 49.99m },
            new Product { Name = "Keyboard", Description = "Mechanical keyboard with RGB lighting", Price = 89.99m },
            new Product { Name = "Monitor", Description = "27-inch 4K UHD monitor", Price = 399.99m },
            new Product { Name = "Headphones", Description = "Noise-cancelling over-ear headphones", Price = 199.99m }
        );
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllers();

app.Run();
