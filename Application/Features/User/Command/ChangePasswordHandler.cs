using Application.Features.User.Command;
using Application.Interfaces.HelperInterface;
using Application.Interfaces.Query;
using Application.Interfaces.UserInterface;
using Application.Models.Request;
using Application.Models.Response;
using MediatR;

namespace Application.Features.User.Handlers
{
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, StatusResponse>
    {
        private readonly IUserCommand _userCommand;
        private readonly IUserQuery _userQuery;
        private readonly IHashingService _hashingService;

        public ChangePasswordHandler(
            IUserCommand userCommand,
            IUserQuery userQuery,
            IHashingService hashingService)
        {
            _userCommand = userCommand;
            _userQuery = userQuery;
            _hashingService = hashingService;
        }

        public async Task<StatusResponse> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var request = command.request;
            var specialChars = new[] { "@", "_", "-", "$", "#", "&", "/", "|", "!", "%", "?", "=", "¡", "¿" };

            if (request == null)
                throw new ArgumentNullException(nameof(request), "El request no puede ser nulo");

            if (request.CurrentPassword == request.NewPassword)
                throw new ArgumentException("La nueva contraseña no puede ser igual a la actual");

            if (request.NewPassword.Length <= 8)
                throw new ArgumentException("La nueva contraseña debe tener más de 8 caracteres");

            if (!specialChars.Any(c => request.NewPassword.Contains(c)))
                throw new ArgumentException("La nueva contraseña debe contener caracteres especiales");

            var user = await _userQuery.GetById(request.UserId);
            if (user == null)
                throw new KeyNotFoundException("Usuario no encontrado");

            var hashedCurrent = _hashingService.encryptSHA256(request.CurrentPassword);

            if (!user.Password.Equals(hashedCurrent))
                throw new UnauthorizedAccessException("La contraseña actual es incorrecta");

            /*request.NewPassword = _hashingService.encryptSHA256(request.NewPassword);

            var success = await _userCommand.ChangePassword(request);*/

            var hashedNewPassword = _hashingService.encryptSHA256(request.NewPassword);

            var updateRequest = new ChangePasswordRequest
            {
                UserId = request.UserId,
                CurrentPassword = request.CurrentPassword,
                NewPassword = hashedNewPassword
            };

            var success = await _userCommand.ChangePassword(updateRequest);

            if (!success)
                throw new InvalidOperationException("Error al actualizar la contraseña");

            return new StatusResponse
            {
                Message = "Contraseña actualizada correctamente"
            };
        }
    }
}
