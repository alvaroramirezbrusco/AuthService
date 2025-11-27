using Application.Interfaces.UserInterface;
using Application.Models.AuthModels.Register;
using Application.Models.Request;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Commands.UserCommand
{
    public class UserCommand : IUserCommand
    {
        private readonly AppDbContext _context;
        public UserCommand(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ChangePassword(ChangePasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);

            if (user == null)
            {
                return false;
            }

            var dbBytes = Encoding.UTF8.GetBytes(user.Password);
            var reqBytes = Encoding.UTF8.GetBytes(request.CurrentPassword);

            if (!user.Password.Trim().Equals(request.CurrentPassword.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            user.Password = request.NewPassword;
            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangeUserRole(ChangeUserRoleRequest request)
        {
            var user =  await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            user.RoleId = request.NewRole;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User> InsertUser(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

    }
}
