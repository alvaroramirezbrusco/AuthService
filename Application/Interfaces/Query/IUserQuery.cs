using Application.Models.AuthModels.Login;
using Application.Models.UserModels;
using Domain.Entities;

namespace Application.Interfaces.Query
{
    public interface IUserQuery
    {
        Task<bool> IsEmailUnique(string email);
        Task<bool> ExistUser(Guid id);
        Task<User> GetById(Guid userId);
        Task<User> GetByEmail(string email);
        Task<List<UserResponseDTO>> GetAll();
    }
}
