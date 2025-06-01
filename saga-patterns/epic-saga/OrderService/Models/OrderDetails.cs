namespace OrderService.Models
{
    public enum Status
    {
        PendingCustomerValidation,
        PendingInventoryReservation,
        PendingPayment,

        Pending,
        PendingShipping,
        Completed,
        FailedCustomerValidation,
        FailedInventoryReservation,
        FailedPayment,
        FailedShipping,
    }

    public class OrderDetails
    {
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalAmount { get; set; }
        public Status Status { get; set; }
        public string PaymentTransactionId { get; set; }

    }

    public class OrderItem
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }


}