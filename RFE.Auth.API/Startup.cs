using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RFE.Auth.API.Heplers;
using RFE.Auth.API.Models;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Interfaces.Repositories;
using RFE.Auth.Core.Interfaces.Repositories.Shared;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Models;
using RFE.Auth.Core.Models.Shared;
using RFE.Auth.Core.Models.User;
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
             //Add Swagger relates setting  
            
            services.AddSwaggerGen(c =>
            {
                    c.SwaggerDoc("v1", new OpenApiInfo{
                        Title = "rfe.auth.api",
                        Version = "V1"
                    });
            });
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
                services.AddScoped<IAuthUserService, UserService>();
                services.AddScoped<IEmailSender, EmailSender>();
            #endregion

            #region  Add Repositories
                services.AddScoped<IAuthUserRepository, AuthUserRepository>();
            #endregion
            
            #region  SqlServer Config Section
            
            // var  server = Configuration.GetValue<string>("SQLServerDockerConfig:SQLServer");
            // var  port = Configuration.GetValue<string>("SQLServerDockerConfig:SQLPort");
            // var  database = Configuration.GetValue<string>("SQLServerDockerConfig:SQLDatabase");
            // var  user = Configuration.GetValue<string>("SQLServerDockerConfig:SQLUser");
            // var  password = Configuration.GetValue<string>("SQLServerDockerConfig:SQLPassword");
            //services.AddDbContext<UserContext> (options => options.UseSqlServer($"Server={server},{port}; Initial Catalog={database};User ID={user};Password={password}"));
            services.AddDbContext<UserContext> (options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConection")));
            // services.Configure<SQLServerDockerConfig>(Configuration.GetSection("SQLServerDockerConfig"));
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        
            app.ConfigureExceptionHandler(logger);
            //PrepDB.PrepPopulation(app);
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSwagger();    
            app.UseSwaggerUI(c =>    
            {    
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Common Auth API");    
            }); 
        }
    }
}
