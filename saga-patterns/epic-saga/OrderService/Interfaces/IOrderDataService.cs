using OrderService.Models;

namespace OrderService.Interfaces
{
    public interface IOrderDataService // For persisting order state
    {
        Task CreateOrderAsync(OrderDetails order);
        Task UpdateOrderStatusAsync(Guid orderId, string status);
        Task<OrderDetails> GetOrderAsync(Guid orderId);
        Task RecordPaymentTransactionIdAsync(Guid orderId, string paymentTransactionId);
    }
}