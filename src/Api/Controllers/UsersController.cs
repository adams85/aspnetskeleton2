using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Api.Infrastructure.Security;
using WebApp.Common.Roles;
using WebApp.Service.Infrastructure;
using WebApp.Service.Roles;
using WebApp.Service.Settings;
using WebApp.Service.Users;

namespace WebApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(ApiSecurityService.ApiAuthorizationPolicy, Roles = nameof(RoleEnum.Administators))]
    public class UsersController : Controller
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;

        public UsersController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher, ISettingsProvider settingsProvider)
        {
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<UserData[]>> List([FromQuery] ListUsersQuery model)
        {
            if (model == null)
                return BadRequest();

            var result = await _queryDispatcher.DispatchAsync(model, HttpContext.RequestAborted);

            return result.Items ?? Array.Empty<UserData>();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string username, string password, string email)
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
        public async Task<IActionResult> Approve([FromBody] ApproveUserCommand model)
        {
            if (model == null)
                return BadRequest();

            await _commandDispatcher.DispatchAsync(model, HttpContext.RequestAborted);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddUsersToRoles([FromBody] AddUsersToRolesCommand model)
        {
            if (model == null)
                return BadRequest();

            await _commandDispatcher.DispatchAsync(model, HttpContext.RequestAborted);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUsersFromRolesCommand([FromBody] RemoveUsersFromRolesCommand model)
        {
            if (model == null)
                return BadRequest();

            await _commandDispatcher.DispatchAsync(model, HttpContext.RequestAborted);

            return Ok();
        }
    }
}
