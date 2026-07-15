using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using RFE.Auth.API.Helpers;
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
using Scalar.AspNetCore;
using Newtonsoft.Json;
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
            services.AddControllers()
                    .AddNewtonsoftJson();

            services.AddLogging();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Cleaned-up configuration using custom extension methods
            services.AddCustomOptions(Configuration)
                    .AddJwtAndGoogleAuthentication(Configuration)
                    .AddOpenIddictServer()
                    .AddSwaggerAndScalar(Assembly.GetExecutingAssembly().GetName().Name, AppContext.BaseDirectory)
                    .AddAppServicesAndRepositories(Configuration);

            services.AddAutoMapper(ProjectAssemblies);
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
            PrepDB.PrepPopulation(app);
            //app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();    
            app.UseSwaggerUI(c =>    
            {    
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");    
            }); 
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapScalarApiReference(options =>
                {
                    options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
                    try
                    {
                        var scalarAuthHtmlPath = Path.Combine(AppContext.BaseDirectory, "Resources", "scalar-auth.html");
                        if (File.Exists(scalarAuthHtmlPath))
                        {
                            var customAuthHtml = File.ReadAllText(scalarAuthHtmlPath);
                            options.AddHeaderContent(customAuthHtml);
                        }
                    }
                    catch (Exception)
                    {
                        // Graceful fallback to avoid crashing startup
                    }
                });
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
