using System.Security.Claims;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TICinema.Contracts.Protos.Users;
using TICinema.Gateway.DTOs;

namespace TICinema.Gateway.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(UsersService.UsersServiceClient usersClient) : ControllerBase
{
    [Authorize] // Метод доступен только с валидным JWT
    [HttpGet("@me")]
    public async Task<IActionResult> GetMe()
    {
        // 1. Извлекаем ID пользователя из Claims токена
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        try
        {
            // 2. Делаем gRPC запрос в User Service
            var response = await usersClient.GetMeAsync(new GetMeRequest { Id = userId });

            // 3. Возвращаем данные профиля фронтенду
            return Ok(response.User);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return NotFound("Profile not found");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal error: {ex.Message}");
        }
    }

    [Authorize]
    [HttpPatch("@me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var request = new PatchUserRequest
        {
            UserId = userId,
            Name = dto.Name // gRPC сам поймет, что поле заполнено
        };

        var response = await usersClient.PatchUserAsync(request);

        return Ok(new { success = response.Ok });
    }
}