using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using eShopLegacyMVC.Modules;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Use Autofac as the service provider
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add framework services
builder.Services.AddControllersWithViews();

// Register classic EF DbContext as scoped and initializer
builder.Services.AddScoped<CatalogDBContext>(provider => new CatalogDBContext("name=CatalogDBContext"));
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
    // Register application module (preserves existing Autofac registrations)
    containerBuilder.RegisterModule(new ApplicationModule(useMockData));
});

var app = builder.Build();

// Run database initializer similar to legacy behavior
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var initializer = services.GetRequiredService<CatalogDBInitializer>();
    var context = services.GetRequiredService<CatalogDBContext>();
    // For classic EF we can call initializer directly
    initializer.Seed(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.Run();
