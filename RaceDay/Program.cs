
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;

namespace RaceDay
{
    public partial class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Controllers and database
            builder.Services.AddControllers();

            builder.Services.AddDbContext<RaceDayContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // Password hashing service
            builder.Services.AddScoped<
                Microsoft.AspNetCore.Identity.IPasswordHasher<RaceDay.Models.User>,
                Microsoft.AspNetCore.Identity.PasswordHasher<RaceDay.Models.User>>();

            // Swagger generation for interactive UI
            builder.Services.AddEndpointsApiExplorer();
            IServiceCollection servicesWithSwagger = builder.Services.AddSwaggerGen();

            // Cookie-based authentication
            void cookieAuthOptions(CookieAuthenticationOptions options)
            {
                options.Cookie.Name = "RaceDay.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;

                // APIs return status codes instead of redirecting to login pages.
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode =
                        StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode =
                        StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            }
            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(cookieAuthOptions);

            builder.Services.AddAuthorization();

            // Note: Use Swashbuckle (AddSwaggerGen) instead of Microsoft.AspNetCore.OpenApi
            // to avoid runtime conflicts between OpenAPI generators.

            var app = builder.Build();

            // OpenAPI / Swagger UI (development only)
            if (app.Environment.IsDevelopment())
            {
                // Use Swashbuckle to provide interactive documentation.
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Authentication must run before authorization.
            app.UseAuthentication();
            IApplicationBuilder applicationBuilder = app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

