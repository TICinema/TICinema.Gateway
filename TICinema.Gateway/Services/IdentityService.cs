using System.Text;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.WebUtilities;
using TICinema.Contracts.Protos.Identity;
using TICinema.Gateway.DTOs;
using TICinema.Gateway.Interfaces;

namespace TICinema.Gateway.Services
{
    public class IdentityService(AuthService.AuthServiceClient authClient) : IIdentityService
    {
        public async Task<SendOtpResponse> SendOtpAsync(SendOtpRequestDto dto)
        {
            var request = new SendOtpRequest
            {
                Identifier = dto.Identifier,
                Type = dto.Type
            };
            return await authClient.SendOtpAsync(request);
        }

        public async Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequestDto dto)
        {
            var request = new VerifyOtpRequest
            {
                Identifier = dto.Identifier,
                Type = dto.Type,
                Code = dto.Code
            };
            return await authClient.VerifyOtpAsync(request);
        }

        public async Task<RefreshResponse> RefreshAsync(string refreshToken)
        {
            var request = new RefreshRequest { RefreshToken = refreshToken };
            return await authClient.RefreshAsync(request);
        }

        public async Task<string> GetTelegramAuthUrlAsync()
        {
            var response = await authClient.TelegramInitAsync(new Empty());
            return response.Url;
        }
        
        public async Task<TelegramVerifyResponse> TelegramVerifyAsync(TelegramDto.TelegramVerifyRequestDto dto)
        {
            // 1. Декодируем Base64
            byte[] jsonBytes = WebEncoders.Base64UrlDecode(dto.TgAuthResult);
            string jsonString = Encoding.UTF8.GetString(jsonBytes);

            // 2. Парсим данные
            var rawQuery = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Неверный формат данных Telegram");

            // 3. Формируем gRPC запрос
            var request = new TelegramVerifyRequest();
            foreach (var (key, value) in rawQuery)
            {
                request.Query.Add(key, value?.ToString() ?? string.Empty);
            }

            // 4. Вызываем Identity Service по gRPC
            return await authClient.TelegramVerifyAsync(request);
        }

        public async Task<TelegramVerifyResponse> TelegramVerifyAsync(Dictionary<string, string> query)
        {
            var request = new TelegramVerifyRequest();

            foreach (var (key, value) in query)
            {
                request.Query.Add(key, value ?? string.Empty);
            }

            return await authClient.TelegramVerifyAsync(request);
        }

        public async Task<TelegramConsumeResponse> TelegramConsumeAsync(TelegramConsumeRequest request)
        {
            return await authClient.TelegramConsumeAsync(request);
        }
    }
}
