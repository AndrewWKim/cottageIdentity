using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using IdentityServer.Configurations;
using IdentityServer.Core.Repositories;
using IdentityServer.Core.Services;
using IdentityServer.Middlewares;
using IdentityServer.Services;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer.Core;
using IdentityServer.Data.Repositories;
using Serilog;
using Serilog.Events;
using IdentityServer.Data;
using Microsoft.Extensions.Hosting;

namespace IdentityServer
{
    public class Startup
    {
        private IdentityServerConfig _config;

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            _config = Configuration.Get<IdentityServerConfig>();

            // init CacheService
            CacheService.Init(_config.CacheTimeInMinutes);

            ConfigureLogging();

            var cert = new X509Certificate2(_config.CertFullName, _config.CertPassword, X509KeyStorageFlags.MachineKeySet);
            var serverBuilder = services
                .AddIdentityServer() /*options =>
                {
                    options.Authentication.CookieLifetime = TimeSpan.FromHours(10);
                }*/
                .AddSigningCredential(cert);
            serverBuilder.AddExtensionGrantValidator<BiometricSignatureValidator>();
            AddCustomServices(services);
            AddSqlStore(serverBuilder);

            services.AddCors(o => o.AddPolicy("AllowAnyOrigin", builder =>
            {
                builder.AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowAnyOrigin();
            }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowAnyOrigin");
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseGeneralExceptionMiddleware();
            app.UseSerilogMiddleware();

            app.UseStaticFiles();
        }

        private void AddCustomServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(IdentityServerConfig), _config);
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(_config.AuthConnectionString));

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IUserService, UserService>();
            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
            services.AddTransient<IExtensionGrantValidator, BiometricSignatureValidator>();
            services.AddTransient<IProfileService, ProfileService>();
        }

        private void AddSqlStore(IIdentityServerBuilder serverBuilder)
        {
            var migrationsAssembly = typeof(ApplicationDbContext).GetTypeInfo().Assembly.GetName().Name;

            serverBuilder.AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder => { SelectDbContext(builder, migrationsAssembly); };
                })
                //// this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => SelectDbContext(builder, migrationsAssembly);
                });
        }

        private void SelectDbContext(DbContextOptionsBuilder builder, string migrationAssembly)
        {
            builder.UseSqlServer(_config.AuthConnectionString, sql => sql.MigrationsAssembly(migrationAssembly));
        }

        private void ConfigureLogging()
        {
            var minimumLogLevel = string.IsNullOrWhiteSpace(_config.MinimumLogLevel)
                ? LogEventLevel.Information
                : Enum.Parse<LogEventLevel>(_config.MinimumLogLevel);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(minimumLogLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler", LogEventLevel.Warning)
                .MinimumLevel.Override("IdentityServer4.AccessTokenValidation", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .WriteTo.Async(a =>
                {
                    a.RollingFile(
                        "Logs\\identity-{Date}.txt",
                        minimumLogLevel,
                        "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj} {NewLine}{Properties:j} {NewLine}{Exception}");
                })
                .CreateLogger();
        }
    }
}
