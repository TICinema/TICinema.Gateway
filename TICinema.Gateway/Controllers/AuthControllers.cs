using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Json;
using TICinema.Contracts.Protos.Identity; // Твои gRPC контракты
using TICinema.Gateway.DTOs;
using TICinema.Gateway.Extensions;
using TICinema.Gateway.Interfaces;
using static TICinema.Gateway.DTOs.TelegramDto;

namespace TICinema.Gateway.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IIdentityService identityService) : ControllerBase
{
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequestDto dto)
        => Ok(await identityService.SendOtpAsync(dto));

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto)
    {
        var result = await identityService.VerifyOtpAsync(dto);
        Response.SetRefreshTokenCookie(result.RefreshToken);
        return Ok(new { accessToken = result.AccessToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var token))
            return Unauthorized();

        var result = await identityService.RefreshAsync(token);
        Response.SetRefreshTokenCookie(result.RefreshToken);
        return Ok(new { accessToken = result.AccessToken });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.DeleteRefreshTokenCookie();
        return Ok(new { message = "Вышли" });
    }

    [HttpGet("telegram")]
    public async Task<IActionResult> InitTelegram()
        => Ok(new { url = await identityService.GetTelegramAuthUrlAsync() });

    [HttpPost("telegram/verify")]
    public async Task<IActionResult> TelegramVerify([FromBody] TelegramVerifyRequestDto dto)
    {
        var result = await identityService.TelegramVerifyAsync(dto);

        // Случай 1: Нам вернули URL для бота (нужна доп. регистрация)
        if (result.ResultCase == TelegramVerifyResponse.ResultOneofCase.Url)
        {
            return Ok(new { url = result.Url });
        }

        // Случай 2: Пользователь сразу залогинился (токены на месте)
        if (!string.IsNullOrEmpty(result.AccessToken) && !string.IsNullOrEmpty(result.RefreshToken))
        {
            // Работа с куками остается в контроллере, так как это часть HTTP-слоя
            Response.SetRefreshTokenCookie(result.RefreshToken);
            return Ok(new { accessToken = result.AccessToken });
        }

        return BadRequest("Ошибка авторизации: неверные данные или истекшая сессия");
    }

    [HttpPost("telegram/finalize")]
    public async Task<IActionResult> FinalizeTelegram(TelegramFinalizeRequestDto dto)
    {
        var tokens = await identityService.TelegramConsumeAsync(new TelegramConsumeRequest()
        {
            SessionId = dto.SessionId,
        });
        
        Response.SetRefreshTokenCookie(tokens.RefreshToken);
        
        return Ok(new { accessToken = tokens.AccessToken });
    }
}