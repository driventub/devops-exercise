using Microsoft.Extensions.Primitives;

namespace DevOpsService.Api.Middleware
{
    public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        private readonly RequestDelegate _next = next;
        private const string ApiKeyHeader = "X-Parse-REST-API-Key";
        private const string JwtHeader = "X-JWT-KWY";
        private readonly string _apiKey = configuration["ApiKey"] ?? string.Empty;

        public async Task InvokeAsync(HttpContext context, Service.IJwtService jwtService)
        {

            if (context.Request.Method != HttpMethods.Post)
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                await context.Response.WriteAsync("ERROR").ConfigureAwait(false);
                return;
            }


            if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out StringValues apiKey)
                || apiKey != _apiKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }


            if (!context.Request.Headers.TryGetValue(JwtHeader, out StringValues jwt)
                || string.IsNullOrWhiteSpace(jwt))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }


            if (!jwtService.IsJwtUnique(jwt!))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            jwtService.MarkJwtAsUsed(jwt!);
            await _next(context).ConfigureAwait(false);
        }
    }
}