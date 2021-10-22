using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RFE.Auth.Core.Models.Shared;

namespace RFE.Auth.API.Helpers
{
    /// <summary>
    /// custom exception middleware
    /// </summary>
    public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _logger = logger;
        _next = next;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong: {ex}");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await context.Response.WriteAsync(new ErrorDetails()
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.StackTrace
        }.ToString());
    }
}
}