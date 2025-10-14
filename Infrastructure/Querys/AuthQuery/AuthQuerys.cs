using Application.Interfaces.AuthInterface;
using Application.Models.UserModel;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Querys.AuthQuery
{
    public class AuthQuerys : IAuthQuery
    {
        private readonly AppDbContext _context;
        public AuthQuerys(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserResponseDTO> Get(LoginUserDTO login)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == login.Email && u.Password == login.Password);
            if (user == null)
            {
                throw new ArgumentException("Usuario no encontrado");
                return null;
            }
            var userResponse = new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                Password = user.Password,
                Phone = user.Phone,
                RoleId = user.RoleId,
                RoleName = user.Role.Name
            };
            return await Task.FromResult(userResponse);

        }

        public async Task<UserResponseDTO> GetById(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                throw new ArgumentException("Usuario no encontrado");
                return null;
            }
            var userResponse = new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                Password = user.Password,
                Phone = user.Phone,
                RoleId = user.RoleId,
                RoleName = user.Role.Name
            };
            return await Task.FromResult(userResponse);
        }
    }
}
