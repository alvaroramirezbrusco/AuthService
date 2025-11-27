using Application.Features.User.Query;
using Application.Interfaces.UserInterface;
using Application.Models.AuthModels.Login;
using Application.Models.AuthModels.Register;
using Application.Models.Request;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthServiceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMediator _mediator;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            var result = await _userService.RegisterUser(request);
            return new JsonResult(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _mediator.Send(new LoginQuery(request));
            return Ok(result);
        }


        [HttpPatch("change-password")]
        [Authorize(Roles = "Current,Admin,SuperAdmin")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "UserId claim not found." });
            }

            request.UserId = Guid.Parse(userIdClaim.Value);

            var result = await _userService.ChangePassword(request);

            if (!result)
            {
                return BadRequest(new { message = "Current password is incorrect." });
            }

            return Ok(new { message = "Password updated successfully." });
        }

        [HttpPatch("change-role/{userId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ChangeUserRole([FromRoute] Guid userId, [FromBody] ChangeUserRoleRequest request)
        {
            request.UserId = userId;
            var result = await _userService.ChangeUserRole(request);
            return new JsonResult(result);
        }

        [HttpGet("users")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userService.GetAll();
            return new JsonResult(result);
        }
    }
}
