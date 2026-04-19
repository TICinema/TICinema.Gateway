using TICinema.Contracts.Protos.Media;

namespace TICinema.Gateway.Interfaces
{
    public interface IMediaGatewayService
    {
        Task<UploadResponse> UploadAvatarAsync(IFormFile file, string userId);
    }
}
