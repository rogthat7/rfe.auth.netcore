using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using RFE.Auth.API.Helpers;
using RFE.Auth.API.Heplers;
using RFE.Auth.API.Models;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Interfaces.Repositories;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Interfaces.Shared;
using RFE.Auth.Core.Models.Shared;
using RFE.Auth.Core.Services;
using RFE.Auth.Infrastructure.Repositories;

namespace RFE.Auth.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        private const string DefaultAssemblyNamesPrefix = "RFE";
        public IConfiguration Configuration { get; }
        protected virtual string AssemblyNamesPrefix
        {
            get
            {
                return DefaultAssemblyNamesPrefix;
            }
        }
        /// <summary>
        /// gets all assemblies
        /// </summary>
        /// <value></value>
        protected virtual IEnumerable<Assembly> ProjectAssemblies
        {
            get
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(m => m.GetName().Name.StartsWith(AssemblyNamesPrefix, true, CultureInfo.InvariantCulture));
                return assemblies;
            }
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddLogging();
            services.Configure<CustomOptions>(Configuration.GetSection("CustomOptions"));
            services.Configure<JwtOptions>(Configuration.GetSection("JwtConfig"));
            services.Configure<ApiInfo>(Configuration.GetSection("ApiInfo"));

            // This method gets called by the runtime. Use this method to add services to the container.
            services.AddMvc(options => {
                options.EnableEndpointRouting = false;
            });
            // services.AddApiVersioning(options =>
            // {
            //     options.ReportApiVersions = true;
            //     options.AssumeDefaultVersionWhenUnspecified = true;
            // });

            // services.AddSingleton<IConfigureOptions<ApiVersioningOptions>, ConfigureApiVersioningOptions>();


            #region  JwtAuth Configuration
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x => {
                x.RequireHttpsMetadata = false;
                x.SaveToken  = true;
                x.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey  = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetValue<string>("JwtConfig:Secret"))),
                    ValidateIssuer = false, // doubtful this should be false
                    ValidateAudience = false, // doubtful this should be false
                    RequireExpirationTime = false,
                    ValidateLifetime = true,
                    ValidIssuer =  Configuration["JwtConfig:Issuer"]
                };
            });
            

            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "V1",
                    Title = "rfe.auth.api",
                    Description="ASP.NET Core 3.1 Web API" 
                });
                    // To Enable authorization using Swagger (JWT)  
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
            #endregion
            services.AddScoped<IUnitOfWork, UnitOfWork>(serviceProvider =>
            {
                var connectionString = Configuration.GetConnectionString("DefaultConection");
                return new UnitOfWork(connectionString);
            });

            #region Test Email Set Up
            var emailConfig = Configuration
                .GetSection("EmailConfiguration")
                .Get<EmailConfiguration>();
            services.AddSingleton(emailConfig);
            #endregion

            #region  Add Services
                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IJwtAuthenticationService, JwtAuthenticationService>();
                services.AddScoped<IEmailSender, EmailSender>();
            #endregion

            #region  Add Repositories
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IAuthRepository, AuthRepository>();
            #endregion
            
            #region  SqlServer DBContext Section
            
            services.AddDbContext<UserContext> (options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConection")));
            
            #endregion

            // Auto Mapper Configurations
            services.AddAutoMapper(ProjectAssemblies);
            services.AddControllers().AddNewtonsoftJson();
        }
        /// <summary>
        /// // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, IOptions<ApiInfo> _apiInfo)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMiddleware<ExceptionMiddleware>();
            }
        
            else app.ConfigureExceptionHandler(logger);
            //PrepDB.PrepPopulation(app);
            //app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMvc();

            app.UseSwagger();    
            app.UseSwaggerUI(c =>    
            {    
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");    
            }); 
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context => {
                   await context.Response.WriteAsync(JsonConvert.SerializeObject(new ApiInfo{
                        apiName = _apiInfo.Value.apiName,
                        basePath = context.Request.Scheme+"://"+context.Request.Host + _apiInfo.Value.basePath,
                        apiDocumentation = context.Request.Scheme+"://"+context.Request.Host + _apiInfo.Value.apiDocumentation,
                        version = _apiInfo.Value.version
                   }));
                });
            });
        }
    }
}
