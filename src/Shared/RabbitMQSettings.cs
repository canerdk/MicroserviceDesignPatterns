using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class RabbitMQSettings
    {
        public const string StockOrderCreateEventQueueName = "stock-order-created-queue";
        public const string StockReservedEventQueueName = "stock-reserved-queue";
        public const string PaymentStockReservedEventQueueName = "payment-stock-reserved-queue";
    }
}
