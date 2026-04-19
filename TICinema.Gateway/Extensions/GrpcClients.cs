using TICinema.Contracts.Protos.Booking;
using TICinema.Contracts.Protos.Category;
using TICinema.Contracts.Protos.Hall;
using TICinema.Contracts.Protos.Identity;
using TICinema.Contracts.Protos.Media;
using TICinema.Contracts.Protos.Movie;
using TICinema.Contracts.Protos.Payment;
using TICinema.Contracts.Protos.Screening;
using TICinema.Contracts.Protos.Seat;
using TICinema.Contracts.Protos.Theater;
using TICinema.Contracts.Protos.Users;

namespace TICinema.Gateway.Extensions;

public static class GrpcClients
{
    public static IServiceCollection AddGrpcClients(this IServiceCollection services, IConfiguration configuration)
    {
        // Выносим общую логику настройки в Action, чтобы не дублировать код
        Action<IServiceProvider, Grpc.Net.ClientFactory.GrpcClientFactoryOptions> ConfigureClient(string configKey) => 
            (sp, o) => {
                var url = configuration[$"ServiceUrls:{configKey}"];
                if (string.IsNullOrEmpty(url)) throw new Exception($"URL для сервиса {configKey} не найден в конфигурации!");
                o.Address = new Uri(url);
            };

        // Регистрация Identity клиентов
        services.AddGrpcClientWithDefaultConfig<AuthService.AuthServiceClient>(ConfigureClient("Identity"));
        services.AddGrpcClientWithDefaultConfig<AccountService.AccountServiceClient>(ConfigureClient("Identity"));

        // Регистрация Movie клиентов
        services.AddGrpcClientWithDefaultConfig<MovieService.MovieServiceClient>(ConfigureClient("Movie"));
        services.AddGrpcClientWithDefaultConfig<CategoryService.CategoryServiceClient>(ConfigureClient("Movie"));

        // Регистрация остальных
        services.AddGrpcClientWithDefaultConfig<UsersService.UsersServiceClient>(ConfigureClient("Users"));
        services.AddGrpcClientWithDefaultConfig<MediaService.MediaServiceClient>(ConfigureClient("Media"));
        services.AddGrpcClientWithDefaultConfig<TheaterService.TheaterServiceClient>(ConfigureClient("Theater"));
        services.AddGrpcClientWithDefaultConfig<HallService.HallServiceClient>(ConfigureClient("Theater"));
        services.AddGrpcClientWithDefaultConfig<SeatService.SeatServiceClient>(ConfigureClient("Theater"));
        
        services.AddGrpcClientWithDefaultConfig<ScreeningService.ScreeningServiceClient>(ConfigureClient("Screening"));
        services.AddGrpcClientWithDefaultConfig<BookingService.BookingServiceClient>(ConfigureClient("Booking"));
        services.AddGrpcClient<PaymentService.PaymentServiceClient>(ConfigureClient("Payment"));
        
        return services;
    }

    // Вспомогательный метод, чтобы не писать SocketsHttpHandler для каждого клиента
    private static IHttpClientBuilder AddGrpcClientWithDefaultConfig<TClient>(
        this IServiceCollection services, 
        Action<IServiceProvider, Grpc.Net.ClientFactory.GrpcClientFactoryOptions> configureClient) 
        where TClient : class
    {
        return services.AddGrpcClient<TClient>(configureClient)
            .ConfigureChannel(o =>
            {
                o.HttpVersion = System.Net.HttpVersion.Version20;
                o.HttpVersionPolicy = HttpVersionPolicy.RequestVersionExact;
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            });
    }
}