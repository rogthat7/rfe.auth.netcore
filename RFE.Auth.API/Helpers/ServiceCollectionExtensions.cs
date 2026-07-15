using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using RFE.Auth.API.Models;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Interfaces.Repositories;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Interfaces.Shared;
using RFE.Auth.Core.Models.Shared;
using RFE.Auth.Core.Services;
using RFE.Auth.Infrastructure.Repositories;

namespace RFE.Auth.API.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CustomOptions>(configuration.GetSection("CustomOptions"));
            services.Configure<JwtOptions>(configuration.GetSection("JwtConfig"));
            services.Configure<ApiInfo>(configuration.GetSection("ApiInfo"));
            services.Configure<CommunicationServiceConfiguration>(configuration.GetSection("CommunicationServiceConfiguration"));

            return services;
        }

        public static IServiceCollection AddJwtAndGoogleAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x => {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.GetValue<string>("JwtConfig:Secret"))),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = false,
                    ValidateLifetime = true,
                    ValidIssuer = configuration["JwtConfig:Issuer"]
                };
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => {
                options.LoginPath = "/api/auth/login";
            }).AddGoogle(options => {
                options.ClientId = configuration["Authentication:Google:ClientId"] ?? "dummy-id.apps.googleusercontent.com";
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? "dummy-secret";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            return services;
        }

        public static IServiceCollection AddOpenIddictServer(this IServiceCollection services)
        {
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                           .UseDbContext<DatabaseContext>();
                })
                .AddServer(options =>
                {
                    options.SetAuthorizationEndpointUris("connect/authorize")
                           .SetTokenEndpointUris("connect/token")
                           .SetUserInfoEndpointUris("connect/userinfo");

                    options.AllowAuthorizationCodeFlow()
                           .AllowRefreshTokenFlow();

                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();

                    options.UseAspNetCore()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableTokenEndpointPassthrough()
                           .EnableUserInfoEndpointPassthrough();

                    options.RequireProofKeyForCodeExchange();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

            return services;
        }

        public static IServiceCollection AddSwaggerAndScalar(this IServiceCollection services, string assemblyName, string baseDirectory)
        {
            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "V1",
                    Title = "rfe.auth.api",
                    Description = "ASP.NET Core 10.0 Web API"
                });

                var xmlFile = $"{assemblyName}.xml";
                var xmlPath = Path.Combine(baseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    swagger.IncludeXmlComments(xmlPath);
                }

                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"JWT Token\"",
                });

                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            });

            return services;
        }

        public static IServiceCollection AddAppServicesAndRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();
            services.AddHttpClient("CommunicationService");

            services.AddScoped<IUnitOfWork, UnitOfWork>(serviceProvider =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConection");
                return new UnitOfWork(connectionString);
            });

            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IJwtAuthenticationService, JwtAuthenticationService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<ISmsSender, SmsSender>();
            services.AddScoped<IWhatsAppSender, WhatsAppSender>();

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();

            // PostgreSQL Database Context
            services.AddDbContext<DatabaseContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConection")));

            return services;
        }
    }
}
