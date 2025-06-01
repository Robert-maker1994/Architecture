
public interface IShippingService
{
    Task<bool> ArrangeShippingAsync(Guid orderId, string customerId /*, address etc */);
    Task CancelShippingAsync(Guid orderId); // Compensating
}
