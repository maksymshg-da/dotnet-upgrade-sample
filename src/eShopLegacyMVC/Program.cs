using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using eShopLegacyMVC.Modules;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Use Autofac as the service provider
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add framework services
builder.Services.AddControllersWithViews();

// Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// Read connection string from configuration
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("CatalogDBContext");
if (string.IsNullOrEmpty(connectionString))
{
    // fallback to appSettings or web.config style
    connectionString = configuration["CatalogDBContext"];
}

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddScoped<CatalogDBContext>(provider => new CatalogDBContext(connectionString));
}
else
{
    // Register default constructor if no connection string provided; runtime will fail if DB access attempted
    builder.Services.AddScoped<CatalogDBContext>();
}

builder.Services.AddScoped<CatalogDBInitializer>();

// Configure Autofac container
var useMockData = false;
var useMockConfig = builder.Configuration["UseMockData"];
if (!string.IsNullOrEmpty(useMockConfig) && bool.TryParse(useMockConfig, out var parsed))
{
    useMockData = parsed;
}

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ApplicationModule(useMockData));
});

var app = builder.Build();

// Run database initializer similar to legacy behavior
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CatalogDBContext>();
        // Ensure database initialization runs (this will invoke the initializer)
        context.Database.Initialize(true);
    }
    catch (Exception ex)
    {
        // log and continue; initialization may require manual configuration
        Console.WriteLine($"Database initializer skipped: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseSession();

app.Use(async (context, next) =>
{
    try
    {
        context.Session.SetString("MachineName", System.Environment.MachineName);
        context.Session.SetString("SessionStartTime", System.DateTime.Now.ToString());
    }
    catch
    {
    }
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.Run();