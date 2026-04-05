using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using TICinema.Contracts.Protos.Identity;
using TICinema.Gateway.Interfaces;
using TICinema.Gateway.Middleware;
using TICinema.Gateway.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["ServiceUrls:Identity"]!);
});

builder.Services.AddGrpcClient<AccountService.AccountServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["ServiceUrls:Identity"]!);
});

// Настройка CORS (оставляем как было)
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

// 1. Получаем настройки
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

// Это заставит .NET не переименовывать claim-ы из JWT
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// 2. Добавляем Аутентификацию (Authentication)
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

builder.Services.AddAuthorization(); // Добавляем Авторизацию

var app = builder.Build();

app.UseMiddleware<GrpcExceptionMiddleware>();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.MapControllers();

app.Run();