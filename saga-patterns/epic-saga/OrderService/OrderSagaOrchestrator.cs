using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Extensions;
using OrderService.Interfaces;
using OrderService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public class SagaStepFailedException : Exception
    {
        public SagaStepFailedException(string message) : base(message) { }
    }
    
    public interface IOrderSagaOrchestrator
    {
        Task<bool> PlaceOrderAsync(OrderDetails order);
    }

    public class OrderSagaOrchestrator
    {
        private readonly ICustomerService _customerService;
        private readonly IInventoryService _inventoryService;
        private readonly IPaymentService _paymentService;
        private readonly IShippingService _shippingService;
        private readonly IOrderDataService _orderDataService;
        private readonly ILogger<OrderSagaOrchestrator> _logger;

        public OrderSagaOrchestrator(
            ICustomerService customerService,
            IInventoryService inventoryService,
            IPaymentService paymentService,
            IShippingService shippingService,
            IOrderDataService orderDataService,
            ILogger<OrderSagaOrchestrator> logger)
        {
            _customerService = customerService;
            _inventoryService = inventoryService;
            _paymentService = paymentService;
            _shippingService = shippingService;
            _orderDataService = orderDataService;
            _logger = logger;
        }

        public async Task<bool> PlaceOrderAsync(OrderDetails order)
        {
            var completedSteps = new Stack<Func<Task>>();
            string paymentTransactionId = null;

            try
            {
                if (order.OrderId == Guid.Empty) order.OrderId = Guid.NewGuid();

                _logger.LogInformation("Order {OrderId}: Creating order, initial status PENDING_CUSTOMER_VALIDATION", order.OrderId);
                await _orderDataService.CreateOrderAsync(order);
                await _orderDataService.UpdateOrderStatusAsync(order.OrderId, "PENDING_CUSTOMER_VALIDATION");

                _logger.LogInformation("Order {OrderId}: Validating customer and reserving credit for amount {Amount}", order.OrderId, order.TotalAmount);
                if (!await _customerService.ValidateCustomerAndReserveCreditAsync(order.CustomerId, order.TotalAmount))
                {
                    await _orderDataService.UpdateOrderStatusAsync(order.OrderId, "FAILED_CUSTOMER_VALIDATION");
                    throw new SagaStepFailedException("Customer validation failed.");
                }
                completedSteps.Push(async () => await _customerService.ReleaseCustomerCreditAsync(order.CustomerId, order.TotalAmount));
                await _orderDataService.UpdateOrderStatusAsync(order.OrderId, "PENDING_INVENTORY_RESERVATION");

                _logger.LogInformation("Order {OrderId}: Reserving inventory", order.OrderId);
                if (!await _inventoryService.ReserveInventoryAsync(order.OrderId, order.Items))
                {
                    await _orderDataService.UpdateOrderStatusAsync(order.OrderId, "FAILED_INVENTORY_RESERVATION");
                    throw new SagaStepFailedException("Inventory reservation failed.");
                }
                completedSteps.Push(async () => await _inventoryService.ReleaseInventoryAsync(order.OrderId, order.Items));
                await _orderDataService.UpdateOrderStatusAsync(order.OrderId, "PENDING_PAYMENT");

                _logger.LogInformation("Order {OrderId}: Processing payment", order.OrderId);
                paymentTransactionId = await _paymentService.ProcessPaymentAsync(order.OrderId, order.TotalAmount, order.CustomerId);
                if (string.IsNullOrEmpty(paymentTransactionId))
                {
                    await _orderDataService.UpdateOrderStatusAsync(order.OrderId, "FAILED_PAYMENT");
                    throw new SagaStepFailedException("Payment processing failed.");
                }
                await _orderDataService.RecordPaymentTransactionIdAsync(order.OrderId, paymentTransactionId);
                string finalPaymentTxId = paymentTransactionId;
                completedSteps.Push(async () => await _paymentService.RefundPaymentAsync(order.OrderId, finalPaymentTxId));
                await _orderDataService.UpdateOrderStatusAsync(order.OrderId, "PENDING_SHIPPING");

                _logger.LogInformation("Order {OrderId}: Arranging shipping", order.OrderId);
                if (!await _shippingService.ArrangeShippingAsync(order.OrderId, order.CustomerId))
                {
                    await _orderDataService.UpdateOrderStatusAsync(order.OrderId, "FAILED_SHIPPING");
                    throw new SagaStepFailedException("Shipping arrangement failed.");
                }
                completedSteps.Push(async () => await _shippingService.CancelShippingAsync(order.OrderId));
                await _orderDataService.UpdateOrderStatusAsync(order.OrderId, "COMPLETED");
                _logger.LogInformation("Order {OrderId}: Saga COMPLETED successfully.", order.OrderId);
                return true;
            }
            catch (SagaStepFailedException ex)
            {
                _logger.LogError(ex, "Order {OrderId}: Saga failed at step. Initiating rollback.", order.OrderId);
                await RollbackAsync(order.OrderId, completedSteps, ex.Message);
                return false;
            }
            catch (Exception ex) // Catch any other unexpected errors
            {
                _logger.LogCritical(ex, "Order {OrderId}: An unexpected error occurred in saga. Initiating rollback.", order.OrderId);

                var currentOrder = await _orderDataService.GetOrderAsync(order.OrderId);
                if (currentOrder != null && !currentOrder.Status.GetDisplayName().StartsWith("FAILED"))
                {
                    await _orderDataService.UpdateOrderStatusAsync(order.OrderId, "FAILED_UNEXPECTED_ERROR");
                }
                await RollbackAsync(order.OrderId, completedSteps, "Unexpected error: " + ex.Message);
                return false;
            }
        }

        private async Task RollbackAsync(Guid orderId, Stack<Func<Task>> completedSteps, string failureReason)
        {
            _logger.LogWarning("Order {OrderId}: Rolling back {StepCount} completed steps due to: {FailureReason}", orderId, completedSteps.Count, failureReason);
            while (completedSteps.Count > 0)
            {
                var compensatingAction = completedSteps.Pop();
                try
                {
                    await compensatingAction();
                    _logger.LogInformation("Order {OrderId}: Compensating action executed successfully.", orderId);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Order {OrderId}: CRITICAL - Compensating action failed. Manual intervention likely required.", orderId);
                    await _orderDataService.UpdateOrderStatusAsync(orderId, "FAILED_ROLLBACK_ACTION_MANUAL_INTERVENTION");
                    // Depending on requirements, you might re-throw or handle this to stop further rollbacks if one fails critically.
                    return; // Stop further rollback attempts if one compensation fails critically.
                }
            }

            // Final status update after rollback attempts, if not already set to a critical failure.
            var order = await _orderDataService.GetOrderAsync(orderId);
            if (order != null && order.Status != Status.FailedPayment && order.Status != Status.FailedShipping)
            {
                await _orderDataService.UpdateOrderStatusAsync(orderId, "FAILED_ROLLED_BACK");
            }
            _logger.LogInformation("Order {OrderId}: Rollback process finished.", orderId);
        }
    }
}