using Application.Models.UserModels;

namespace Application.Interfaces.UserInterfaces
{
    public interface IUserQuery
    {
        Task<UserResponseDTO> GetUser(Guid Id);
        Task<UserResponseDTO> Login(string email, string password);
        Task<List<UserResponseDTO>> GetAllUsers();
        Task<bool> ExistUser(string email);
    }
}
