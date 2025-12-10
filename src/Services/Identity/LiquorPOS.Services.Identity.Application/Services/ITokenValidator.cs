using System.Security.Claims;

namespace LiquorPOS.Services.Identity.Application.Services;

public interface ITokenValidator
{
    ClaimsPrincipal? ValidateToken(string token);
    bool IsTokenExpired(string token);
}
