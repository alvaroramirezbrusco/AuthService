using Application.Exceptions;
using Application.Features.User.Command;
using Application.Features.User.Handler;
using Application.Interfaces.HelperInterface;
using Application.Interfaces.Query;
using Application.Interfaces.UserInterface;
using Application.Models.Request;
using Microsoft.Extensions.Configuration;
using Moq;
using RoleEntity = Domain.Entities.Role;
using UserEntity = Domain.Entities.User;

namespace AuthTest.User.Command
{
    public class RegisterHandlerTests
    {
        private readonly Mock<IUserCommand> _userCommand;
        private readonly Mock<IUserQuery> _userQuery;
        private readonly Mock<IHashingService> _hashingService;
        private readonly Mock<IConfiguration> _configuration;

        public RegisterHandlerTests()
        {
            _userCommand = new Mock<IUserCommand>();
            _userQuery = new Mock<IUserQuery>();
            _hashingService = new Mock<IHashingService>();
            _configuration = new Mock<IConfiguration>();
        }

        [Fact]
        public async Task Handle_ReturnsToken_WhenRegistrationIsSuccessful()
        {
            _configuration
                .Setup(c => c["Jwt:Key"])
                .Returns("aVeryLongSuperSecretKeyOfAtLeast32Chars!");

            _configuration
                .Setup(c => c["Jwt:Issuer"])
                .Returns("AuthService");

            var password = "Password123!";

            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Smith",
                Email = "johnsmith@gmail.com",
                Password = password,
                Phone = "123456789",
                Role = new RoleEntity
                {
                    RoleId = 2,
                    Name = "Usuario"
                }
            };

            _userQuery
                .Setup(q => q.IsEmailUnique(user.Email))
                .ReturnsAsync(true);

            _hashingService
                .Setup(h => h.encryptSHA256(password))
                .Returns("HASH123");

            _userCommand
                .Setup(c => c.InsertUser(It.IsAny<UserEntity>()))
                .ReturnsAsync(user);

            var handler = new RegisterHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object,
                _configuration.Object);

            var request = new RegisterRequest
            {
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                Password = password,
                Phone = user.Phone
            };

            var command = new RegisterCommand(request);

            var result = await handler.Handle(command, default);

            Assert.NotNull(result);
            Assert.Equal(user.Name, result.Name);
            Assert.Equal(user.LastName, result.LastName);
            Assert.Equal(user.Email, result.Email);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));

            _userQuery.Verify(
                q => q.IsEmailUnique(user.Email),
                Times.Once);

            _hashingService.Verify(
                h => h.encryptSHA256(password),
                Times.Once);

            _userCommand.Verify(
                c => c.InsertUser(It.IsAny<UserEntity>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyFields_Throws()
        {
            var handler = new RegisterHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object,
                _configuration.Object);

            var password = "Password123!";

            var request = new RegisterRequest
            {
                Name = string.Empty,
                LastName = "Smith",
                Email = "johnsmith@gmail.com",
                Password = password,
                Phone = "123456789"
            };

            var command = new RegisterCommand(request);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(command, default));

            _userQuery.Verify(
                q => q.IsEmailUnique(It.IsAny<string>()),
                Times.Never);

            _hashingService.Verify(
                h => h.encryptSHA256(It.IsAny<string>()),
                Times.Never);

            _userCommand.Verify(
                c => c.InsertUser(It.IsAny<UserEntity>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidEmail_Throws()
        {
            var handler = new RegisterHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object,
                _configuration.Object);

            var password = "Password123!";

            var request = new RegisterRequest
            {
                Name = "John",
                LastName = "Smith",
                Email = "Johnsmith",
                Password = password,
                Phone = "123456789"
            };

            var command = new RegisterCommand(request);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(command, default));

            _userQuery.Verify(
                q => q.IsEmailUnique(It.IsAny<string>()),
                Times.Never);

            _hashingService.Verify(
                h => h.encryptSHA256(It.IsAny<string>()),
                Times.Never);

            _userCommand.Verify(
                c => c.InsertUser(It.IsAny<UserEntity>()),
                Times.Never);
        }
        
        [Fact]
        public async Task Handle_WeakPassword_Throws()
        {
            var handler = new RegisterHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object,
                _configuration.Object);

            var password = "admin";

            var request = new RegisterRequest
            {
                Name = "John",
                LastName = "Smith",
                Email = "johnsmith@gmail.com",
                Password = password,
                Phone = "123456789"
            };

            var command = new RegisterCommand(request);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(command, default));

            _userQuery.Verify(
                q => q.IsEmailUnique(It.IsAny<string>()),
                Times.Never);

            _hashingService.Verify(
                h => h.encryptSHA256(It.IsAny<string>()),
                Times.Never);

            _userCommand.Verify(
                c => c.InsertUser(It.IsAny<UserEntity>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidPhone_Throws()
        {
            var handler = new RegisterHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object,
                _configuration.Object);

            var password = "Password123!";

            var request = new RegisterRequest
            {
                Name = "John",
                LastName = "Smith",
                Email = "johnsmith@gmail.com",
                Password = password,
                Phone = "123"
            };

            var command = new RegisterCommand(request);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(command, default));

            _userQuery.Verify(
                q => q.IsEmailUnique(It.IsAny<string>()),
                Times.Never);

            _hashingService.Verify(
                h => h.encryptSHA256(It.IsAny<string>()),
                Times.Never);

            _userCommand.Verify(
                c => c.InsertUser(It.IsAny<UserEntity>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_EmailAlreadyExists_Throws()
        {
            var handler = new RegisterHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object,
                _configuration.Object);

            var password = "Password123!";

            var request = new RegisterRequest
            {
                Name = "John",
                LastName = "Smith",
                Email = "johnsmith@gmail.com",
                Password = password,
                Phone = "123456789"
            };

            _userQuery
                .Setup(q => q.IsEmailUnique(request.Email))
                .ReturnsAsync(false);

            var command = new RegisterCommand(request);

            await Assert.ThrowsAsync<ConflictException>(() =>
                handler.Handle(command, default));

            _userQuery.Verify(
                q => q.IsEmailUnique(request.Email),
                Times.Once);

            _hashingService.Verify(
                h => h.encryptSHA256(It.IsAny<string>()),
                Times.Never);

            _userCommand.Verify(
                c => c.InsertUser(It.IsAny<UserEntity>()),
                Times.Never);
        }
    }
}