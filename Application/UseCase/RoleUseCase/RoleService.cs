using Application.Interfaces.RoleInterfaces;
using Application.Models.RoleModels;

namespace Application.UseCase.RoleUseCase
{
    public class RoleService : IRoleService
    {
        private readonly IRoleQuery _roleQuery;
        public RoleService(IRoleQuery roleQuery)
        {
            _roleQuery = roleQuery;
        }
        public async Task<RoleResponseDTO> GetRoleById(int roleId)
        {
            if (_roleQuery.RoleExists(roleId).Result == false)
            {
                throw new ArgumentException("Role ID must be a positive integer.", nameof(roleId));
            }
            return await _roleQuery.GetRoleById(roleId);
        }
    }
}
