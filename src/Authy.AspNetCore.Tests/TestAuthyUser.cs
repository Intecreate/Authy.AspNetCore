using Xunit;

namespace Authy.AspNetCore.Tests
{
    public class TestAuthyUser
    {
        [Fact]
        public void TestGetSetAuthyUser()
        {
            var user = new AuthyUser
            {
                CountryCode = "1",
                PhoneNumber = "123-456-7890",
                Email = "example@example.com"
            };
            Assert.Equal("1", user.CountryCode);
            Assert.Equal("123-456-7890", user.PhoneNumber);
            Assert.Equal("example@example.com", user.Email);
        }
    }
}
