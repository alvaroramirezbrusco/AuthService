using Application.Features.User.Query;
using Application.Interfaces.HelperInterface;
using Application.Interfaces.Query;
using Application.Models.Request;
using Microsoft.Extensions.Configuration;
using Moq;
using RoleEntity = Domain.Entities.Role;
using UserEntity = Domain.Entities.User;

namespace AuthTest.User.Query
{
    public class LoginHandlerTests
    {
        private readonly Mock<IUserQuery> _userQuery;
        private readonly Mock<IHashingService> _hashingService;
        private readonly Mock<IConfiguration> _configuration;

        public LoginHandlerTests()
        {
            _userQuery = new Mock<IUserQuery>();
            _hashingService = new Mock<IHashingService>();
            _configuration = new Mock<IConfiguration>();
        }

        [Fact]
        public async Task Handle_ReturnsToken_WhenCredentialsAreValid()
        {
            _configuration
                .Setup(c => c["Jwt:Key"])
                .Returns("aVeryLongSuperSecretKeyOfAtLeast32Chars!");

            _configuration
                .Setup(c => c["Jwt:Issuer"])
                .Returns("AuthService");

            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "johndoe@gmail.com",
                Password = "HASH123",
                Phone = "123456789",
                Role = new RoleEntity
                {
                    RoleId = 1,
                    Name = "Administrador"
                }
            };

            _userQuery
                .Setup(q => q.GetByEmail(user.Email))
                .ReturnsAsync(user);

            _hashingService
                .Setup(h => h.encryptSHA256("1234"))
                .Returns("HASH123");

            var handler = new LoginQueryHandler(
                _userQuery.Object,
                _hashingService.Object,
                _configuration.Object);

            var request = new LoginRequest
            {
                Email = user.Email,
                Password = "1234"
            };

            var query = new LoginQuery(request);

            var result = await handler.Handle(query, default);

            Assert.NotNull(result);
            Assert.Equal(user.Name, result.Name);
            Assert.Equal(user.LastName, result.LastName);
            Assert.Equal(user.Email, result.Email);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));

            _userQuery.Verify(
                q => q.GetByEmail(user.Email),
                Times.Once);

            _hashingService.Verify(
                h => h.encryptSHA256("1234"),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidEmail_Throws()
        {
            var handler = new LoginQueryHandler(
                _userQuery.Object,
                _hashingService.Object,
                _configuration.Object);

            var request = new LoginRequest
            {
                Email = "John@gmail.com",
                Password = "1234"
            };

            var query = new LoginQuery(request);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(query, default));

            _userQuery.Verify(
                q => q.GetByEmail(It.IsAny<string>()),
                Times.Never);

            _hashingService.Verify(
                h => h.encryptSHA256(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyPassword_Throws()
        {
            var handler = new LoginQueryHandler(
                _userQuery.Object,
                _hashingService.Object,
                _configuration.Object);

            var request = new LoginRequest
            {
                Email = "johndoe@gmail.com",
                Password = ""
            };

            var query = new LoginQuery(request);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(query, default));

            _userQuery.Verify(
                q => q.GetByEmail(It.IsAny<string>()),
                Times.Never);

            _hashingService.Verify(
                h => h.encryptSHA256(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_UserNotFound_Throws()
        {
            _userQuery
                .Setup(q => q.GetByEmail("johndoe@gmail.com"))
                .ReturnsAsync((UserEntity?)null);

            var handler = new LoginQueryHandler(
                _userQuery.Object,
                _hashingService.Object,
                _configuration.Object);

            var request = new LoginRequest
            {
                Email = "johndoe@gmail.com",
                Password = "1234"
            };

            var query = new LoginQuery(request);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(query, default));

            _userQuery.Verify(
                q => q.GetByEmail("johndoe@gmail.com"),
                Times.Once);

            _hashingService.Verify(
                h => h.encryptSHA256(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidPassword_Throws()
        {
            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "johndoe@gmail.com",
                Password = "HASH_CORRECTO",
                Phone = "123456789",
                Role = new RoleEntity
                {
                    RoleId = 1,
                    Name = "Administrador"
                }
            };

            _userQuery
                .Setup(q => q.GetByEmail(user.Email))
                .ReturnsAsync(user);

            _hashingService
                .Setup(h => h.encryptSHA256("1234"))
                .Returns("HASH_INCORRECTO");

            var handler = new LoginQueryHandler(
                _userQuery.Object,
                _hashingService.Object,
                _configuration.Object);

            var request = new LoginRequest
            {
                Email = user.Email,
                Password = "1234"
            };

            var query = new LoginQuery(request);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(query, default));

            _userQuery.Verify(
                q => q.GetByEmail(user.Email),
                Times.Once);

            _hashingService.Verify(
                h => h.encryptSHA256("1234"),
                Times.Once);
        }
    }
}
