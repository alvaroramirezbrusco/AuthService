using Application.Features.User.Command;
using Application.Features.User.Handlers;
using Application.Interfaces.Query;
using Application.Interfaces.UserInterface;
using Application.Models.Request;
using Moq;
using RoleEntity = Domain.Entities.Role;

namespace AuthTest.User.Command
{
    public class ChangeUserRoleTests
    {
        private readonly Mock<IUserCommand> _userCommand;
        private readonly Mock<IUserQuery> _userQuery;
        private readonly Mock<IRoleQuery> _roleQuery;

        public ChangeUserRoleTests()
        {
            _userCommand = new Mock<IUserCommand>();
            _userQuery = new Mock<IUserQuery>();
            _roleQuery = new Mock<IRoleQuery>();
        }

        [Fact]
        public async Task Handle_ReturnsStatus_WhenRoleIsUpdated()
        {
            var userId = Guid.NewGuid();
            var newRole = 1;

            _userQuery
                .Setup(q => q.ExistUser(userId))
                .ReturnsAsync(true);

            var role = new RoleEntity
            {
                RoleId = newRole,
                Name = "Administrador"
            };

            _roleQuery
                .Setup(q => q.GetByIdAsync(newRole, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            _userCommand
                .Setup(c => c.ChangeUserRole(userId, newRole))
                .ReturnsAsync(true);

            var handler = new ChangeUserRoleHandler(
                _userCommand.Object,
                _userQuery.Object,
                _roleQuery.Object);

            var request = new ChangeUserRoleRequest
            {
                NewRole = newRole
            };

            var command = new ChangeUserRoleCommand(
                userId,
                request);

            var result = await handler.Handle(command, default);

            Assert.NotNull(result);
            Assert.Equal(
                "Rol actualizado correctamente a Administrador",
                result.Message);

            _userQuery.Verify(
                q => q.ExistUser(userId),
                Times.Once);

            _roleQuery.Verify(
                q => q.GetByIdAsync(newRole, It.IsAny<CancellationToken>()),
                Times.Once);

            _userCommand.Verify(
                c => c.ChangeUserRole(userId, newRole),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ThrowsArgumentException_WhenNewRoleIsInvalid()
        {
            var userId = Guid.NewGuid();
            var invalidRole = 0;

            var handler = new ChangeUserRoleHandler(
                _userCommand.Object,
                _userQuery.Object,
                _roleQuery.Object);

            var request = new ChangeUserRoleRequest
            {
                NewRole = invalidRole
            };

            var command = new ChangeUserRoleCommand(
                userId,
                request);

            await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, default));

            _userQuery.Verify(
                q => q.ExistUser(userId),
                Times.Never);

            _roleQuery.Verify(
                q => q.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _userCommand.Verify(
                c => c.ChangeUserRole(userId, It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsKeyNotFoundException_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            var newRole = 1;

            _userQuery
                .Setup(q => q.ExistUser(userId))
                .ReturnsAsync(false);

            var handler = new ChangeUserRoleHandler(
                _userCommand.Object,
                _userQuery.Object,
                _roleQuery.Object);

            var request = new ChangeUserRoleRequest
            {
                NewRole = newRole
            };

            var command = new ChangeUserRoleCommand(
                userId,
                request);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, default));

            _userQuery.Verify(
                q => q.ExistUser(userId),
                Times.Once);

            _roleQuery.Verify(
                q => q.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _userCommand.Verify(
                c => c.ChangeUserRole(It.IsAny<Guid>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsKeyNotFoundException_WhenRoleDoesNotExist()
        {
            var userId = Guid.NewGuid();
            var newRole = 6;

            _userQuery
                .Setup(q => q.ExistUser(userId))
                .ReturnsAsync(true);

            _roleQuery
                .Setup(q => q.GetByIdAsync(newRole, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RoleEntity?)null);

            var handler = new ChangeUserRoleHandler(
                _userCommand.Object,
                _userQuery.Object,
                _roleQuery.Object);

            var request = new ChangeUserRoleRequest
            {
                NewRole = newRole
            };

            var command = new ChangeUserRoleCommand(
                userId,
                request);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, default));

            _userQuery.Verify(
                q => q.ExistUser(userId),
                Times.Once);

            _roleQuery.Verify(
                q => q.GetByIdAsync(newRole, It.IsAny<CancellationToken>()),
                Times.Once);

            _userCommand.Verify(
                c => c.ChangeUserRole(It.IsAny<Guid>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenRoleUpdateFails()
        {
            var userId = Guid.NewGuid();
            var newRole = 1;

            _userQuery
                .Setup(q => q.ExistUser(userId))
                .ReturnsAsync(true);

            var role = new RoleEntity
            {
                RoleId = newRole,
                Name = "Administrador"
            };

            _roleQuery
                .Setup(q => q.GetByIdAsync(newRole, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            _userCommand
                .Setup(c => c.ChangeUserRole(userId, newRole))
                .ReturnsAsync(false);

            var handler = new ChangeUserRoleHandler(
                _userCommand.Object,
                _userQuery.Object,
                _roleQuery.Object);

            var request = new ChangeUserRoleRequest
            {
                NewRole = newRole
            };

            var command = new ChangeUserRoleCommand(
                userId,
                request);

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
            
            _userQuery.Verify(
                q => q.ExistUser(userId),
                Times.Once);

            _roleQuery.Verify(
                q => q.GetByIdAsync(newRole, It.IsAny<CancellationToken>()),
                Times.Once);

            _userCommand.Verify(
                c => c.ChangeUserRole(userId, newRole),
                Times.Once);
        }
    }
}
