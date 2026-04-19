using TICinema.Contracts.Protos.Media;
using TICinema.Gateway.Interfaces;

namespace TICinema.Gateway.Services
{
    public class MediaGatewayService(MediaService.MediaServiceClient mediaClient) : IMediaGatewayService
    {
        public async Task<UploadResponse> UploadAvatarAsync(IFormFile file, string userId)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var request = new UploadRequest
            {
                FileName = $"{userId}_{DateTime.UtcNow.Ticks}{Path.GetExtension(file.FileName)}",
                Folder = "avatars",
                ContentType = file.ContentType,
                Data = Google.Protobuf.ByteString.CopyFrom(ms.ToArray())
            };

            var response = await mediaClient.UploadAsync(request);
            
            return response; 
        }
    }
}
