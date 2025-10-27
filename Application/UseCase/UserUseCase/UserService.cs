using Application.Interfaces.UserInterfaces;
using Application.Models.UserModels;

namespace Application.UseCase.UserUseCase
{
    public class UserService : IUserService
    {
        private readonly IUserCommand _userCommand;
        private readonly IUserQuery _userQuery;
        public UserService(IUserCommand userCommand, IUserQuery userQuery)
        {
            _userCommand = userCommand;
            _userQuery = userQuery;
        }
        public Task<UserResponseDTO> DeleteUser(UserRequestDTO user)
        {
            throw new NotImplementedException();
        }

        public async Task<List<UserResponseDTO>> GetAllUsers()
        {
            List<UserResponseDTO> users = await _userQuery.GetAllUsers();
            return users;
        }

        public async Task<UserResponseDTO> Login(UserLoginDTO login)
        {
            if ((login == null) || (login.Email == null) || (login.Password == null))
            {
                throw new ArgumentException("Bad Request");
            }
            return await _userQuery.Login(login.Email, login.Password);
        }

        public async Task<UserResponseDTO> RegisterUser(UserRequestDTO user)
        {
            if (_userQuery.ExistUser(user.Email).Result == true)
            {
                throw new ArgumentException("User already exist");
            }
            if ((user == null) || (user.Name == null) || (user.Email == null) || (user.Password == null)
                || (user.Phone == null) || (user.RoleId < 1) || (user.RoleId > 3) || (user.RoleId == null))
            {
                throw new ArgumentException("Bad Request");
            }
            return await _userCommand.InsertUser(user);
        }

        public Task<UserResponseDTO> UpdateUser(UserRequestDTO user)
        {
            throw new NotImplementedException();
        }
        
    }
}
