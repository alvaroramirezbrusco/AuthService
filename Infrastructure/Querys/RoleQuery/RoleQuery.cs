using Application.Interfaces.RoleInterfaces;
using Application.Models.RoleModels;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Querys.RoleQuery
{
    public class RoleQuery : IRoleQuery
    {
        private readonly AppDbContext _context;
        public RoleQuery(AppDbContext context)
        {
            _context = context;
        }
        public async Task<RoleResponseDTO> GetRoleById(int roleId)
        {
            Role role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (role == null)
            {
                return null;
            }
            return new RoleResponseDTO
            {
                RoleId = role.RoleId,
                Name = role.Name
            };
        }

        public async Task<bool> RoleExists(int roleId)
        {
            return await _context.Roles.AnyAsync(r => r.RoleId == roleId);
        }
    }
}
