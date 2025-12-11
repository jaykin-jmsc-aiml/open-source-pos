using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using LiquorPOS.Services.Identity.Domain.Options;
using LiquorPOS.Services.Identity.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace LiquorPOS.Services.Identity.UnitTests.Services;

public sealed class TokenValidatorTests
{
    private readonly JwtOptions _jwtOptions;
    private readonly TokenValidator _sut;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public TokenValidatorTests()
    {
        _jwtOptions = new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningKey = "ThisIsATestSigningKeyWith32PlusCharacters!",
            AccessTokenLifetimeMinutes = 15,
            RefreshTokenLifetimeDays = 7
        };

        var optionsMock = new Mock<IOptions<JwtOptions>>();
        optionsMock.Setup(o => o.Value).Returns(_jwtOptions);

        _sut = new TokenValidator(optionsMock.Object);
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    [Fact]
    public void ValidateToken_ShouldReturnClaimsPrincipal_WhenTokenIsValid()
    {
        var token = GenerateValidToken();

        var principal = _sut.ValidateToken(token);

        principal.Should().NotBeNull();
        principal!.Claims.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenIsInvalid()
    {
        var invalidToken = "invalid.token.here";

        var principal = _sut.ValidateToken(invalidToken);

        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenIsExpired()
    {
        var expiredToken = GenerateExpiredToken();

        var principal = _sut.ValidateToken(expiredToken);

        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenHasWrongIssuer()
    {
        var token = GenerateTokenWithWrongIssuer();

        var principal = _sut.ValidateToken(token);

        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenHasWrongAudience()
    {
        var token = GenerateTokenWithWrongAudience();

        var principal = _sut.ValidateToken(token);

        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenHasWrongSignature()
    {
        var token = GenerateTokenWithWrongSignature();

        var principal = _sut.ValidateToken(token);

        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenIsEmpty()
    {
        var principal = _sut.ValidateToken(string.Empty);

        principal.Should().BeNull();
    }

    [Fact]
    public void IsTokenExpired_ShouldReturnFalse_WhenTokenIsValid()
    {
        var token = GenerateValidToken();

        var isExpired = _sut.IsTokenExpired(token);

        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsTokenExpired_ShouldReturnTrue_WhenTokenIsExpired()
    {
        var expiredToken = GenerateExpiredToken();

        var isExpired = _sut.IsTokenExpired(expiredToken);

        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsTokenExpired_ShouldReturnTrue_WhenTokenIsInvalid()
    {
        var invalidToken = "invalid.token.here";

        var isExpired = _sut.IsTokenExpired(invalidToken);

        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsTokenExpired_ShouldReturnTrue_WhenTokenIsEmpty()
    {
        var isExpired = _sut.IsTokenExpired(string.Empty);

        isExpired.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_ShouldContainExpectedClaims_WhenTokenIsValid()
    {
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var token = GenerateValidToken(userId, email);

        var principal = _sut.ValidateToken(token);

        principal.Should().NotBeNull();
        // JWT claims are normalized to their full URI forms after validation
        principal!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
    }

    private string GenerateValidToken(Guid? userId = null, string? email = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, (userId ?? Guid.NewGuid()).ToString()),
            new(JwtRegisteredClaimNames.Email, email ?? "test@example.com"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = credentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    private string GenerateExpiredToken()
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, "test@example.com"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var notBefore = DateTime.UtcNow.AddMinutes(-5);
        var expires = DateTime.UtcNow.AddMinutes(-1);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = notBefore,
            Expires = expires,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = credentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    private string GenerateTokenWithWrongIssuer()
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, "test@example.com")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes),
            Issuer = "WrongIssuer",
            Audience = _jwtOptions.Audience,
            SigningCredentials = credentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    private string GenerateTokenWithWrongAudience()
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, "test@example.com")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes),
            Issuer = _jwtOptions.Issuer,
            Audience = "WrongAudience",
            SigningCredentials = credentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    private string GenerateTokenWithWrongSignature()
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, "test@example.com")
        };

        var wrongKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("WrongSigningKeyThatIsAtLeast32CharactersLong!"));
        var credentials = new SigningCredentials(wrongKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = credentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }
}
