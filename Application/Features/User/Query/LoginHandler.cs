using Application.Interfaces.HelperInterface;
using Application.Interfaces.Query;
using Application.Interfaces.UserInterface;
using Application.Models.Response;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Features.User.Query
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, GenericResponse>
    {
        private readonly IUserQuery _userQuery;
        private readonly IHashingService _hash;
        private readonly IConfiguration _configuration;

        public LoginQueryHandler(IUserQuery userQuery, IHashingService hash, IConfiguration configuration)
        {
            _userQuery = userQuery;
            _hash = hash;
            _configuration = configuration;
        }

        public async Task<GenericResponse> Handle(LoginQuery query, CancellationToken cancellationToken)
        {
            var request = query.request;
            var Vmails = new[] { "@gmail.com", "@outlook.com", "@hotmail.com", "@yahoo.com" };

            if (string.IsNullOrWhiteSpace(request.Email) ||
                !Vmails.Any(vm => request.Email.Contains(vm)) ||
                request.Email.Any(char.IsUpper))
            {
                throw new ArgumentException("Ingrese un mail válido");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Ingrese una contraseña válida");

            var user = await _userQuery.GetByEmail(request.Email);

            if (user == null)
                throw new KeyNotFoundException("No se encontró el usuario.");

            var hashedInput = _hash.encryptSHA256(request.Password);

            if (user.Password != hashedInput)
                throw new ArgumentException("Contraseña Incorrecta.");

            string token = GenerateJwtToken(user);

            return new GenericResponse
            {
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                Token = token
            };
        }

        private string GenerateJwtToken(Domain.Entities.User user)
        {
            var claims = new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim("userRole", user.Role.Name),
                new Claim("Username", user.Name),
                new Claim("UserLastName", user.LastName),
                new Claim("UserPhone", user.Phone),
                new Claim("UserEmail", user.Email)
            };

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(90),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
