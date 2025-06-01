namespace OrderService.Tests;
using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrderService.interfaces;
using OrderService.models; 


public class OrderDetails 
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; }
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } // Crucial for saga state tracking
    public string PaymentTransactionId { get; set; } // Needed for refunds
}

public class OrderItem
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
}


public class SagaStepFailedException : Exception
{
    public SagaStepFailedException(string message) : base(message) { }
}



public class OrderSagaOrchestratorTests
{
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly Mock<IInventoryService> _mockInventoryService;
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly Mock<IShippingService> _mockShippingService;
    private readonly Mock<IOrderDataService> _mockOrderDataService;
    private readonly Mock<ILogger<OrderSagaOrchestrator>> _mockLogger;
    private readonly OrderSagaOrchestrator _orchestrator;

    public OrderSagaOrchestratorTests()
    {
        _mockCustomerService = new Mock<ICustomerService>();
        _mockInventoryService = new Mock<IInventoryService>();
        _mockPaymentService = new Mock<IPaymentService>();
        _mockShippingService = new Mock<IShippingService>();
        _mockOrderDataService = new Mock<IOrderDataService>();
        _mockLogger = new Mock<ILogger<OrderSagaOrchestrator>>();

        _orchestrator = new OrderSagaOrchestrator(
            _mockCustomerService.Object,
            _mockInventoryService.Object,
            _mockPaymentService.Object,
            _mockShippingService.Object,
            _mockOrderDataService.Object,
            _mockLogger.Object
        );

        // Default setup for GetOrderAsync for rollback status checks
        _mockOrderDataService.Setup(s => s.GetOrderAsync(It.IsAny<Guid>()))
                             .ReturnsAsync((Guid orderId) => new OrderDetails { OrderId = orderId, Status = "UNKNOWN" }); // Default mock
    }

    private OrderDetails CreateTestOrder() => new OrderDetails
    {
        CustomerId = "cust123",
        Items = new List<OrderItem> { new OrderItem { ProductId = "prodA", Quantity = 1 } },
        TotalAmount = 100.00m
    };

    [Fact]
    public async Task PlaceOrderAsync_HappyPath_ShouldSucceedAndCompleteOrder()
    {
        // Arrange
        var order = CreateTestOrder();
        _mockCustomerService.Setup(s => s.ValidateCustomerAndReserveCreditAsync(order.CustomerId, order.TotalAmount)).ReturnsAsync(true);
        _mockInventoryService.Setup(s => s.ReserveInventoryAsync(It.IsAny<Guid>(), order.Items)).ReturnsAsync(true);
        _mockPaymentService.Setup(s => s.ProcessPaymentAsync(It.IsAny<Guid>(), order.TotalAmount, order.CustomerId)).ReturnsAsync("txn_123");
        _mockShippingService.Setup(s => s.ArrangeShippingAsync(It.IsAny<Guid>(), order.CustomerId)).ReturnsAsync(true);

        // Act
        var result = await _orchestrator.PlaceOrderAsync(order);

        // Assert
        Assert.True(result);
        _mockOrderDataService.Verify(s => s.CreateOrderAsync(It.Is<OrderDetails>(o => o.CustomerId == order.CustomerId)), Times.Once);
        _mockOrderDataService.Verify(s => s.UpdateOrderStatusAsync(order.OrderId, "COMPLETED"), Times.Once);

        // Verify all steps were called
        _mockCustomerService.Verify(s => s.ValidateCustomerAndReserveCreditAsync(order.CustomerId, order.TotalAmount), Times.Once);
        _mockInventoryService.Verify(s => s.ReserveInventoryAsync(order.OrderId, order.Items), Times.Once);
        _mockPaymentService.Verify(s => s.ProcessPaymentAsync(order.OrderId, order.TotalAmount, order.CustomerId), Times.Once);
        _mockShippingService.Verify(s => s.ArrangeShippingAsync(order.OrderId, order.CustomerId), Times.Once);
        _mockOrderDataService.Verify(s => s.RecordPaymentTransactionIdAsync(order.OrderId, "txn_123"), Times.Once);
    }

    [Fact]
    public async Task PlaceOrderAsync_PaymentFails_ShouldRollbackCustomerAndInventory()
    {
        // Arrange
        var order = CreateTestOrder();
        _mockCustomerService.Setup(s => s.ValidateCustomerAndReserveCreditAsync(order.CustomerId, order.TotalAmount)).ReturnsAsync(true);
        _mockInventoryService.Setup(s => s.ReserveInventoryAsync(It.IsAny<Guid>(), order.Items)).ReturnsAsync(true);
        _mockPaymentService.Setup(s => s.ProcessPaymentAsync(It.IsAny<Guid>(), order.TotalAmount, order.CustomerId)).ReturnsAsync((string)null); // Payment fails

        // Act
        var result = await _orchestrator.PlaceOrderAsync(order);

        // Assert
        Assert.False(result);
        _mockOrderDataService.Verify(s => s.UpdateOrderStatusAsync(order.OrderId, "FAILED_PAYMENT"), Times.Once);

        // Verify compensating actions
        _mockInventoryService.Verify(s => s.ReleaseInventoryAsync(order.OrderId, order.Items), Times.Once);
        _mockCustomerService.Verify(s => s.ReleaseCustomerCreditAsync(order.CustomerId, order.TotalAmount), Times.Once);

        // Verify payment refund and shipping cancel were NOT called
        _mockPaymentService.Verify(s => s.RefundPaymentAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        _mockShippingService.Verify(s => s.CancelShippingAsync(It.IsAny<Guid>()), Times.Never);

        // Verify final status after rollback
        _mockOrderDataService.Verify(s => s.UpdateOrderStatusAsync(order.OrderId, "FAILED_ROLLED_BACK"), Times.Once);
    }

    // TODO: Add more tests for:
    // - Customer validation failure
    // - Inventory reservation failure
    // - Shipping arrangement failure
    // - Failure of a compensating action (ensure critical logging and specific status)
}
