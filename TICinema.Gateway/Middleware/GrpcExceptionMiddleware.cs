using Grpc.Core;
using System.Net;
using System.Text.Json;

namespace TICinema.Gateway.Middleware;

public class GrpcExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GrpcExceptionMiddleware> _logger;

    public GrpcExceptionMiddleware(RequestDelegate next, ILogger<GrpcExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (RpcException ex)
        {
            await HandleRpcExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleGenericExceptionAsync(context, ex);
        }
    }

    private static Task HandleRpcExceptionAsync(HttpContext context, RpcException ex)
    {
        // Маппинг gRPC статусов в HTTP статусы
        var (httpStatusCode, message) = ex.StatusCode switch
        {
            StatusCode.InvalidArgument => (HttpStatusCode.BadRequest, ex.Status.Detail),

            // Для 401 тоже лучше брать ex.Status.Detail, а не хардкодить текст
            StatusCode.Unauthenticated => (HttpStatusCode.Unauthorized, ex.Status.Detail ?? "Вы не авторизованы"),

            StatusCode.NotFound => (HttpStatusCode.NotFound, ex.Status.Detail),
            StatusCode.PermissionDenied => (HttpStatusCode.Forbidden, "Доступ запрещен"),
            StatusCode.DeadlineExceeded => (HttpStatusCode.GatewayTimeout, "Сервис не ответил вовремя"),
            _ => (HttpStatusCode.InternalServerError, "Внутренняя ошибка микросервиса")
        };

        return WriteErrorResponse(context, httpStatusCode, message);
    }

    private static Task HandleGenericExceptionAsync(HttpContext context, Exception ex)
    {
        return WriteErrorResponse(context, HttpStatusCode.InternalServerError, "Internal Server Error");
    }

    private static Task WriteErrorResponse(HttpContext context, HttpStatusCode code, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var result = JsonSerializer.Serialize(new { error = message, status = (int)code });
        return context.Response.WriteAsync(result);
    }
}