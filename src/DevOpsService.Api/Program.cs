using DotNetEnv;
using DevOpsService.Api.Middleware;
using DevOpsService.Api.Services;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IJwtService, JwtService>();

var app = builder.Build();

app.UseMiddleware<ApiKeyMiddleware>();
app.MapControllers();

app.Run();
