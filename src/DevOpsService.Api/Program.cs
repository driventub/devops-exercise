using DotNetEnv;
using DevOpsService.Api.Middleware;
using DevOpsService.Api.Service;

Env.Load();
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IJwtService, JwtService>();

WebApplication app = builder.Build();

app.UseMiddleware<ApiKeyMiddleware>();
app.MapControllers();

app.Run();
