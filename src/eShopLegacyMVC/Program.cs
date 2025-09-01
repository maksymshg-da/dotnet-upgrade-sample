
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Modules;
using eShopLegacyMVC.Services;
using System.Data.Entity;
using System.Globalization;

    namespace eShopLegacyMVC
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add connection string from Web.config
                builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                builder.Configuration.AddEnvironmentVariables();

                // Store configuration in static ConfigurationManager
                ConfigurationManager.Configuration = builder.Configuration;

                // Add services to the container (formerly ConfigureServices)
                builder.Services.AddControllersWithViews();

                // Add session support
                builder.Services.AddDistributedMemoryCache();
                builder.Services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

                // Register application services
                var useMockData = builder.Configuration.GetValue<bool>("UseMockData");
                var useCustomizationData = builder.Configuration.GetValue<bool>("UseCustomizationData");
                builder.Services.RegisterApplicationServices(useMockData);

                // Add logging
                builder.Services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddDebug();
                });

                // Register database initializer
                builder.Services.AddScoped<IDatabaseInitializer<CatalogDBContext>, CatalogDBInitializer>();

                // Set globalization settings from Web.config
                builder.Services.Configure<RequestLocalizationOptions>(options =>
                {
                    var supportedCultures = new[] { new CultureInfo("en-US") };
                    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                });

                var app = builder.Build();

                // Configure the HTTP request pipeline (formerly Configure method)
                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                // Apply request localization from Web.config
                app.UseRequestLocalization();

                // Use session middleware
                app.UseSession();

                // Configure database
using (var scope = app.Services.CreateScope())
                {
                    var useMockDataFromConfig = app.Configuration.GetValue<bool>("UseMockData");
                    if (!useMockDataFromConfig)
                    {
                        var initializer = scope.ServiceProvider.GetService<IDatabaseInitializer<CatalogDBContext>>();
                        Database.SetInitializer(initializer);

                        // Set connection string from Web.config
                        if (!string.IsNullOrEmpty(app.Configuration.GetConnectionString("CatalogDBContext")))
                        {
                            // Connection string is available in configuration
                            // EntityFramework 6 will use it through the ConfigurationManager
                        }
                    }
                }

                // Add request logging middleware
                app.Use(async (context, next) =>
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    var activityId = GetOrCreateActivityId();
                    var requestInfo = $"{context.Request.Path}, {context.Request.Headers["User-Agent"]}";

                    logger.LogDebug($"Request: {activityId} - {requestInfo}");

                    // Store session data (equivalent to Session_Start)
                    if (!context.Session.Keys.Contains("MachineName"))
                    {
                        context.Session.SetString("MachineName", Environment.MachineName);
                        context.Session.SetString("SessionStartTime", DateTime.Now.ToString());
                    }

                    await next.Invoke();
                });

                app.UseRouting();

                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Register areas if needed
                app.MapAreaControllerRoute(
                    name: "areas",
                    areaName: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                // Configure client validation settings from Web.config
                builder.Services.AddMvc().AddViewOptions(options =>
                {
                    options.HtmlHelperOptions.ClientValidationEnabled = true;
                });

                app.Run();
            }

            private static string GetOrCreateActivityId()
            {
                if (Trace.CorrelationManager.ActivityId == Guid.Empty)
                {
                    Trace.CorrelationManager.ActivityId = Guid.NewGuid();
                }
                return Trace.CorrelationManager.ActivityId.ToString();
            }
        }

        public static class ServiceCollectionExtensions
        {
            public static IServiceCollection RegisterApplicationServices(this IServiceCollection services, bool useMockData)
            {
                // Register application modules
                if (useMockData)
                {
// Register mock implementations (temporarily using real service until mock is created)
                    services.AddScoped<ICatalogService, CatalogService>();
                }
                else
                {
                    // Register real implementations
                    services.AddScoped<ICatalogService, CatalogService>();
                    services.AddScoped<CatalogDBContext>();
                }

                return services;
            }
        }

        public class ConfigurationManager
        {
            public static IConfiguration Configuration { get; set; }
        }
    }