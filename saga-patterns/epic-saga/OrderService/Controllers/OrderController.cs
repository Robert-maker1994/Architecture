using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using Services.OrderService;
using OrderService.Interfaces;
using OrderService.Services;
using Microsoft.Extensions.ObjectPool;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
 {
 private readonly IOrderSagaOrchestrator _orderSagaOrchestrator;

 public OrderController(IOrderSagaOrchestrator orderSagaOrchestrator )
        {
            _orderSagaOrchestrator = orderSagaOrchestrator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDetails order)
        {
            try
            {
               bool response = await _orderSagaOrchestrator.PlaceOrderAsync(order);
                
                return CreatedAtAction("Placed order", new { successful = response });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while creating the order.");
            }
        }

      
    }
}
