using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.DataAccess;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentCompletedEventConsumer>();
    x.AddConsumer<PaymentFailedEventConsumer>();
    x.AddConsumer<StockNotReservedEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
        cfg.ReceiveEndpoint(RabbitMQSettings.OrderPaymentCompletedEventQueueName, e =>
        {
            e.ConfigureConsumer<PaymentCompletedEventConsumer>(context);
        });
        cfg.ReceiveEndpoint(RabbitMQSettings.OrderFailedCompletedEventQueueName, e =>
        {
            e.ConfigureConsumer<PaymentFailedEventConsumer>(context);
        });
        cfg.ReceiveEndpoint(RabbitMQSettings.StockNotReservedEventQueueName, e =>
        {
            e.ConfigureConsumer<StockNotReservedEventConsumer>(context);
        });
    });
});

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"));
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
