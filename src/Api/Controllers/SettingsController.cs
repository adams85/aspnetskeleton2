using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Api.Infrastructure.Security;
using WebApp.Common.Roles;
using WebApp.Service;
using WebApp.Service.Infrastructure;
using WebApp.Service.Settings;

namespace WebApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = ApiAuthenticationSchemes.CookieAndJwtBearer, Roles = nameof(RoleEnum.Administators))]
    public class SettingsController : Controller
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;

        public SettingsController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher, ISettingsProvider settingsProvider)
        {
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
        }

        /// <summary>
        /// Lists user-adjustable application settings.
        /// </summary>
        /// <response code="200">The response contains the requested data.</response>
        /// <response code="400">Some of the request parameters are missing or invalid.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have permission to execute this operation.</response>
        [ProducesResponseType(typeof(SettingData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceErrorData), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        [HttpGet]
        public async Task<ActionResult<ListResult<SettingData>>> List([FromQuery] ListSettingsQuery model)
        {
            if (model == null)
                return BadRequest();

            var result = await _queryDispatcher.DispatchAsync(model, HttpContext.RequestAborted);

            return result;
        }

        /// <summary>
        /// Updates a user-adjustable application setting.
        /// </summary>
        /// <response code="200">Operation was executed successfully.</response>
        /// <response code="400">Some of the request parameters are missing or invalid.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have permission to execute this operation.</response>
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceErrorData), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateSettingCommand model)
        {
            if (model == null)
                return BadRequest();

            await _commandDispatcher.DispatchAsync(model, HttpContext.RequestAborted);

            return Ok();
        }
    }
}
