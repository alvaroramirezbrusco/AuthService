using RoleEntity = Domain.Entities.Role;
using Application.Interfaces.Query;
using Moq;
using Application.Features.Role.Query;

namespace AuthTest.Role.Query
{
    public class GetAllRolesHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsAllRoles()
        {
            var roleQuery = new Mock<IRoleQuery>();

            var roles = new List<RoleEntity>
            {
                new RoleEntity { RoleId = 1, Name = "Administrador" },
                new RoleEntity { RoleId = 2, Name = "Usuario" }
            };

            roleQuery
                .Setup(rq => rq.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(roles);

            var handler = new GetAllRolesHandler(roleQuery.Object);

            var result = await handler.Handle(new GetAllRolesQuery(), default);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            roleQuery.Verify(
                q => q.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
