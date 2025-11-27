using Application.Models.Request;
using Application.Models.Response;
using MediatR;

namespace Application.Features.User.Query
{
    public record LoginQuery(LoginRequest request) : IRequest<GenericResponse>; 
}
