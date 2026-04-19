using Microsoft.AspNetCore.Mvc;
using Stripe;
using TICinema.Contracts.Protos.Payment;

namespace TICinema.Gateway.Controllers;

[ApiController]
[Route("api/webhooks/stripe")]
public class StripeWebhookController(
    IConfiguration configuration,
    PaymentService.PaymentServiceClient paymentClient // gRPC клиент для связи с Payment Service
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Handle()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"];
        var webhookSecret = configuration["Stripe:WebhookSecret"];

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json, 
                stripeSignature, 
                webhookSecret, 
                throwOnApiVersionMismatch: false // Вот этот парень нам нужен
            );

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            
                // Защита: проверяем, есть ли ключ в метаданных
                if (session?.Metadata == null || !session.Metadata.ContainsKey("paymentId"))
                {
                    Console.WriteLine("❌ Ошибка: В Metadata нет paymentId!");
                    return Ok(); // Возвращаем 200, чтобы Stripe не мучал нас повторами
                }

                var paymentId = session.Metadata["paymentId"];
                Console.WriteLine($"✅ Обработка оплаты для PaymentId: {paymentId}");

                var result = await paymentClient.UpdatePaymentStatusAsync(new UpdatePaymentStatusRequest
                {
                    PaymentId = paymentId,
                    Status = "SUCCESS"
                });

                return Ok();
            }

            return Ok();
        }
        catch (Exception ex)
        {
            // ВАЖНО: Выведи ошибку в консоль Gateway
            Console.WriteLine($"Critical Error in Webhook: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"Inner: {ex.InnerException.Message}");
        
            return StatusCode(500, ex.Message);
        }
    }
}