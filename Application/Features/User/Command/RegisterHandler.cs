using Application.Exceptions;
using Application.Features.User.Command;
using Application.Interfaces.HelperInterface;
using Application.Interfaces.Query;
using Application.Interfaces.UserInterface;
using Application.Models.Response;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Features.User.Handler
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, GenericResponse>
    {
        private readonly IUserCommand _userCommand;
        private readonly IUserQuery _userQuery;
        private readonly IHashingService _hash;
        private readonly IConfiguration _configuration;

        public RegisterHandler(IUserCommand userCommand,
                               IUserQuery userQuery,
                               IHashingService hash,
                               IConfiguration configuration)
        {
            _userCommand = userCommand;
            _userQuery = userQuery;
            _hash = hash;
            _configuration = configuration;
        }

        public async Task<GenericResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var model = request.request;

            var carac = new[] { "@", "_", "-", "$", "#", "&", "/", "|", "!", "%", "?", "=", "¡", "¿" };
            var Vmails = new[] { "@gmail.com", "@outlook.com", "@hotmail.com", "@yahoo.com" };

            if (model == null ||
                string.IsNullOrEmpty(model.Email) ||
                string.IsNullOrEmpty(model.Password) ||
                string.IsNullOrEmpty(model.Name) ||
                string.IsNullOrEmpty(model.Phone))
                throw new ArgumentException("Complete los campos solicitados...");

            if (string.IsNullOrWhiteSpace(model.Email) ||
                !Vmails.Any(vm => model.Email.Contains(vm)) ||
                model.Email.Any(char.IsUpper))
            {
                throw new ArgumentException("Ingrese un mail válido");
            }

            if (model.Password.Length <= 8 || !carac.Any(c => model.Password.Contains(c)))
                throw new ArgumentException("Contraseña insegura");

            if (model.Phone.Length <= 5 || model.Phone.Length >= 16)
                throw new ArgumentException("Ingrese un número de teléfono válido");

            if (string.IsNullOrWhiteSpace(model.Email) ||
                !Vmails.Any(vm => model.Email.Contains(vm)) ||
                model.Email.Any(char.IsUpper))
            {
                throw new ArgumentException("Ingrese un mail válido");
            }

            var isUnique = await _userQuery.IsEmailUnique(model.Email);
            if (!isUnique)
                throw new ConflictException("El email ya está registrado");

            var hashedPass = _hash.encryptSHA256(model.Password);

            var newUser = await _userCommand.InsertUser(new Domain.Entities.User
            {
                Name = model.Name,
                LastName = model.LastName,
                Email = model.Email,
                Password = hashedPass,
                Phone = model.Phone,
                RoleId = 1
            });

            var token = GenerateJwtToken(new LoginResponseDTO
            {
                Id = newUser.Id,
                Username = newUser.Name,
                UserLastName = newUser.LastName,
                UserPhone = newUser.Phone,
                Email = newUser.Email,
                RoleName = "Current"
            });

            return new GenericResponse
            {
                Name = newUser.Name,
                LastName = newUser.LastName,
                Email = newUser.Email,
                Token = token
            };
        }

        private string GenerateJwtToken(LoginResponseDTO user)
        {
            var claims = new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim("userRole", user.RoleName),
                new Claim("Username", user.Username),
                new Claim("UserLastName", user.UserLastName),
                new Claim("UserPhone", user.UserPhone),
                new Claim("UserEmail", user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(90),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
