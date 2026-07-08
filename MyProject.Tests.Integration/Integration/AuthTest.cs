using System.Net;
using System.Net.Http.Json;
using Xunit;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace MyProject.Tests.Integration.Integration
{
    [Collection("Integration Tests")]
    public class AuthTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AuthTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task ResetDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        [Fact]
        public async Task Login_ReturnsToken_WhenCredentialsAreCorrect()
        {
            await ResetDatabaseAsync();

            var user = new
            {
                name = "Test",
                lastName = "User",
                phone = "123456789",
                email = "test@mail.com",
                password = "Test123!"
            };

            await _client.PostAsJsonAsync("/User/register", user);

            var loginData = new
            {
                email = "test@mail.com",
                password = "Test123!"
            };

            var response = await _client.PostAsJsonAsync("/User/login", loginData);
            var json = await response.Content.ReadFromJsonAsync<AuthResponse>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(json?.Token);
        }

        [Fact]
        public async Task Login_Returns401_WhenCredentialsWrong()
        {
            await ResetDatabaseAsync();

            var response = await _client.PostAsJsonAsync("/User/login", new { email = "x", password = "x" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Register_ReturnsToken_WhenUserIsValid()
        {
            await ResetDatabaseAsync();

            var payload = new
            {
                name = "Pepe",
                lastName = "Gomez",
                phone = "987654321",
                email = "pepe@mail.com",
                password = "Pepe123!"
            };

            var response = await _client.PostAsJsonAsync("/User/register", payload);
            var json = await response.Content.ReadFromJsonAsync<AuthResponse>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(json?.Token);
        }

        [Fact]
        public async Task Register_ReturnsConflict_WhenEmailAlreadyExists()
        {
            await ResetDatabaseAsync();

            var payload = new
            {
                name = "Pepe",
                lastName = "Gomez",
                phone = "987654321",
                email = "duplicate@mail.com",
                password = "Test123!"
            };

            await _client.PostAsJsonAsync("/User/register", payload);
            var response = await _client.PostAsJsonAsync("/User/register", payload);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
    }
}
