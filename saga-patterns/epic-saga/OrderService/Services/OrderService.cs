using Microsoft.Extensions.Logging;
using OrderService.Interfaces;
using OrderService.Models;

namespace Services.OrderService
{

    public class OrderDataService: IOrderDataService
    {
        private readonly ILogger<OrderDataService> _logger;

        public OrderDataService(ILogger<OrderDataService> logger)
        {
            _logger = logger;
        }


        public Task<OrderDetails> GetOrderAsync(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public Task RecordPaymentTransactionIdAsync(Guid orderId, string paymentTransactionId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateOrderStatusAsync(Guid orderId, string status)
        {
            throw new NotImplementedException();
        }


        Task IOrderDataService.CreateOrderAsync(OrderDetails order)
        {
            throw new NotImplementedException();
        }
    }

}