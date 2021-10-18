using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RFE.Auth.API.Models;
using RFE.Auth.API.Models.User;
using RFE.Auth.Core.Interfaces.Services;
using RFE.Auth.Core.Services;

namespace RFE.Auth.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        private const string DefaultAssemblyNamesPrefix = "RFE";
        public IConfiguration Configuration { get; }protected virtual string AssemblyNamesPrefix
        {
            get
            {
                return DefaultAssemblyNamesPrefix;
            }
        }

        protected virtual IEnumerable<Assembly> ProjectAssemblies
        {
            get
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(m => m.GetName().Name.StartsWith(AssemblyNamesPrefix, true, CultureInfo.InvariantCulture));
                return assemblies;
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
             //Add Swagger relates setting  
            services.AddSwaggerGen();
            #region  Add Services
                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped<IUserService, UserService>();
            #endregion
            
            #region  SqlServer Config Section
            
            var  server = Configuration.GetValue<string>("SQLServerDockerConfig:SQLServer");
            var  port = Configuration.GetValue<string>("SQLServerDockerConfig:SQLPort");
            var  database = Configuration.GetValue<string>("SQLServerDockerConfig:SQLDatabase");
            var  user = Configuration.GetValue<string>("SQLServerDockerConfig:SQLUser");
            var  password = Configuration.GetValue<string>("SQLServerDockerConfig:SQLPassword");
            services.AddDbContext<UserContext> (options => options.UseSqlServer($"Server={server},{port}; Initial Catalog={database};User ID={user};Password={password}"));
            services.Configure<SQLServerDockerConfig>(Configuration.GetSection("SQLServerDockerConfig"));
            #endregion

            // Auto Mapper Configurations
            services.AddAutoMapper(ProjectAssemblies);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        
            PrepDB.PrepPopulation(app);
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
