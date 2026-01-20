namespace TICinema.Gateway.DTOs
{
    public record SendOtpRequestDto(string Identifier, string Type);
    public record VerifyOtpRequestDto(string Identifier, string Type, string Code);
}
