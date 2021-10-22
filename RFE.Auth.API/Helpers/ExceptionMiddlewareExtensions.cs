using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RFE.Auth.Core.Models.Shared;

namespace RFE.Auth.API.Heplers
{
    /// <summary>
    /// ExceptionMiddlewareExtensions
    /// </summary>
    public static class ExceptionMiddlewareExtensions
    {
        /// <summary>
        /// ConfigureExceptionHandler
        /// </summary>
        /// <param name="app"></param>
        /// <param name="logger"></param>
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature!=null)
                    {
                        logger.LogError($"Something Went Wrong:{contextFeature.Error}");

                        await context.Response.WriteAsync(new ErrorDetails
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = "Internal Server Error"
                        }.ToString());
                    }
                });
            });
        }
    }
}