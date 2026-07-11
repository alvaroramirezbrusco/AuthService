using RoleEntity = Domain.Entities.Role;
using Application.Features.Role.Query;
using Application.Interfaces.Query;
using Moq;

namespace AuthTest.Role.Query
{
    public class GetRoleByIdHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsRole()
        {
            var roleQuery = new Mock<IRoleQuery>();

            var role = new RoleEntity
            {
                RoleId = 1,
                Name = "Administrador"
            };

            roleQuery
                .Setup(q => q.GetByIdAsync(role.RoleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            var handler = new GetRoleByIdHandler(roleQuery.Object);

            var result = await handler.Handle(new GetRoleByIdQuery(role.RoleId), It.IsAny<CancellationToken>());

            Assert.NotNull(result);
            Assert.Equal(role.RoleId, result.Id);
            Assert.Equal(role.Name, result.Name);

            roleQuery.Verify(q => q.GetByIdAsync(role.RoleId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidId_Throws()
        {
            var roleQuery = new Mock<IRoleQuery>();

            var handler = new GetRoleByIdHandler(roleQuery.Object);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(new GetRoleByIdQuery(0), default));

            roleQuery.Verify(
                q => q.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_RoleNotFound_Throws()
        {
            var roleQuery = new Mock<IRoleQuery>();

            roleQuery
                .Setup(q => q.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RoleEntity?)null);

            var handler = new GetRoleByIdHandler(roleQuery.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(new GetRoleByIdQuery(1), default));

            roleQuery.Verify(
                q => q.GetByIdAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
