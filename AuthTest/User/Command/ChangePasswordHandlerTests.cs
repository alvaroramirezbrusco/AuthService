using Application.Features.User.Command;
using Application.Features.User.Handlers;
using Application.Interfaces.HelperInterface;
using Application.Interfaces.Query;
using Application.Interfaces.UserInterface;
using Application.Models.Request;
using Moq;
using UserEntity = Domain.Entities.User;

namespace AuthTest.User.Command
{
    public class ChangePasswordHandlerTests
    {
        private readonly Mock<IUserCommand> _userCommand;
        private readonly Mock<IUserQuery> _userQuery;
        private readonly Mock<IHashingService> _hashingService;

        public ChangePasswordHandlerTests()
        {
            _userCommand = new Mock<IUserCommand>();
            _userQuery = new Mock<IUserQuery>();
            _hashingService = new Mock<IHashingService>();
        }

        [Fact]
        public async Task Handle_ReturnsStatus_WhenPasswordIsChanged()
        {
            var userId = Guid.NewGuid();

            const string currentPassword = "Password123!";
            const string newPassword = "NewPassword123!";

            var user = new UserEntity
            {
                Id = userId,
                Name = "John",
                LastName = "Smith",
                Email = "johnsmith@gmail.com",
                Password = "CURRENT_HASH"
            };

            _userQuery
                .Setup(q => q.GetById(userId))
                .ReturnsAsync(user);

            _hashingService
                .Setup(h => h.encryptSHA256(currentPassword))
                .Returns("CURRENT_HASH");

            _hashingService
                .Setup(h => h.encryptSHA256(newPassword))
                .Returns("NEW_HASH");

            _userCommand
                .Setup(c => c.ChangePassword(It.IsAny<ChangePasswordRequest>()))
                .ReturnsAsync(true);

            var handler = new ChangePasswordHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object);

