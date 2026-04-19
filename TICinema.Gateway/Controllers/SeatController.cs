using Microsoft.AspNetCore.Mvc;
using TICinema.Contracts.Protos.Seat;

namespace TICinema.Gateway.Controllers;

[ApiController]
[Route("api/seats")] // Базовый путь контроллера
public class SeatController(SeatService.SeatServiceClient seatClient) : ControllerBase
{
    private readonly SeatService.SeatServiceClient _seatClient = seatClient;

    [HttpGet("{hallId}/{screeningId}")]
    public async Task<IActionResult> ListsByHall(string hallId, string screeningId)
    {
        // 1. Собираем параметры в gRPC-запрос
        var request = new ListSeatsRequest 
        { 
            HallId = hallId,
            ScreeningId = screeningId
        };

        var response = await _seatClient.ListSeatsByHallAsync(request);

        return Ok(response.Seat); 
    }
}