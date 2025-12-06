using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Reflection;
using System.Text.Json.Serialization;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Hosting;
using TheatreCenter.Domain.Models;



namespace TheatreCenter.Backend.WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        //var builder = WebApplication.CreateBuilder(args);

        //// Минимальная конфигурация
        //builder.Services.AddDbContext<AppDbContext>(options =>
        //    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        //var app = builder.Build();
        //app.Run();
        //return;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/theatrecenter-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();



        try
        {
            Log.Information("Starting web application");

            var builder = WebApplication.CreateBuilder(args);



            builder.Host.UseSerilog();

            builder.Services.AddControllers();

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.WriteIndented = true;
            });



            //builder.Services.AddApiVersioning(options =>
            //{
            //    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            //    options.AssumeDefaultVersionWhenUnspecified = true;
            //    options.ReportApiVersions = true;
            //});

            //builder.Services.AddVersionedApiExplorer(options =>
            //{
            //    options.GroupNameFormat = "'v'VVV";
            //    options.SubstituteApiVersionInUrl = true;
            //});


            builder.Services.AddEndpointsApiExplorer();

            //builder.Services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo
            //    {
            //        Title = "TheatreCenter API",
            //        Version = "v1",
            //        Description = "API for managing theatrical productions",
            //        Contact = new OpenApiContact
            //        {
            //            Name = "Theatre Center",
            //            Email = "support@theatrecenter.com"
            //        }
            //    });
            //    c.EnableAnnotations();


            //    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            //    if (File.Exists(xmlPath))
            //    {
            //        c.IncludeXmlComments(xmlPath);
            //    }
            //});

            // Обновить настройки Swagger в Program.cs
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

                //c.SwaggerDoc("v1", new OpenApiInfo
                //{
                //    Title = "CoffeeShops API",
                //    Version = "v1",
                //    Description = "REST API для системы управления кофейнями, напитками и пользователями",
                //});

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
                        new string[] {}
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

            //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            //?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


            builder.Services.AddHttpContextAccessor();
            //builder.Services.AddScoped<IConnectionResolver, ConnectionResolver>();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString,
                    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

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
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"])),
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

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            //builder.Services.AddApiVersioning(options =>
            //{
            //    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            //    options.AssumeDefaultVersionWhenUnspecified = true;
            //    options.ReportApiVersions = true;
            //});

            //builder.Services.AddVersionedApiExplorer(options =>
            //{
            //    options.GroupNameFormat = "'v'VVV";
            //    options.SubstituteApiVersionInUrl = true;
            //});

            //var app = builder.Build();

            //builder.Services.AddCors();
            builder.Services.AddCors(options =>
            {
                //options.AddPolicy("AllowAll", policy =>
                //{
                //    policy.WithOrigins("http://localhost:5173")
                //          .AllowAnyMethod()
                //          .AllowAnyHeader();
                //});
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            var app = builder.Build();

            //app.UseSwagger();
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoffeeShops API v1");
            //    c.RoutePrefix = "api/v1"; // Swagger будет по /api/v1
            //});


            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TheatreCenter API V1");
                c.RoutePrefix = "swagger";
                c.DisplayOperationId();
                c.DisplayRequestDuration();
            });


            //if (app.Environment.IsDevelopment())
            //{
            //// Рабочая версия:
            //app.UseSwagger();
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TheatreCenter API V1");
            //    c.RoutePrefix = "swagger";
            //    c.DisplayOperationId();
            //    c.DisplayRequestDuration();
            //});





            //app.UseSwagger(c =>
            //{
            //    c.RouteTemplate = "swagger/{documentName}/swagger.json";
            //    c.PreSerializeFilters.Add((swagger, httpReq) =>
            //    {
            //        swagger.Servers = new List<OpenApiServer>
            //        {
            //            new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/api/v1" }
            //        };
            //    });
            //});

            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/api/v1/swagger/swagger.json", "API V1");
            //    c.RoutePrefix = "api/v1/swagger";
            //});

            //    app.UseSwagger(c =>
            //    {
            //        c.RouteTemplate = "swagger/{documentName}/swagger.json";
            //        c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
            //        {
            //            swaggerDoc.Servers = new List<OpenApiServer>
            //{
            //    new OpenApiServer {
            //        Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/api/v1",
            //        Description = "API Server"
            //    }
            //};
            //        });
            //    });

            //    app.UseSwaggerUI(c =>
            //    {
            //        c.SwaggerEndpoint("/api/v1/swagger/v1/swagger.json", "TheatreCenter API V1");
            //        c.RoutePrefix = "api/v1/swagger";
            //        c.DisplayOperationId();
            //        c.DisplayRequestDuration();
            //    });
            //}
            //else
            //{
            //    app.UseExceptionHandler("/error");
            //}





            //if (app.Environment.IsDevelopment())
            //{
            //    app.UsePathBase("/api/v1");

            //    app.UseSwagger(c =>
            //    {
            //        c.RouteTemplate = "swagger/{documentName}/swagger.json";
            //        c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
            //        {
            //            swaggerDoc.Servers = new List<OpenApiServer>
            //{
            //    new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/api/v1" }
            //};
            //        });
            //    });

            //    app.UseSwaggerUI(c =>
            //    {
            //        c.SwaggerEndpoint("/api/v1/swagger/v1/swagger.json", "TheatreCenter API V1");
            //        c.RoutePrefix = "api/v1/swagger";
            //    });
            //}






            //app.UsePathBase(new PathString("/api/v1"));

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
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
                    //var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    //logger.LogError(ex, "An error occurred while migrating the database.");
                    Log.Error(ex, "An error occurred while migrating the database.");
                }
            }

            //app.MapGet("/", () => "TheatreCenter Backend is running!");
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
