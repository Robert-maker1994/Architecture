using OrderService.Models;

namespace OrderService.Interfaces
{
    public interface IInventoryService
    {
        Task<bool> ReserveInventoryAsync(Guid orderId, List<OrderItem> items);
        Task ReleaseInventoryAsync(Guid orderId, List<OrderItem> items); // Compensating
    }

}
