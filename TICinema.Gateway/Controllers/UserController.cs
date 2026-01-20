using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TICinema.Contracts.Protos.Identity;
using TICinema.Gateway.Models.DTOs;

namespace TICinema.Gateway.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController(AccountService.AccountServiceClient accountClient) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var response = await accountClient.GetAccountAsync(new GetAccountRequest { Id = userId });
        return Ok(response);
    }

    // --- СМЕНА EMAIL ---

    [HttpPost("change-email/init")]
    public async Task<IActionResult> InitEmailChange([FromBody] InitChangeDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        var result = await accountClient.InitEmailChangeAsync(new InitEmailChangeRequest 
        { 
            UserId = userId, 
            Email = dto.Value 
        });

        return result.Ok ? Ok(new { message = "Код отправлен на новую почту" }) : BadRequest();
    }

    [HttpPost("change-email/confirm")]
    public async Task<IActionResult> ConfirmEmailChange([FromBody] ConfirmChangeDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var result = await accountClient.ConfirmEmailChangeAsync(new ConfirmEmailChangeRequest 
        { 
            UserId = userId, 
            Email = dto.Value, 
            Code = dto.Code 
        });

        return result.Ok ? Ok(new { message = "Email успешно изменен" }) : BadRequest();
    }

    // --- СМЕНА ТЕЛЕФОНА ---

    [HttpPost("change-phone/init")]
    public async Task<IActionResult> InitPhoneChange([FromBody] InitChangeDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        var result = await accountClient.InitPhoneChangeAsync(new InitPhoneChangeRequest 
        { 
            UserId = userId, 
            Phone = dto.Value 
        });

        return result.Ok ? Ok(new { message = "Код отправлен на новый номер" }) : BadRequest();
    }

    [HttpPost("change-phone/confirm")]
    public async Task<IActionResult> ConfirmPhoneChange([FromBody] ConfirmChangeDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var result = await accountClient.ConfirmPhoneChangeAsync(new ConfirmPhoneChangeRequest 
        { 
            UserId = userId, 
            Phone = dto.Value, 
            Code = dto.Code 
        });

        return result.Ok ? Ok(new { message = "Телефон успешно изменен" }) : BadRequest();
    }
}