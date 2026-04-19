using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grpc.Core;
using TICinema.Contracts.Protos.Booking;
using TICinema.Gateway.DTOs;
using System.Security.Claims;

namespace TICinema.Gateway.Controllers;

[Authorize] // Фронтенд должен присылать JWT токен
[ApiController]
[Route("api/[controller]")]
public class BookingController(BookingService.BookingServiceClient bookingClient) : ControllerBase
{
    // 1. Создание бронирования
    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveSeats([FromBody] CreateReservationDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var grpcRequest = new CreateReservationRequest
        {
            UserId = userId,
            ScreeningId = request.ScreeningId
        };
        
        grpcRequest.Seats.AddRange(request.Seats.Select(s => new SeatInput 
        { 
            SeatId = s.SeatId, 
            Price = s.Price 
        }));

        try
        {
            var response = await bookingClient.CreateReservationAsync(grpcRequest);
            return Ok(response);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
        {
            return Conflict(new { message = ex.Status.Detail });
        }
        catch (RpcException ex)
        {
            return StatusCode(500, new { message = "Booking service error", details = ex.Status.Detail });
        }
    }

    // 2. Получение истории бронирований пользователя
    [HttpGet("my-bookings")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var response = await bookingClient.GetUserBookingsAsync(new GetUserBookingsRequest 
        { 
            UserId = userId 
        });

        return Ok(response.Bookings);
    }

    // 3. Получение списка занятых мест (публичный метод, можно без Authorize)
    [AllowAnonymous]
    [HttpGet("reserved-seats/{screeningId}")]
    public async Task<IActionResult> GetReservedSeats(string screeningId, [FromQuery] string hallId)
    {
        var response = await bookingClient.ListReservedSeatsAsync(new ListReservedSeatsRequest
        {
            ScreeningId = screeningId,
            HallId = hallId
        });

        return Ok(response.ReservedSeatIds);
    }

    // 4. Отмена бронирования
    [HttpDelete("cancel/{bookingId}")]
    public async Task<IActionResult> CancelBooking(string bookingId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await bookingClient.CancelBookingAsync(new CancelBookingRequest
        {
            BookingId = bookingId,
            UserId = userId
        });

        return NoContent();
    }
}