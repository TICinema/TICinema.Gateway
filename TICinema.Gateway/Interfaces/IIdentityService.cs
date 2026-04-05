using TICinema.Contracts.Protos.Identity;
using TICinema.Gateway.DTOs;

namespace TICinema.Gateway.Interfaces
{
    public interface IIdentityService
    {
        Task<SendOtpResponse> SendOtpAsync(SendOtpRequestDto dto);
        Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequestDto dto);
        Task<RefreshResponse> RefreshAsync(string refreshToken);
        Task<string> GetTelegramAuthUrlAsync();
        Task<TelegramVerifyResponse> TelegramVerifyAsync(Dictionary<string, string> query);
        Task<TelegramConsumeResponse> TelegramConsumeAsync(TelegramConsumeRequest request);
        Task<TelegramVerifyResponse> TelegramVerifyAsync(TelegramDto.TelegramVerifyRequestDto dto);
    }
}
//eyJpZCI6NjMyMDE2MTI1OCwiZmlyc3RfbmFtZSI6IlNvbG9tb24iLCJwaG90b191cmwiOiJodHRwczpcL1wvdC5tZVwvaVwvdXNlcnBpY1wvMzIwXC84UFpocVJWdWlGV1RTM0Foa1ctOEhfX0V4enVwRTBqcklpYXdsaTNwanUxMGxPTEI1UDR1Q3c5NWhnR28yVVZTLmpwZyIsImF1dGhfZGF0ZSI6MTc3NTI0NjA1OSwiaGFzaCI6IjE4YjE3NDk4MWMzMDQyODcxM2VmMWEwNDZkOTkyOTFiYjI2NWI0MjY1NzYzMjE1YjhkNjcwMTcyOGQ4NjgyY2EifQ