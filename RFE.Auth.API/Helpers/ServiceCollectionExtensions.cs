using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
using Microsoft.AspNetCore.Authorization;
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
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        var clientUrl = configuration["ClientAppUrl"] ?? "https://localhost:3001";
                        var originalUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
                        var redirectUri = clientUrl + "/login?ReturnUrl=" + Uri.EscapeDataString(originalUrl);
                        context.Response.Redirect(redirectUri);
                        return Task.CompletedTask;
                    }
                };
            }).AddGoogle(options => {
                options.ClientId = configuration["Authentication:Google:ClientId"] ?? "dummy-id.apps.googleusercontent.com";
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? "dummy-secret";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddOAuth("GitHub", options => {
                options.ClientId = configuration["Authentication:Github:ClientId"] ?? "dummy-id";
                options.ClientSecret = configuration["Authentication:Github:ClientSecret"] ?? "dummy-secret";
                options.CallbackPath = new PathString("/signin-github");
                options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                options.UserInformationEndpoint = "https://api.github.com/user";
                options.ClaimsIssuer = "GitHub";
                options.SaveTokens = true;
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                        request.Headers.UserAgent.ParseAdd("rfe-auth-api");

                        using var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException("An error occurred while retrieving the user profile from GitHub.");
                        }

                        using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                        var root = user.RootElement;

                        var userId = root.GetProperty("id").GetInt64().ToString();
                        context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId, context.Options.ClaimsIssuer));

                        if (root.TryGetProperty("login", out var login))
                        {
                            context.Identity.AddClaim(new Claim(ClaimTypes.Name, login.GetString(), context.Options.ClaimsIssuer));
                        }
                        if (root.TryGetProperty("name", out var name) && name.ValueKind != JsonValueKind.Null)
                        {
                            context.Identity.AddClaim(new Claim("urn:github:name", name.GetString(), context.Options.ClaimsIssuer));
                        }

                        string emailStr = null;
                        if (root.TryGetProperty("email", out var email) && email.ValueKind != JsonValueKind.Null && !string.IsNullOrEmpty(email.GetString()))
                        {
                            emailStr = email.GetString();
                        }
                        else
                        {
                            // Fallback to fetch emails
                            using var emailRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
                            emailRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            emailRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                            emailRequest.Headers.UserAgent.ParseAdd("rfe-auth-api");

                            using var emailResponse = await context.Backchannel.SendAsync(emailRequest, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                            if (emailResponse.IsSuccessStatusCode)
                            {
                                var emailsJson = await emailResponse.Content.ReadAsStringAsync();
                                using var emailsDoc = JsonDocument.Parse(emailsJson);
                                emailStr = emailsDoc.RootElement.EnumerateArray()
                                    .FirstOrDefault(e => e.GetProperty("verified").GetBoolean() && e.GetProperty("primary").GetBoolean())
                                    .GetProperty("email").GetString();
                            }
                        }

                        if (!string.IsNullOrEmpty(emailStr))
                        {
                            context.Identity.AddClaim(new Claim(ClaimTypes.Email, emailStr, context.Options.ClaimsIssuer));
                        }

                        if (root.TryGetProperty("avatar_url", out var avatarUrl) && avatarUrl.ValueKind != JsonValueKind.Null)
                        {
                            context.Identity.AddClaim(new Claim("urn:github:avatar", avatarUrl.GetString(), context.Options.ClaimsIssuer));
                        }
                    },
                    OnRemoteFailure = context =>
                    {
                        context.HandleResponse();
                        var clientUrl = configuration["ClientAppUrl"] ?? "https://localhost:3001";
                        context.Response.Redirect(clientUrl + "/login?error=" + Uri.EscapeDataString("Authentication cancelled"));
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                var defaultPolicy = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme,
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
                options.DefaultPolicy = defaultPolicy;
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
                    options.SetIssuer(new Uri("https://localhost:5001/"));

                    options.SetAuthorizationEndpointUris("connect/authorize")
                           .SetTokenEndpointUris("connect/token")
                           .SetUserInfoEndpointUris("connect/userinfo");

                    options.AllowAuthorizationCodeFlow()
                           .AllowRefreshTokenFlow();

                    options.RegisterScopes(
                        OpenIddict.Abstractions.OpenIddictConstants.Scopes.Email,
                        OpenIddict.Abstractions.OpenIddictConstants.Scopes.Profile,
                        OpenIddict.Abstractions.OpenIddictConstants.Scopes.OpenId
                    );

                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();

                    options.UseAspNetCore()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableTokenEndpointPassthrough()
                           .EnableUserInfoEndpointPassthrough();

                    options.RequireProofKeyForCodeExchange();

                    options.DisableAccessTokenEncryption();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                    options.Configure(o =>
                    {
                        o.TokenValidationParameters.ValidIssuers = new[] { "https://localhost:5001/", "https://rfe-auth-api:5001/" };
                    });
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
