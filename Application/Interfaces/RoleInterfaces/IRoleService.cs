using Application.Models.RoleModels;

namespace Application.Interfaces.RoleInterfaces
{
    public interface IRoleService
    {
        Task<RoleResponseDTO> GetRoleById(int roleId);
    }
}