            var request = new ChangePasswordRequest
            {
                UserId = userId,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            var command = new ChangePasswordCommand(request);

            var result = await handler.Handle(command, default);

            Assert.NotNull(result);
            Assert.Equal("Contraseña actualizada correctamente", result.Message);

            _userQuery.Verify(
                q => q.GetById(userId),
                Times.Once);

            _hashingService.Verify(
                h => h.encryptSHA256(currentPassword),
                Times.Once);

            _hashingService.Verify(
                h => h.encryptSHA256(newPassword),
                Times.Once);

            _userCommand.Verify(
                c => c.ChangePassword(It.IsAny<ChangePasswordRequest>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NullRequest_Throws()
        {
            var handler = new ChangePasswordHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object);

            var command = new ChangePasswordCommand(null!);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                handler.Handle(command, default));

            _userQuery.Verify(
                q => q.GetById(It.IsAny<Guid>()),
                Times.Never);

            _hashingService.Verify(
                h => h.encryptSHA256(It.IsAny<string>()),
                Times.Never);

            _userCommand.Verify(
                c => c.ChangePassword(It.IsAny<ChangePasswordRequest>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsArgumentException_WhenNewPasswordIsSameAsCurrent()
        {
            var userId = Guid.NewGuid();
            const string password = "Password123!";

            var request = new ChangePasswordRequest
            {
                UserId = userId,
                CurrentPassword = password,
                NewPassword = password
            };

            var command = new ChangePasswordCommand(request);

            var handler = new ChangePasswordHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, default));

            _userQuery.Verify(
                q => q.GetById(userId),
                Times.Never);

            _hashingService.Verify(
                h => h.encryptSHA256(It.IsAny<string>()),
                Times.Never);

            _userCommand.Verify(
                c => c.ChangePassword(It.IsAny<ChangePasswordRequest>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsArgumentException_WhenPasswordIsTooShort()
        {
            var userId = Guid.NewGuid();
            const string currentPassword = "Password123!";
            const string newPassword = "Short1!";

            var request = new ChangePasswordRequest
            {
                UserId = userId,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            var command = new ChangePasswordCommand(request);

            var handler = new ChangePasswordHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, default));

            _userQuery.Verify(
                q => q.GetById(userId),
                Times.Never);

            _hashingService.Verify(
                h => h.encryptSHA256(It.IsAny<string>()),
                Times.Never);

            _userCommand.Verify(
                c => c.ChangePassword(It.IsAny<ChangePasswordRequest>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsArgumentException_WhenPasswordHasNoSpecialCharacters()
        {
            var userId = Guid.NewGuid();
            const string currentPassword = "Password123!";
            const string newPassword = "Password123";

            var request = new ChangePasswordRequest
            {
                UserId = userId,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            var command = new ChangePasswordCommand(request);

            var handler = new ChangePasswordHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, default));

            _userQuery.Verify(
                q => q.GetById(userId),
                Times.Never);

            _hashingService.Verify(
                h => h.encryptSHA256(It.IsAny<string>()),
                Times.Never);

            _userCommand.Verify(
                c => c.ChangePassword(It.IsAny<ChangePasswordRequest>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsKeyNotFoundException_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            const string currentPassword = "Password123!";
            const string newPassword = "NewPassword123!";

            _userQuery
                .Setup(q => q.GetById(userId))
                .ReturnsAsync((UserEntity?)null);

            var request = new ChangePasswordRequest
            {
                UserId = userId,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            var command = new ChangePasswordCommand(request);

            var handler = new ChangePasswordHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, default));

            _userQuery.Verify(
                q => q.GetById(userId),
                Times.Once);

            _hashingService.Verify(
                h => h.encryptSHA256(It.IsAny<string>()),
                Times.Never);

            _userCommand.Verify(
                c => c.ChangePassword(It.IsAny<ChangePasswordRequest>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsUnauthorizedAccessException_WhenCurrentPasswordIsIncorrect()
        {
            var userId = Guid.NewGuid();

            const string currentPassword = "Password123!";
            const string newPassword = "NewPassword123!";

            var user = new UserEntity
            {
                Id = userId,
                Password = "HASH_CORRECTO"
            };

            _userQuery
                .Setup(q => q.GetById(userId))
                .ReturnsAsync(user);

            _hashingService
                .Setup(h => h.encryptSHA256(currentPassword))
                .Returns("HASH_INCORRECTO");

            var handler = new ChangePasswordHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object);

            var request = new ChangePasswordRequest
            {
                UserId = userId,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            var command = new ChangePasswordCommand(request);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(command, default));

            _userQuery.Verify(
                q => q.GetById(userId),
                Times.Once);

            _hashingService.Verify(
                h => h.encryptSHA256(currentPassword),
                Times.Once);

            _userCommand.Verify(
                c => c.ChangePassword(It.IsAny<ChangePasswordRequest>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsInvalidOperationException_WhenChangePasswordFails()
        {
            var userId = Guid.NewGuid();
            const string currentPassword = "Password123!";
            const string newPassword = "NewPassword123!";

            var user = new UserEntity
            {
                Id = userId,
                Password = "HASH_CORRECTO"
            };

            _userQuery
                .Setup(q => q.GetById(userId))
                .ReturnsAsync(user);

            _hashingService
                .Setup(h => h.encryptSHA256(currentPassword))
                .Returns("HASH_CORRECTO");

            _hashingService
                .Setup(h => h.encryptSHA256(newPassword))
                .Returns("HASH_NUEVO");

            _userCommand
                .Setup(c => c.ChangePassword(It.IsAny<ChangePasswordRequest>()))
                .ReturnsAsync(false);

            var handler = new ChangePasswordHandler(
                _userCommand.Object,
                _userQuery.Object,
                _hashingService.Object);

            var request = new ChangePasswordRequest
            {
                UserId = userId,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            var command = new ChangePasswordCommand(request);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(command, default));

            _userQuery.Verify(
                q => q.GetById(userId),
                Times.Once);

            _hashingService.Verify(
                h => h.encryptSHA256(currentPassword),
                Times.Once);

            _hashingService.Verify(
                h => h.encryptSHA256(newPassword),
                Times.Once);

            _userCommand.Verify(
                c => c.ChangePassword(It.IsAny<ChangePasswordRequest>()),
                Times.Once);
        }
    }
}
