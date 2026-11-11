
using MooreHotelAndSuites.Application.Interfaces.Identity;

namespace MooreHotelAndSuites.Application.Interfaces.Services;

public interface IJwtTokenService
{
    Task<string> GenerateAccessTokenAsync(
        IApplicationUser user,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);

    string GenerateRefreshToken();
}
