using Application.Interfaces.HelperInterface;
using Application.Interfaces.Query;
using Application.Interfaces.UserInterface;
using Application.Models.AuthModels.Login;
using Application.Models.AuthModels.Register;
using Application.Models.Request;
using Application.Models.UserModels;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.UseCase.UserUseCase
{
    public class UserService : IUserService
    {
        private readonly IUserCommand _userCommand;
        private readonly IUserQuery _userQuery;
        private readonly IConfiguration _configuration;
        private readonly IHashingService _hash;
        public UserService(IUserCommand userCommand, IUserQuery userQuery, IConfiguration configuration,
            IHashingService hash)
        {
            _userCommand = userCommand;
            _userQuery = userQuery;
            _configuration = configuration;
            _hash = hash;
        }

        public async Task<bool> ChangePassword(ChangePasswordRequest request)
        {
            var carac = new string[] { "@", "_", "-", "$", "#", "&", "/", "|", "!", "%", "?", "=", "¡", "¿" };
            if (request == null) throw new ArgumentNullException("El request no puede ser nulo");
            if (request.CurrentPassword == request.NewPassword)
                throw new ArgumentException("La nueva contraseña no puede ser igual a la actual");
            if (request.NewPassword.Length <= 8)
                throw new ArgumentException("Ingrese una contraseña segura (Que contenga más de 8 caracteres)");
            if (!carac.Any(c => request.NewPassword.Contains(c)))
                throw new ArgumentException("La contraseña debe contener caracteres especiales. Ejemplo: @, _, -, $, #, &, /)");

            request.CurrentPassword = _hash.encryptSHA256(request.CurrentPassword);
            request.NewPassword = _hash.encryptSHA256(request.NewPassword);

            return await _userCommand.ChangePassword(request);
        }

        public async Task<bool> ChangeUserRole(ChangeUserRoleRequest request)
        {
            if (await _userQuery.ExistUser(request.UserId) == false)
            {
                throw new KeyNotFoundException("No se encontró el usuario.");
            }
            return await _userCommand.ChangeUserRole(request);
        }

        public async Task<List<UserResponseDTO>> GetAll()
        {
            return await _userQuery.GetAll();
        }

        public async Task<string> LoginUser(LoginDTO request)
        {
            var Vmails = new string[] { "@gmail.com", "@outlook.com", "@hotmail.com", "@yahoo.com" };

            if (string.IsNullOrWhiteSpace(request.Email) || !Vmails.Any(vm=>request.Email.Contains(vm)) || request.Email.Any(char.IsUpper))
            {
                throw new ArgumentException("Ingrese un mail válido");
            }


            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Ingrese una contraseña válida");
            }

            var user = await _userQuery.GetByEmail(request.Email);

            if (user == null)
            {
                throw new KeyNotFoundException("No se encontró el usuario.");
            }
            
            var hashedInput = _hash.encryptSHA256(request.Password);

            if (user.Password != hashedInput)
                throw new ArgumentException("Contraseña Incorrecta.");

            var userDto=new LoginResponseDTO
            {
                Id = user.Id,
                Email = user.Email,
                Username=user.Name,
                UserLastName = user.LastName,
                UserPhone = user.Phone,
                RoleName = user.Role.Name,
                Password = user.Password
            };



            string token = GenerateJwtToken(new LoginResponseDTO
            {
                Id = userDto.Id,
                Username=userDto.Username,
                UserLastName=userDto.UserLastName,
                UserPhone=userDto.UserPhone,
                Email = userDto.Email,
                RoleName = userDto.RoleName
            });

            return token;

        }

        public async Task<RegisterResponseDTO> RegisterUser(RegisterRequestDTO request)
        {
            var carac = new string[] {"@","_","-","$","#","&","/","|","!", "%", "?", "=","¡","¿" };
            var Vmails = new string[] { "@gmail.com", "@outlook.com", "@hotmail.com", "@yahoo.com" };
           
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Phone))
            {
                throw new ArgumentException("Complete los campos solicitados...");
            }

            if (!Vmails.Any(Vm=>request.Email.Contains(Vm))) 
            {
                throw new ArgumentException("Ingrese un Email válido");
            }
            if (request.Password.Length <= 8) 
            {
                throw new ArgumentException("Ingrese una contraseña segura (Que contenga más de 8 caracteres)");
            }
            if (!carac.Any(c=>request.Password.Contains(c)))
            {
                throw new ArgumentException("La contraseña debe contener caracteres especiales. Ejemplo: @, _, -, $, #, &, /)");
            }
            if (request.Phone.Length <= 5 || request.Phone.Length >= 16) 
            {
                throw new ArgumentException("Ingrese un numero de telefono válido");
            }

            User user = await _userCommand.InsertUser(new User
            {
                Name = request.Name,
                LastName = request.LastName,
                Email = request.Email,
                Password = _hash.encryptSHA256(request.Password),
                Phone = request.Phone,
                RoleId = 1
            });
            return new RegisterResponseDTO
            {
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email
            };
        }
        private string GenerateJwtToken(LoginResponseDTO user)
        {
            var userClaims = new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim("userRole", user.RoleName),
                new Claim("Username", user.Username),
                new Claim("UserLastName",user.UserLastName),
                new Claim("UserPhone",user.UserPhone),
                new Claim("UserEmail",user.Email)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jwtConfig = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    claims: userClaims,
                    expires: DateTime.Now.AddMinutes(90),
                    signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtConfig);
        }
    }
}
