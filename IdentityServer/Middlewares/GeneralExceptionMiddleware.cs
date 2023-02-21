using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer.Middlewares
{
    public class GeneralExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GeneralExceptionMiddleware> _logger;

        public GeneralExceptionMiddleware(RequestDelegate next, ILogger<GeneralExceptionMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context.Response, ex, HttpStatusCode.InternalServerError);
            }
        }

        private Task HandleException(HttpResponse response, Exception exception, HttpStatusCode statusCode)
        {
            _logger.LogError(exception, exception.Message);
            response.StatusCode = (int)statusCode;

            return Task.CompletedTask;
        }
    }
}
