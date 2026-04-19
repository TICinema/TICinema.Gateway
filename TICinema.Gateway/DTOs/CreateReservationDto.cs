namespace TICinema.Gateway.DTOs;

public record CreateReservationDto(string ScreeningId, List<SeatReservationDto> Seats);