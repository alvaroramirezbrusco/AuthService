using Application.Interfaces.UserInterfaces;
using Application.Models.UserModels;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Querys.UserQuery
{
    public class UserQuery : IUserQuery
    {
        private readonly AppDbContext _context;
        public UserQuery(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistUser(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email.ToLower());
        }

        public async Task<List<UserResponseDTO>> GetAllUsers()
        {
            var users = await _context.Users.Include(u => u.Role).Select(user => new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                Password = user.Password,
                Phone = user.Phone,
                RoleId = user.RoleId,
                RoleName = user.Role.Name
            }).ToListAsync();
            return users;
        }

        public async Task<UserResponseDTO> GetUser(Guid Id)
        {
            User user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == Id);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }
            UserResponseDTO userResponse = new UserResponseDTO
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
            return userResponse;
        }

        public async Task<UserResponseDTO> Login(string email, string password)
        {
            User user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            if (user == null)
            {
                throw new ArgumentException("Invalid email or password");
            }
            UserResponseDTO userResponse = new UserResponseDTO
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
            return userResponse;
        }
    }
}
