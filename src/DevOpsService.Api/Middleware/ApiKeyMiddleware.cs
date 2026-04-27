using Microsoft.Extensions.Primitives;

namespace DevOpsService.Api.Middleware
{
    internal class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        private readonly RequestDelegate _next = next;
        private const string _apiKeyHeader = "X-Parse-REST-API-Key";
        private const string _jwtHeader = "X-JWT-KWY";
        private readonly string _apiKey = configuration["ApiKey"] ?? string.Empty;

        public async Task InvokeAsync(HttpContext context, Service.IJwtService jwtService)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(jwtService);
            // ApiKeyMiddleware.cs — add at the top of InvokeAsync
            if (context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context).ConfigureAwait(false);
                return;
            }

            if (context.Request.Method != HttpMethods.Post)
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                await context.Response.WriteAsync("ERROR").ConfigureAwait(false);
                return;
            }


            if (!context.Request.Headers.TryGetValue(_apiKeyHeader, out StringValues apiKey)
                || apiKey != _apiKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }


            if (!context.Request.Headers.TryGetValue(_jwtHeader, out StringValues jwt)
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