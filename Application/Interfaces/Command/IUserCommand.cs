using Application.Models.Request;
using Domain.Entities;

namespace Application.Interfaces.UserInterface
{
    public interface IUserCommand
    {
        Task<User> InsertUser(User user);
        Task<bool> ChangePassword(ChangePasswordRequest request);
        Task<bool> ChangeUserRole(ChangeUserRoleRequest request);
    }
}
