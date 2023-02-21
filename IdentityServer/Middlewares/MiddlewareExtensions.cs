using Microsoft.AspNetCore.Builder;
using IdentityServer.Middlewares;

namespace IdentityServer.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseSerilogMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SerilogMiddleware>();
        }

        public static IApplicationBuilder UseGeneralExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GeneralExceptionMiddleware>();
        }
    }
}
