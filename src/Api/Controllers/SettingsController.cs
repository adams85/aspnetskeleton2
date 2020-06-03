using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Api.Helpers;
using WebApp.Api.Infrastructure.Security;
using WebApp.Common.Roles;
using WebApp.Service;
using WebApp.Service.Contract.Settings;
using WebApp.Service.Infrastructure;
using WebApp.Service.Settings;

namespace WebApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(SecurityService.ApiAuthorizationPolicy, Roles = nameof(RoleEnum.Administators))]
    public class SettingsController : Controller
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ISettingsAccessor _settingsAccessor;

        public SettingsController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, ISettingsAccessor settingsAccessor)
        {
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
            _settingsAccessor = settingsAccessor ?? throw new ArgumentNullException(nameof(settingsAccessor));
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
        public async Task<ActionResult<IEnumerable<SettingData>>> List([FromQuery] ListSettingsQuery model)
        {
            if (model == null)
                return BadRequest();

            model.MaxPageSize = _settingsAccessor.GetMaxPageSize();

            var result = await _queryDispatcher.DispatchAsync(model, HttpContext.RequestAborted);

            return Ok(result.Items ?? Enumerable.Empty<SettingData>());
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
