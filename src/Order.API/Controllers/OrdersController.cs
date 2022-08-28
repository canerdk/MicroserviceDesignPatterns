using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.DataAccess;
using Order.API.DTOs;
using Order.API.Models;
using Shared.Events;
using Shared.Messages;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(AppDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto order)
        {
            var newOrder = new Models.Order()
            {
                BuyerId = order.BuyerId,
                Status = OrderStatus.Suspend,
                Address = new Address() { Line = order.Address.Line, District = order.Address.District , Province = order.Address.Province},
                CreatedDate = DateTime.Now
            };

            order.OrderItems.ForEach(item =>
            {
                newOrder.Items.Add(new OrderItem() { Price = item.Price, ProductId = item.ProductId, Count = item.Count });
            });

            await _context.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            var orderCreatedEvent = new OrderCreatedEvent()
            {
                BuyerId = order.BuyerId,
                OrderId = newOrder.Id,
                Payment = new PaymentMessage() { CardName = order.Payment.CardName, CardNumber = order.Payment.CardNumber, CVV = order.Payment.CVV, Expiration = order.Payment.Expiration, TotalPrice = order.OrderItems.Sum(x => x.Price * x.Count) }
            };

            order.OrderItems.ForEach(item =>
            {
                orderCreatedEvent.OrderItems.Add(new OrderItemMessage() { Count = item.Count, ProductId = item.ProductId });
            });

            await _publishEndpoint.Publish(orderCreatedEvent);

            return Ok();
        }
    }
}
