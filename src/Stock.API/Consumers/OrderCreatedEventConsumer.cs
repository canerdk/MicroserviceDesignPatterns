using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Events;
using Stock.API.DataAccess;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderCreatedEventConsumer> _logger;
        private readonly ISendEndpointProvider _sendEndpoint;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(AppDbContext context, ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpoint, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _logger = logger;
            _sendEndpoint = sendEndpoint;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var stockResult = new List<bool>();

            foreach (var item in context.Message.OrderItems)
            {
                stockResult.Add(await _context.Stocks.AnyAsync(x => x.ProductId == item.ProductId && x.Count > item.Count));
            }

            if(stockResult.All(x => x.Equals(true)))
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var stock = await _context.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                    if(stock != null)
                    {
                        stock.Count -= item.Count;
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Stock was reserved for BuyerId:{context.Message.BuyerId}");
                    var sendEndpoint = await _sendEndpoint.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StockReservedEventQueueName}"));

                    var stockReservedEvent = new StockReservedEvent()
                    {
                        Payment = context.Message.Payment,
                        BuyerId = context.Message.BuyerId,
                        OrderId = context.Message.OrderId,
                        OrderItems = context.Message.OrderItems
                    };

                    await sendEndpoint.Send(stockReservedEvent);
                }
            }
            else
            {
                var stockNotReserved = new StockNotReservedEvent()
                {
                    OrderId = context.Message.OrderId,
                    Message = "Stock not enough"
                };
                await _publishEndpoint.Publish(stockNotReserved);
                _logger.LogInformation("Stock not reserved");
            }


        }
    }
}
