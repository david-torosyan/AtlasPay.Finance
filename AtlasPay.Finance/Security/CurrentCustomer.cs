using AtlasPay.Finance.Application.Abstractions.Identity;
using System.Security.Claims;

namespace AtlasPay.Finance.Api.Security;

public sealed class CurrentCustomer(
    IHttpContextAccessor httpContextAccessor) : ICurrentCustomer
{
    public Guid CustomerId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var customerId = user.FindFirstValue("customer_id");

            if (!Guid.TryParse(customerId, out var id) ||
                id == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "Invalid customer identity.");
            }

            return id;
        }
    }
}