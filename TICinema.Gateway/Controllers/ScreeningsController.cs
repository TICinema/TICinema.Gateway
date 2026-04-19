using Microsoft.AspNetCore.Mvc;
using TICinema.Contracts.Protos.Screening;

namespace TICinema.Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreeningsController(ScreeningService.ScreeningServiceClient screeningClient) : ControllerBase
{
    private readonly ScreeningService.ScreeningServiceClient _screeningClient = screeningClient;

    // 1. Создание сеанса: POST /api/screenings
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateScreeningRequest request)
    {
        // Просто перенаправляем запрос в gRPC сервис
        var response = await _screeningClient.CreateScreeningAsync(request);
        
        if (!response.Ok)
            return BadRequest(new { message = "Не удалось создать сеанс (возможно, наложение по времени)" });

        return Ok(response);
    }

    // 2. Список сеансов: GET /api/screenings?date=2026-04-22&theaterId=...
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] string? date, [FromQuery] string? theaterId)
    {
        var request = new GetScreeningsRequest();
        
        if (!string.IsNullOrEmpty(date)) request.Date = date;
        if (!string.IsNullOrEmpty(theaterId)) request.TheaterId = theaterId;

        var response = await _screeningClient.GetScreeningsAsync(request);
        return Ok(response.Screening);
    }

    // 3. Сеансы для фильма: GET /api/screenings/movie/guid?date=...
    [HttpGet("movie/{movieId}")]
    public async Task<IActionResult> GetByMovie(string movieId, [FromQuery] string? date)
    {
        var request = new GetScreeningsByMovieRequest { MovieId = movieId };
        if (!string.IsNullOrEmpty(date)) request.Date = date;

        var response = await _screeningClient.GetScreeningsByMovieAsync(request);
        return Ok(response.Screenings);
    }

    // 4. Один сеанс: GET /api/screenings/guid
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var response = await _screeningClient.GetScreeningAsync(new GetScreeningRequest { Id = id });
            return Ok(response.Screening);
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return NotFound(new { message = ex.Status.Detail });
        }
    }
}