using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RFE.Auth.API.Helpers
{
    public class ConfigureApiVersioningOptions : IConfigureOptions<ApiVersioningOptions>
    {
        private readonly IServiceProvider _serviceProvider;

        public ConfigureApiVersioningOptions(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public void Configure(ApiVersioningOptions options)
        {
            var apiVersion = _serviceProvider.GetRequiredService<IConfiguration>().GetSection("ApiInfo:version").Value;
            options.DefaultApiVersion = ApiVersion.Parse(apiVersion);
        }
    }

}