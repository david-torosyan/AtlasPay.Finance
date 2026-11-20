namespace AtlasPay.Finance.Application.Abstractions.Identity;

public interface ICurrentCustomer
{
    Guid CustomerId { get; }
}