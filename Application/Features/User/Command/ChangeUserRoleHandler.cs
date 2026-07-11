using Application.Features.User.Command;
using Application.Interfaces.Query;
using Application.Interfaces.UserInterface;
using Application.Models.Response;
using MediatR;

namespace Application.Features.User.Handlers
{
    public class ChangeUserRoleHandler : IRequestHandler<ChangeUserRoleCommand, StatusResponse>
    {
        private readonly IUserCommand _userCommand;
        private readonly IUserQuery _userQuery;
        private readonly IRoleQuery _roleQuery;

        public ChangeUserRoleHandler(IUserCommand userCommand, IUserQuery userQuery, IRoleQuery roleQuery)
        {
            _userCommand = userCommand;
            _userQuery = userQuery;
            _roleQuery = roleQuery;
        }

        public async Task<StatusResponse> Handle(ChangeUserRoleCommand command, CancellationToken cancellationToken)
        {
            if (command.request.NewRole <= 0)
            {
                throw new ArgumentException("El role ingresado es inválido.");
            }

            if (!await _userQuery.ExistUser(command.userId))
                throw new KeyNotFoundException("Usuario no encontrado");

            var role = await _roleQuery.GetByIdAsync(command.request.NewRole);

            if (role is null)
            {
                throw new KeyNotFoundException("No se encontró el role ingresado.");
            }

            var success = await _userCommand.ChangeUserRole(command.userId, command.request.NewRole);

            if (!success)
                throw new InvalidOperationException("No se pudo actualizar el rol del usuario");

            return new StatusResponse
            {
                Message = "Rol actualizado correctamente a " + role.Name
            };
        }
    }
}
