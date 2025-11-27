using Application.Models.AuthModels.Login;
using Application.Models.AuthModels.Register;
using Application.Models.Request;
using Application.Models.UserModels;
using Domain.Entities;

namespace Application.Interfaces.UserInterface
{
    public interface IUserService
    {
        Task<RegisterResponseDTO> RegisterUser(RegisterRequestDTO request);
        Task<string> LoginUser(LoginDTO request);
        Task<bool> ChangePassword(ChangePasswordRequest request);
        Task<bool> ChangeUserRole(ChangeUserRoleRequest request);
        Task<List<UserResponseDTO>> GetAll();
    }
}
