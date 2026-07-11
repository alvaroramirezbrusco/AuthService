using Application.Interfaces.Query;
using Moq;
using UserEntity = Domain.Entities.User;
using RoleEntity = Domain.Entities.Role;
using Application.Features.User.Handlers;
using Application.Features.User.Query;

namespace AuthTest.User.Query
{
    public class GetAllUsersHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsAllUsers()
        {
            var userQuery = new Mock<IUserQuery>();

            var users = new List<UserEntity>
            {
                new UserEntity {
                    Id = Guid.NewGuid(),
                    Name = "John",
                    LastName = "Doe",
                    Email = "johndoe@mail.com",
                    Role = new RoleEntity
                    {
                        RoleId = 1,
                        Name = "Administrador"
                    }
                },
                new UserEntity {
                    Id = Guid.NewGuid(),
                    Name = "Jane",
                    LastName = "Smith",
                    Email = "janesmith@gmail.com",
                    Role = new RoleEntity
                    {
                        RoleId = 2,
                        Name = "Usuario"
                    }
                }
            };

            userQuery
                .Setup(x => x.GetAll())
                .ReturnsAsync(users);

            var handler = new GetAllUsersHandler(userQuery.Object);

            var result = await handler.Handle(new GetAllUsersQuery(), default);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            userQuery.Verify(
                x => x.GetAll(),
                Times.Once);
        }
    }
}
