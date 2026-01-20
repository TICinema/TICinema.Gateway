using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using TICinema.Contracts.Protos.Identity; // Твои gRPC контракты
using TICinema.Gateway.DTOs;

namespace TICinema.Gateway.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService.AuthServiceClient _grpcClient;

    public AuthController(AuthService.AuthServiceClient grpcClient)
    {
        _grpcClient = grpcClient;
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequestDto dto)
    {
        // 1. Формируем gRPC запрос
        var grpcRequest = new SendOtpRequest
        {
            Identifier = dto.Identifier,
            Type = dto.Type
        };

        // 2. Вызываем Identity Service
        // Если будет ошибка, Middleware её поймает, try-catch не нужен
        var response = await _grpcClient.SendOtpAsync(grpcRequest);

        return Ok(response);
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto)
    {
        var grpcRequest = new VerifyOtpRequest
        {
            Identifier = dto.Identifier,
            Type = dto.Type,
            Code = dto.Code
        };

        var response = await _grpcClient.VerifyOtpAsync(grpcRequest);

        // 3. Настраиваем Cookie для Refresh Token
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Важно: в продакшене только HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        // Используем свойство Response контроллера
        Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);

        // 4. Возвращаем только AccessToken
        return Ok(new { accessToken = response.AccessToken });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "Вышли из системы" });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        // 1. Пытаемся достать рефреш-токен из куки
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return Unauthorized(new { message = "Refresh token не найден" });
        }

        // 2. Делаем запрос в Identity Service
        var response = await _grpcClient.RefreshAsync(new Contracts.Protos.Identity.RefreshRequest { RefreshToken = refreshToken });

        // 3. Обновляем куку
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);

        // 4. Возвращаем новый Access Token
        return Ok(new { accessToken = response.AccessToken });
    }

    [HttpGet("me")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAccount()
    {
        var userId = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return await Task.FromResult(Ok(new { Message = $"Привет, твой ID: {userId}" }));
    }
}