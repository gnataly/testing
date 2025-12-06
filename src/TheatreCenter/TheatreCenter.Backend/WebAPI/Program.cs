using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using TheatreCenter.Backend.WebAPI.Telemetry;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Services.Services;

namespace TheatreCenter.Backend.WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var loggingOptions = builder.Configuration.GetSection(LoggingOptions.SectionName).Get<LoggingOptions>() ?? new LoggingOptions();
        var loggingLevelSwitch = new LoggingLevelSwitch(loggingOptions.Extended ? LogEventLevel.Debug : LogEventLevel.Information);

        var serilogConfiguration = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(loggingLevelSwitch)
            .Enrich.FromLogContext()
            .Enrich.WithSpan()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code, restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.File("logs/theatrecenter-.txt", rollingInterval: RollingInterval.Day);

        if (loggingOptions.Extended)
        {
            serilogConfiguration.WriteTo.File(
                loggingOptions.ExtendedFilePath ?? "logs/theatrecenter-extended-.txt",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Debug);
        }

        Log.Logger = serilogConfiguration.CreateLogger();

        try
        {
            Log.Information("Starting web application");

            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .MinimumLevel.ControlledBy(loggingLevelSwitch)
                    .Enrich.FromLogContext()
                    .Enrich.WithSpan()
                    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                    .WriteTo.File("logs/theatrecenter-.txt", rollingInterval: RollingInterval.Day);

                if (loggingOptions.Extended)
                {
                    configuration.WriteTo.File(
                        loggingOptions.ExtendedFilePath ?? "logs/theatrecenter-extended-.txt",
                        rollingInterval: RollingInterval.Day,
                        restrictedToMinimumLevel: LogEventLevel.Debug);
                }
            });

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "TheatreCenter API",
                    Version = "v1",
                    Description = "API for managing theatrical productions",
                    Contact = new OpenApiContact
                    {
                        Name = "Theatre Center",
                        Email = "support@theatrecenter.com"
                    }
                });
                c.EnableAnnotations();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            var connectionString = builder.Configuration["DB_CONNECTION_STRING"]
                ?? builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

            builder.Services.AddScoped<IActorRepository, ActorRepository>();
            builder.Services.AddScoped<IMusicalRepository, MusicalRepository>();
            builder.Services.AddScoped<ITheatreRepository, TheatreRepository>();
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<ICastMemberRepository, CastMemberRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IShowRepository, ShowRepository>();
            builder.Services.AddScoped<IThemeRepository, ThemeRepository>();

            builder.Services.AddScoped<IActorService, ActorService>();
            builder.Services.AddScoped<IMusicalService, MusicalService>();
            builder.Services.AddScoped<ITheatreService, TheatreService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ICastMemberService, CastMemberService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IShowService, ShowService>();
            builder.Services.AddScoped<IThemeService, ThemeService>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"])),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole(AccessLevel.Admin.ToString()));
                options.AddPolicy("UserOnly", policy => policy.RequireRole(AccessLevel.User.ToString()));
                options.AddPolicy("AdminOrUser", policy => policy.RequireRole(AccessLevel.Admin.ToString(), AccessLevel.User.ToString()));
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ReactPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddTheatreCenterTelemetry(builder.Configuration, Log.Logger);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TheatreCenter API V1");
                    c.RoutePrefix = "swagger";
                    c.DisplayOperationId();
                    c.DisplayRequestDuration();
                    c.ConfigObject.DisplayRequestDuration = true;
                });
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            // HTTPS редирект отключен в Dev, чтобы не ловить 307 при обращении к http://localhost:5000
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            app.UseCors("ReactPolicy");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStatusCodePages();
            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while migrating the database.");
                }
            }

            app.MapGet("/", () => "TheatreCenter Backend is running!");
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
