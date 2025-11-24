using API.Entities;
using API.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.Tests.Services
{
    public class TokenServiceTests
    {
        private IConfiguration BuildConfig(string? key)
        {
            var dict = new Dictionary<string, string?>
            {
                { "TokenKey", key }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .Build();
        }

        private AppUser BuildUser()
        {
            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                DisplayName = "admin",
                Email = "admin@admin.pl",
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("admin")),
                PasswordSalt = hmac.Key
            };
            return user;
        }

        [Fact]
        public void CreateToken_ShouldGenerate_ValidJwtWithClaims()
        {
            var key = new string('x', 64);
            var config = BuildConfig(key);
            var service = new TokenService(config);
            var user = BuildUser();

            var token = service.CreateToken(user);

            var handler = new JwtSecurityTokenHandler();
            handler.CanReadToken(token).Should().BeTrue();

            var jwt = handler.ReadJwtToken(token);

            jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
            jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);

            jwt.ValidTo.Should().BeAfter(DateTime.UtcNow.AddDays(6.9));
            jwt.Header.Alg.Should().Be(SecurityAlgorithms.HmacSha512);
        }

        [Fact]
        public void CreateToken_ShouldThrow_WhenTokenKeyMissing()
        {
            var config = BuildConfig(null);
            var service = new TokenService(config);

            Action act = () => service.CreateToken(BuildUser());

            act.Should().Throw<Exception>()
                .WithMessage("Cannot get token key");
        }

        [Fact]
        public void CreateToken_ShouldThrow_WhenTokenKeyTooShort()
        {
            var config = BuildConfig("short");
            var service = new TokenService(config);

            Action act = () => service.CreateToken(BuildUser());

            act.Should().Throw<Exception>()
                .WithMessage("Your token key needs to be >= 64 characters");
        }

        [Fact]
        public void CreateToken_ShouldBeValid_WhenValidatedWithCorrectKey()
        {
            var key = new string('a', 64);
            var config = BuildConfig(key);
            var service = new TokenService(config);
            var user = BuildUser();

            var token = service.CreateToken(user);

            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
            };

            var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

            principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
            principal.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);

            validatedToken.Should().BeOfType<JwtSecurityToken>();
        }
    }
}