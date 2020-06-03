using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Api.Infrastructure.Security;

namespace WebApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TokenController : Controller
    {
        private readonly ISecurityService _securityService;

        public TokenController(ISecurityService securityService)
        {
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        }

        /// <summary>
        /// Validates the specified user credentials and issues a JWT access token with a refresh token upon success.
        /// </summary>
        /// <remarks>
        /// The tokens are returned as HTTP headers (see <see cref="SecurityService.JwtAccessTokenHttpHeaderName"/> and <see cref="SecurityService.JwtRefreshTokenHttpHeaderName"/>).<br/>
        /// The short-lived access token can then be used to access protected endpoints, while the long-lived refresh token can be used to renew the access token.<br/>
        /// On expiration of the access token, repeat the request with the refresh token supplied as an appendix of the Authorization HTTP header
        /// in the form of "Bearer &lt;access-token&gt;; &lt;param_name&gt;=&lt;refresh-token&gt;" where param_name is <see cref="SecurityService.JwtRefreshTokenAuthorizationHeaderParamName"/>.<br/>
        /// Alternatively, the refresh token can be passed in each request to renew the tokens without an extra round-trip in case of expiration.
        /// </remarks>
        /// <param name="model">The user credentials.</param>
        /// <response code="200">The user credentials was valid. The response contains the tokens.</response>
        /// <response code="400">The user credentials was invalid. No tokens were issued.</response>
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
        [HttpPost]
        [AllowAnonymous]
        public new async Task<IActionResult> Request([FromBody] NetworkCredential model)
        {
            if (model == null)
                return BadRequest();

            var success = await _securityService.TryIssueJwtTokenAsync(model, HttpContext);
            if (!success)
                return BadRequest();

            return Ok();
        }
    }
}
