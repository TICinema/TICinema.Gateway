using Microsoft.AspNetCore.Mvc;
using TICinema.Contracts.Protos.Payment;
using TICinema.Gateway.DTOs;

namespace TICinema.Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController(PaymentService.PaymentServiceClient paymentClient) : ControllerBase
{
    private readonly PaymentService.PaymentServiceClient _paymentClient = paymentClient;

    [HttpPost("init")]
    public async Task<IActionResult> InitializePayment([FromBody] InitPaymentRequestDto request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                     ?? "guest_user"; 

        int totalAmountInTiyns = request.Seats.Sum(s => s.Price) * 100;
        string description = $"Оплата {request.Seats.Count()} мест на сеанс {request.ScreeningId}";

        var grpcRequest = new CreatePaymentRequest
        {
            // ВАЖНО: Теперь OrderId в gRPC-запросе — это наш ScreeningId
            OrderId = request.ScreeningId.ToString(), 
            Amount = totalAmountInTiyns,
            Method = request.PaymentMethodId,
            Description = description,
            //UserId = userId
        };

        try
        {
            var response = await _paymentClient.CreatePaymentAsync(grpcRequest);

            // 4. Возвращаем фронтенду всё необходимое: ссылку на оплату, статус и т.д.
            return Ok(new
            {
                paymentId = response.PaymentId,
                paymentUrl = response.PaymentUrl, // Ссылка на Stripe Checkout
                status = response.Status,
                token = response.Token
            });
        }
        catch (Grpc.Core.RpcException ex)
        {
            return StatusCode(500, new { message = "Ошибка связи с сервисом платежей", details = ex.Status.Detail });
        }
    }
}