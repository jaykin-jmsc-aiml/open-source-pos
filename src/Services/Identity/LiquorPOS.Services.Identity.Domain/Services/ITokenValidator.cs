namespace LiquorPOS.Services.Identity.Domain.Services;

public interface ITokenValidator
{
    System.Security.Claims.ClaimsPrincipal? ValidateToken(string token);
    bool IsTokenExpired(string token);
}