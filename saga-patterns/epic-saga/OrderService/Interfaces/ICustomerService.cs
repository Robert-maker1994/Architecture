namespace OrderService.Interfaces
{

    public interface ICustomerService
    {
        Task<bool> ValidateCustomerAndReserveCreditAsync(string customerId, decimal amountToReserve);
        Task ReleaseCustomerCreditAsync(string customerId, decimal amountToRelease); // Compensating
    }

}