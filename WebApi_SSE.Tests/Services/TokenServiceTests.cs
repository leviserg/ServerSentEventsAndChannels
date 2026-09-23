using WebApi_SSE.Services;
using Xunit;

namespace WebApi_SSE.Tests.Services
{
    public class TokenServiceTests
    {
        [Fact]
        public void GenerateAndValidateToken_ReturnsUserEmail()
        {
            var sut = new TokenService();

            var token = sut.GenerateToken("u1@example.com");
            var email = sut.ValidateToken(token);

            Assert.Equal("u1@example.com", email);
        }

        [Fact]
        public void ValidateToken_CannotBeUsedTwice()
        {
            var sut = new TokenService();

            var token = sut.GenerateToken("u1@example.com");
            _ = sut.ValidateToken(token);
            var secondTry = sut.ValidateToken(token);

            Assert.Null(secondTry);
        }

        [Fact]
        public void ValidateToken_UnknownToken_ReturnsNull()
        {
            var sut = new TokenService();

            var result = sut.ValidateToken("not-a-token");

            Assert.Null(result);
        }
    }
}
