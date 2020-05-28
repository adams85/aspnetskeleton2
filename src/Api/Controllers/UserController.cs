using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Api.Infrastructure.Security;
using WebApp.Common.Roles;
using WebApp.Service.Infrastructure;
using WebApp.Service.Roles;
using WebApp.Service.Users;

namespace WebApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(SecurityService.ApiAuthorizationPolicy, Roles = nameof(RoleEnum.Administators))]
    public class UserController : Controller
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;

        public UserController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<UserData[]>> ListUsers([FromQuery] ListUsersQuery model)
        {
            var result = await _queryDispatcher.DispatchAsync(model, HttpContext.RequestAborted);

            return result.Items ?? Array.Empty<UserData>();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string username, string password, string email)
        {
            await _commandDispatcher.DispatchAsync(new CreateUserCommand
            {
                CreateProfile = true,
                Email = email,
                Password = password,
                UserName = username,
            }, HttpContext.RequestAborted);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser([FromBody] ApproveUserCommand model)
        {
            await _commandDispatcher.DispatchAsync(model, HttpContext.RequestAborted);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddUsersToRoles([FromBody] AddUsersToRolesCommand model)
        {
            await _commandDispatcher.DispatchAsync(model, HttpContext.RequestAborted);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUsersFromRolesCommand([FromBody] RemoveUsersFromRolesCommand model)
        {
            await _commandDispatcher.DispatchAsync(model, HttpContext.RequestAborted);

            return Ok();
        }
    }
}
