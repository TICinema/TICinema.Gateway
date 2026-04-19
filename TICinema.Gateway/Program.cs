using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TICinema.Contracts.Protos.Category;
using TICinema.Contracts.Protos.Identity;
using TICinema.Contracts.Protos.Users;
using TICinema.Gateway.Interfaces;
using TICinema.Gateway.Middleware;
using TICinema.Gateway.Services;
using TICinema.Contracts.Protos.Media;
using TICinema.Contracts.Protos.Movie;
using TICinema.Contracts.Protos.Theater;
using TICinema.Gateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IMediaGatewayService, MediaGatewayService>();

builder.Services.AddGrpcClients(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Gateaway-Service"))
        .AddAspNetCoreInstrumentation()
        .AddGrpcClientInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRedisInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = new Uri("http://jaeger:4317")));

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Для разработки (локалхост) можно false
        options.SaveToken = true;

        options.TokenValidationParameters.RoleClaimType = "role";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),

            // Важно: по умолчанию .NET дает токенам +5 минут жизни "на всякий случай". 
            // ZeroClockSkew убирает эту задержку, чтобы токен протухал ровно вовремя.
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseRouting();

app.UseMiddleware<GrpcExceptionMiddleware>();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpMetrics();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapScalarApiReference(); 
    app.MapOpenApi();
}

app.UseMetricServer();
app.MapControllers();

app.Run();