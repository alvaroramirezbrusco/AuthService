using Application.Interfaces.RoleInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthServiceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetRoleById(int roleId)
        {
            var result = await _roleService.GetRoleById(roleId);
            return new JsonResult(result);
        }
    }
}
