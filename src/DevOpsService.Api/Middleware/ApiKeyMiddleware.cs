namespace DevOpsService.Api.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeader = "X-Parse-REST-API-Key";
    private const string JwtHeader = "X-JWT-KWY";
    private readonly string _apiKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _apiKey = configuration["ApiKey"] ?? string.Empty;
    }

    public async Task InvokeAsync(HttpContext context, Services.IJwtService jwtService)
    {
        
        if (context.Request.Method != HttpMethods.Post)
        {
            context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            await context.Response.WriteAsync("ERROR");
            return;
        }

        
        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKey) 
            || apiKey != _apiKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        
        if (!context.Request.Headers.TryGetValue(JwtHeader, out var jwt) 
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
        await _next(context);
    }
}