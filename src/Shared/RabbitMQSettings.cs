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
        public const string StockNotReservedEventQueueName = "stock-not-reserved-queue";
        public const string OrderPaymentCompletedEventQueueName = "order-payment-completed-queue";
        public const string OrderFailedCompletedEventQueueName = "order-payment-failed-queue";
    }
}
