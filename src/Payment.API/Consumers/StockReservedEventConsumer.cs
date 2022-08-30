using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        private readonly ILogger<StockReservedEventConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public StockReservedEventConsumer(ILogger<StockReservedEventConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            var balance = 3000m;

            if(balance > context.Message.Payment.TotalPrice)
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was withdrawn from credit card for user id: {context.Message.BuyerId}");

                var paymentSuccessEvent = new PaymentSuccessedEvent()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId
                };

                await _publishEndpoint.Publish(paymentSuccessEvent);
            }
            else
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was not withdrawn from credit card for user id: {context.Message.BuyerId}");

                var paymentFailedEvent = new PaymentFailedEvent()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    Message = "Not enough money"
                };

                await _publishEndpoint.Publish(paymentFailedEvent);
            }

        }
    }
}
