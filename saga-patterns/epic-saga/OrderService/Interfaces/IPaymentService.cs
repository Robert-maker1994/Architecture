public interface IPaymentService
{
    Task<string> ProcessPaymentAsync(Guid orderId, decimal amount, string customerId); // Returns a payment transaction ID
    Task RefundPaymentAsync(Guid orderId, string paymentTransactionId); // Compensating
}
