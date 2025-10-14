using Application.Interfaces.AuthInterface;
using Application.Models.UserModel;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Commands.AuthCommand
{
    public class AuthCommands : IAuthCommand
    {
        private readonly AppDbContext _context;
        public AuthCommands(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserResponseDTO> Insert(UserRequestDTO user)
        {
            var userInsert = new User
            {
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                Password = user.Password,
                Phone = user.Phone,
                RoleId = user.RoleId
            };
            await _context.Users.Add(userInsert)
                .Reference(u => u.Role)
                .LoadAsync();
            await _context.SaveChangesAsync();
            return await MapToResponse(userInsert);
        }

        private async Task<UserResponseDTO> MapToResponse(User user)
        {
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
