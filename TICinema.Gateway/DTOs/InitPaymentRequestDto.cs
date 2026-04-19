namespace TICinema.Gateway.DTOs;

public class InitPaymentRequestDto
{
    public Guid ScreeningId { get; set; }
    public IEnumerable<SeatDto> Seats { get; set; }
    public bool SaveMethodMethod { get; set; }
    public string PaymentMethodId { get; set; } = null!;
}