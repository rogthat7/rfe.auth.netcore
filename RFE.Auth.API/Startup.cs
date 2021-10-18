using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace RFE.Auth.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
             //Add Swagger relates setting  
            services.AddSwaggerGen();
            
            #region  SqlServer Config Section
            
            var  server = Configuration.GetValue<string>("SQLServerDockerConfig:SQLServer");
            var  port = Configuration.GetValue<string>("SQLServerDockerConfig:SQLPort");
            var  database = Configuration.GetValue<string>("SQLServerDockerConfig:SQLDatabase");
            var  user = Configuration.GetValue<string>("SQLServerDockerConfig:SQLUser");
            var  password = Configuration.GetValue<string>("SQLServerDockerConfig:SQLPassword");
            services.AddDbContext<UserContext> (options => options.UseSqlServer($"Server={server},{port}; Initial Catalog={database};User ID={user};Password={password}"));
            services.Configure<SQLServerDockerConfig>(Configuration.GetSection("SQLServerDockerConfig"));
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
