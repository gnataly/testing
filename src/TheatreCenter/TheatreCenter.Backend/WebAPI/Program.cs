using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Services.Options;
using TheatreCenter.Services.Services;

namespace TheatreCenter.Backend.WebAPI;
public partial class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/theatrecenter-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("Starting web application");
            var app = BuildWebApplication(args);
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

    public static WebApplication BuildWebApplication(string[] args, Action<WebApplicationBuilder>? configureBuilder = null, Action<WebApplication>? afterBuild = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(Program).Assembly.FullName,
            ContentRootPath = Directory.GetCurrentDirectory()
        });
        builder.Host.UseSerilog();

        configureBuilder?.Invoke(builder);
        ConfigureServices(builder);

        var app = builder.Build();
        ConfigurePipeline(app);
        afterBuild?.Invoke(app);
        return app;
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddControllers().AddJsonOptions(options =>
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
                Contact = new OpenApiContact
                {
                    Name = "Theatre Center",
                    Email = "support@theatrecenter.com"
                }
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
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

        if (builder.Environment.IsEnvironment("IntegrationTests"))
        {
            builder.Services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var connection = sp.GetRequiredService<SqliteConnection>();
                options.UseSqlite(connection);
            });
        }
        else
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString,
                    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));
        }

        builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
        builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection("Security"));

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
        builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
        builder.Services.AddScoped<IAuthFlowService, AuthFlowService>();

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
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt secret is not configured"))),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole(AccessLevel.Admin.ToString()));

            options.AddPolicy("UserOnly", policy =>
                policy.RequireRole(AccessLevel.User.ToString()));

            options.AddPolicy("AdminOrUser", policy =>
                policy.RequireRole(AccessLevel.Admin.ToString(), AccessLevel.User.ToString()));
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TheatreCenter API V1");
            c.RoutePrefix = "swagger";
            c.DisplayOperationId();
            c.DisplayRequestDuration();
        });

        if (!app.Environment.IsEnvironment("IntegrationTests"))
        {
            app.UseHttpsRedirection();
        }

        app.UseCors("AllowAll");
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStatusCodePages();
        app.MapControllers();

        using var scope = app.Services.CreateScope();
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
}
